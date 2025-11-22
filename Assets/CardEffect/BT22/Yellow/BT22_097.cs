using System;
using System.Collections;
using System.Collections.Generic;

// Music of the Heart
namespace DCGO.CardEffects.BT22
{
    public class BT22_097 : CEntity_Effect
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
                    if (CardEffectCommons.IsOwnerPermanent(permanent, card))
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
                activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] <Draw 1> (Draw 1 card from your deck.) Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }

            #endregion

            #region All Turns - Delay

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Link 1 [Appmon] trait Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When any of your [Appmon] trait Digimon are played, <Delay> (By trashing this card after the placing turn, activate the effect below.)\r\n・You may link 1 [Appmon] trait Digimon card from your hand to 1 of your Digimon without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanDeclareOptionDelayEffect(card)
                        && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => cardSource.CanLink(false));
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasAppmonTraits;
                }

                bool OwnPermamentCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                bool LinkCardCondition(CardSource cardSource, Permanent targetPermament) => cardSource.IsDigimon && cardSource.HasAppmonTraits && cardSource.CanLinkToTargetPermanent(targetPermament, false);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool delaySuccessful = false;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        delaySuccessful = true;
                        yield return null;
                    }

                    if (delaySuccessful)
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, OwnPermamentCondition))
                        {
                            Permanent selectedDigimon = null;

                            #region Select Permament Link Target

                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, OwnPermamentCondition));
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: OwnPermamentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedDigimon = permanent;
                                yield return null;
                            }

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to link to", "The opponent is selecting 1 Digimon to link to");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            #endregion

                            if (selectedDigimon != null && CardEffectCommons.HasMatchConditionOwnersHand(card, handCard => LinkCardCondition(handCard, selectedDigimon)))
                            {
                                CardSource selectedHandCard = null;

                                #region Select Hand card

                                int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, handCard => LinkCardCondition(handCard, selectedDigimon)));
                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: handCard => LinkCardCondition(handCard, selectedDigimon),
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount1,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedHandCard = cardSource;
                                    yield return null;
                                }

                                selectHandEffect.SetUpCustomMessage("Select 1 [Appmon] to link", "The opponent is selecting 1 [Appmon] to link");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Selected card");
                                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                                #endregion

                                if (selectedHandCard != null) yield return ContinuousController.instance.StartCoroutine(selectedDigimon.AddLinkCard(selectedHandCard, activateClass));
                            }
                        }
                    }
                }
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("place in battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Security] place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}