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
                    if(CardEffectCommons.IsOwnerPermanent(permanent, card))
                    {
                        if (permanent.TopCard.EqualsTraits("Appmon"))
                        {
                            if (permanent.IsDigimon || permanent.IsTamer)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionPermanent(HasAppmonTrait, true);
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
                    return cardSource.EqualsTraits("Appmon") || cardSource.EqualsTraits("App Driver");
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

                bool CanSelectLinkTarget(CardSource cardSoure)
                {
                    return cardSoure.CanLink(false);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool delaySuccussful = false;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                    targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                    activateClass: activateClass,
                    successProcess: permanents => SuccessProcess(),
                    failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        delaySuccussful = true;

                        yield return null;
                    }

                    if (delaySuccussful)
                    {
                        Permanent selectedPermanent = null;
                        CardSource cardForLinking = null;

                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectLinkTarget))
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectLinkTarget,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectHandEffect.SetUpCustomMessage("Select 1 card to link.", "The opponent is selecting 1 card to link.");

                            yield return StartCoroutine(selectHandEffect.Activate());
                        }

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            cardForLinking = cardSource;

                            yield return null;
                        }

                        if (cardForLinking != null)
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to link.", "The opponent is selecting 1 Digimon to link.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            bool CanSelectPermanentCondition(Permanent permanent)
                            {
                                return  CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                                        cardForLinking.CanLinkToTargetPermanent(permanent, false);
                            }

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;

                                yield return null;
                            }

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddLinkCard(cardForLinking, activateClass));
                            }
                        }
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