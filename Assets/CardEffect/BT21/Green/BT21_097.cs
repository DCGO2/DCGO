using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT21
{
    public class BT21_097 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Ignore Color Requirment
            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);
                cardEffects.Add(ignoreColorConditionClass);

                bool HasAppmonTrait(Permanent permanent)
                {
                    if (permanent.TopCard.ContainsTraits("Appmon"))
                    {
                        if (permanent.IsDigimon || permanent.IsTamer)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasAppmonTrait);
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] Reveal the top 3 cards of your deck. Add 1 card with the [Appmon]/[App Driver] trait among them to the hand. Trash the rest. Then, place this card in the battle area.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.ContainsTraits("Appmon") || cardSource.ContainsTraits("App Driver");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 card with [Appmon]/[App Driver] in its traits.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null)                     
                        },
                        remainingCardsPlace: RemainingCardsPlace.Trash,
                        activateClass: activateClass
                    ));

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }
            #endregion

            #region End of Your Turn - Delay
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Link 1 card from hand with 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn] You may link 1 card from your hand with 1 of your Digimon without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        return CardEffectCommons.CanDeclareOptionDelayEffect(card);                       
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return true;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        foreach (CardSource cardSource in card.Owner.HandCards)
                        {
                            if (CanSelectCardCondition(cardSource))
                            {
                                if (cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, false, activateClass))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                    targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                    activateClass: activateClass,
                    successProcess: permanents => SuccessProcess(),
                    failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        yield return null; 
                        //if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        //{
                        //    Permanent selectedPermanent = null;

                        //    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        //    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        //    selectPermanentEffect.SetUp(
                        //        selectPlayer: card.Owner,
                        //        canTargetCondition: CanSelectPermanentCondition,
                        //        canTargetCondition_ByPreSelecetedList: null,
                        //        canEndSelectCondition: null,
                        //        maxCount: maxCount,
                        //        canNoSelect: true,
                        //        canEndNotMax: false,
                        //        selectPermanentCoroutine: SelectPermanentCoroutine,
                        //        afterSelectPermanentCoroutine: null,
                        //        mode: SelectPermanentEffect.Mode.Custom,
                        //        cardEffect: activateClass);

                        //    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will digivolve.", "The opponent is selecting 1 Digimon that will digivolve.");

                        //    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        //    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        //    {
                        //        selectedPermanent = permanent;

                        //        yield return null;
                        //    }

                        //    if (selectedPermanent != null)
                        //    {
                        //        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        //            targetPermanent: selectedPermanent,
                        //            cardCondition: CanSelectCardCondition,
                        //            payCost: false,
                        //            reduceCostTuple: null,
                        //            fixedCostTuple: null,
                        //            ignoreDigivolutionRequirementFixedCost: -1,
                        //            isHand: true,
                        //            activateClass: activateClass,
                        //            successProcess: null));
                        //    }
                        //}
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaceSelfDelayOptionSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}