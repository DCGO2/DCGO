using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Ryutaro Williams
namespace DCGO.CardEffects.EX11
{
    public class EX11_056 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start Of Your Turn

            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }

            #endregion

            #region Your turns

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Hatch and digivolve in breeding", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When any of your Digimon digivolve into a level 5 or higher Digimon with [Tyrannomon] in its name or the [Dinosaur] trait, by suspending this Tamer, you may hatch in your breeding area. After, 1 of your Digimon in the breeding area may digivolve into a Digimon card with [Tyrannomon] in its name or the [Reptile] or [Dinosaur] trait in the hand without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasLevel
                        && permanent.TopCard.Level >= 5
                        && (permanent.TopCard.ContainsCardName("Tyrannomon") 
                            || permanent.TopCard.EqualsTraits("Dinosaur"));
                }

                bool DigivolveToCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && (cardSource.ContainsCardName("Tyrannomon")
                        || cardSource.EqualsTraits("Dinosaur")
                            || cardSource.EqualsTraits("Reptile"))
                        && cardSource.CanPlayCardTargetFrame(
                                   frame: card.Owner.GetBreedingAreaPermanents()[0].PermanentFrame,
                                   PayCost: false,
                                   cardEffect: activateClass,
                                   root: SelectCardEffect.Root.Hand,
                                   isBreedingArea: true);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(
                        new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    if (card.Owner.CanHatch)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Hatch", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Not hatch", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Will you hatch in Breeding Area?";
                        string notSelectPlayerMessage = "The opponent is selecting whether to hatch in Breeding Area.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool doHatch = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (doHatch)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new HatchDigiEggClass(player: card.Owner, hashtable: CardEffectCommons.CardEffectHashtable(activateClass)).Hatch());
                        }
                    }

                    if (card.Owner.GetBreedingAreaPermanents().Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: card.Owner.GetBreedingAreaPermanents()[0],
                            cardCondition: DigivolveToCardCondition,
                            payCost: false,
                            reduceCostTuple: null,
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null));
                    }
                }
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}
