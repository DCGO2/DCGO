using System.Collections;
using System.Collections.Generic;
using static Cinemachine.DocumentationSortingAttribute;
using System.Data;
using System.Runtime.ConstrainedExecution;
using UnityEngine.XR;
using System.Linq;

namespace DCGO.CardEffects.P
{
    public class P_147 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Rule: Name is also Pulsemon
            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Also treated as [Pulsemon]", CanUseCondition, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: changeCardNames);
                cardEffects.Add(changeCardNamesClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                List<string> changeCardNames(CardSource cardSource, List<string> CardNames)
                {
                    if (cardSource == card)
                    {
                        CardNames.Add("Pulsemon");
                    }

                    return CardNames;
                }
            }
            #endregion

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.CardNames.Contains("Bibimon"))
                    {
                        return true;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 0,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 level 4 with [Pulsemon] in its text as bottom digivolution source, activate one of that card's [When Digivolving] effects", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] By placing 1 level 4 card with [Pulsemon] in its text from your hand as this Digimon’s bottom digivolution card, activate one of that card's [When Digivolving] effects as an effect of this Digimon.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.HasLevel && cardSource.Level == 4)
                    {
                        if (cardSource.HasPulsemonText)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if(card.Owner.HandCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectHandEffect.SetUpCustomMessage(
                        "Select 1 card to place on bottom of digivolution cards.",
                        "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                    yield return StartCoroutine(selectHandEffect.Activate());


                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    CardSource selectedCard = null;

                    if (selectedCards.Count >= 1)
                    {
                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                                selectedCards,
                                activateClass));

                            selectedCard = selectedCards[0];
                        }
                    }

                    if (selectedCard != null)
                    {
                        List<ICardEffect> candidateEffects = selectedCard.EffectList_ForCard(EffectTiming.OnEnterFieldAnyone, card)
                            .Clone()
                            .Filter(cardEffect => cardEffect != null && cardEffect is ActivateICardEffect && !cardEffect.IsSecurityEffect && cardEffect.IsWhenDigivolving);

                        if (candidateEffects.Count >= 1)
                        {
                            ICardEffect selectedEffect = null;

                            if (candidateEffects.Count == 1)
                            {
                                selectedEffect = candidateEffects[0];
                            }

                            else
                            {
                                List<SkillInfo> skillInfos = candidateEffects
                                    .Map(cardEffect => new SkillInfo(cardEffect, null, EffectTiming.None));

                                List<CardSource> cardSources = candidateEffects
                                    .Map(cardEffect => cardEffect.EffectSourceCard);

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: (cardSource) => true,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 effect to activate.",
                                    maxCount: 1,
                                    canEndNotMax: false,
                                    isShowOpponent: false,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: cardSources,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetNotShowCard();
                                selectCardEffect.SetUpSkillInfos(skillInfos);
                                selectCardEffect.SetUpAfterSelectIndexCoroutine(AfterSelectIndexCoroutine);

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                IEnumerator AfterSelectIndexCoroutine(List<int> selectedIndexes)
                                {
                                    if (selectedIndexes.Count == 1)
                                    {
                                        selectedEffect = candidateEffects[selectedIndexes[0]];
                                        yield return null;
                                    }
                                }
                            }

                            if (selectedEffect != null)
                            {
                                Hashtable effectHashtable = CardEffectCommons.WhenDigivolvingCheckHashtableOfCard(card);

                                if (selectedEffect.CanUse(effectHashtable))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(
                                        ((ActivateICardEffect)selectedEffect).Activate_Optional_Effect_Execute(effectHashtable));
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Your Turn - DP Boost
            if (timing == EffectTiming.None)
            {
                bool HasTamer(Permanent permanent)
                {
                    return permanent.IsTamer;
                }

                bool Condition()
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        return CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasTamer);
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 3000,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}