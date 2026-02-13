using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DCGO.CardEffects.P
{
    public class P_169 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Start of Your Main Phase] If your opponent has a Digimon, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           card.Owner.Enemy.GetBattleAreaDigimons().Count >= 1 &&
                           card.Owner.CanAddMemory(activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            #endregion
            
            #region All Turns
            
            if (timing == EffectTiming.OnDigivolutionCardDiscarded)
            {
                Permanent trashedFromPermanent = null;
                
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 digivolution card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When effects trash digivolution cards from any of your [Mineral] or [Rock] trait Digimon, by suspending this Tamer, place 1 card with the [Mineral] or [Rock] trait from your trash as any of those Digimon's bottom digivolution card.";
                }

                bool IsRockMineralDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.EqualsTraits("Mineral") || permanent.TopCard.EqualsTraits("Rock");
                }

                bool HasProperTraits(CardSource source)
                {
                    return (source.EqualsTraits("Mineral") || source.EqualsTraits("Rock"));
                }

                bool CanSelectDigimonToAddSources(Permanent permanent)
                {
                    return permanent == trashedFromPermanent;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    trashedFromPermanent = CardEffectCommons.GetPermanentFromHashtable(hashtable);
                    return isExistOnField(card) &&
                           CardEffectCommons.IsOwnerPermanent(trashedFromPermanent, card) &&
                            IsRockMineralDigimon(trashedFromPermanent);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {                    
                    return isExistOnField(card) && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;
                    List<CardSource> selectedCards = new List<CardSource>();

                    yield return ContinuousController.instance.StartCoroutine(
                        new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() },
                            CardEffectCommons.CardEffectHashtable(activateClass)).Tap());
                    
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectDigimonToAddSources,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get a digivolution card.", "The opponent is selecting 1 Digimon that will get a digivolution card.");
                    
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    
                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        if (permanent != null)
                            selectedPermanent = permanent;

                        yield return null;

                    }

                    if(selectedPermanent != null)
                    {
                        if(CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, HasProperTraits))
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: HasProperTraits,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: CanEndSelectCondition,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select [Mineral] or [Rock] to place on bottom of digivolution cards.",
                            maxCount: 1,
                            canEndNotMax: true,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");
                        selectCardEffect.SetUpCustomMessage("Select [Mineral] or [Rock] to place on bottom of digivolution cards.", "The opponent is selecting [Mineral] or [Rock] to place on bottom of digivolution cards.");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        bool CanEndSelectCondition(List<CardSource> cardSources)
                        {
                            if (CardEffectCommons.HasNoElement(cardSources))
                            {
                                return false;
                            }

                            return true;
                        }

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        if (selectedCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(selectedCards, activateClass));
                        }
                        }
                    }
                }
            }
            
            #endregion

            #region Security Effect

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}