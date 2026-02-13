using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Deramon
namespace DCGO.CardEffects.BT24
{
    public class BT24_048 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Blocker

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #region Shared OP/WD

            string SharedEffectName = "May hatch, may digivolve in breeding";

            string SharedEffectDescription(string tag) => $"[{tag}] You may hatch in your breeding area. Then, 1 of your Digimon with [Avian] or [Bird] in any of its traits in the breeding area may digivolve into a level 5 or lower Digimon card with [Avian] or [Bird] in any of its traits in the hand without paying the cost.";

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            bool DigivolveFromPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBreedingArea(permanent, card)
                    && (permanent.TopCard.ContainsTraits("Avian")
                    || permanent.TopCard.ContainsTraits("Bird"));
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool DigivolveToCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel
                        && cardSource.Level <= 5
                        && (cardSource.ContainsTraits("Avian")
                        || cardSource.ContainsTraits("Bird"))
                        && cardSource.CanPlayCardTargetFrame(
                                   frame: card.Owner.GetBreedingAreaPermanents()[0].PermanentFrame,
                                   PayCost: false,
                                   cardEffect: activateClass,
                                   root: SelectCardEffect.Root.Hand,
                                   isBreedingArea: true);
                }

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

                if (card.Owner.GetBreedingAreaPermanents().Count(DigivolveFromPermanentCondition) >= 1)
                {
                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new(message: "Digivolve in the breeding area", value: true, spriteIndex: 0),
                            new(message: "Do not digivolve", value: false, spriteIndex: 1),
                        };

                    string selectPlayerMessage = "Digivolve into [Avian] or [Bird] in any of its traits in breeding area?";
                    string notSelectPlayerMessage = "The opponent is choosing whether to digivolve in the breeding area or not.";

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements,
                        selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                        notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                        .WaitForEndSelect());

                    if (GManager.instance.userSelectionManager.SelectedBoolValue)
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

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }

            #endregion

            #region All Turns - ESS

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT24_048_Inherited");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon deletes an opponent's Digimon in battle, it may unsuspend.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition))
                        {
                            bool WinnerCondition(Permanent permanent) => permanent.cardSources.Contains(card);
                            bool WinnerRealCondition(Permanent permanent)
                            {
                                return true;
                            }
                            bool LoserCondition(Permanent permanent) => CardEffectCommons.IsOpponentPermanent(permanent, card);

                            if (CardEffectCommons.CanTriggerWhenDeleteOpponentDigimonByBattle(hashtable: hashtable, winnerCondition: WinnerCondition, winnerRealCondition: WinnerRealCondition, loserCondition: LoserCondition, isOnlyWinnerSurvive: false))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
