using System;
using System.Collections;
using System.Collections.Generic;

// Hudiemon 
namespace DCGO.CardEffects.BT23
{
    public class BT23_101 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.IsLevel3
                        && targetPermanent.TopCard.HasCSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 4,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Erika Mishima");
                }

                bool Condition()
                {
                    return card.Owner.GetBattleAreaPermanents()
                        .Filter(x => x.IsTamer && x.TopCard.HasHudieTraits)
                        .Count >= 4;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: true,
                    card: card,
                    condition: Condition)
                );
            }

            #endregion

            #region Alliance

            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #region OP/WD Shared

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool CanSelectHandCardCondition(CardSource cardSource)
                {
                    return cardSource.HasPlayCost && cardSource.BasePlayCostFromEntity <= 5
                        && cardSource.HasCSTraits;
                }

                bool IsOpponentDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                int HudieDigimonAmount()
                {
                    return card.Owner.GetBattleAreaPermanents()
                        .Filter(x => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(x, card)
                            && x.TopCard.HasHudieTraits)
                        .Count;
                }

                if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectHandCardCondition))
                {
                    CardSource selectedHandCard = null;

                    #region Select Hand card

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, CanSelectHandCardCondition));
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectHandCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
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

                    selectHandEffect.SetUpCustomMessage("Select 1 tamer to play", "The opponent is selecting 1 tamer to play");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Selected card");
                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    #endregion

                    if (selectedHandCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: new List<CardSource>() { selectedHandCard },
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Hand,
                                    activateETB: true));
                }

                if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentDigimon))
                {
                    Permanent selectedOpponentDigimon = null;

                    #region Select Opponent Digimon

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsOpponentDigimon));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOpponentDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedOpponentDigimon = permanent;
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    #endregion

                    if (selectedOpponentDigimon != null)
                    {
                        int dpReduction = HudieDigimonAmount() * 3000;
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            targetPermanent: selectedOpponentDigimon,
                            changeValue: -dpReduction,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass));
                    }
                }

            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 5 cost or lower [CS] card from hand, then 1 digimon gains -3000 for each of your [Hudie] digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] You may play 1 play cost 5 or lower [CS] trait card from your hand without paying the cost. Then, to 1 of your opponent's Digimon, give -3000 DP for the turn for each of your [Hudie] trait Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 5 cost or lower [CS] card from hand, then 1 digimon gains -3000 for each of your [Hudie] digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may play 1 play cost 5 or lower [CS] trait card from your hand without paying the cost. Then, to 1 of your opponent's Digimon, give -3000 DP for the turn for each of your [Hudie] trait Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            #region When Attacking - OPT

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By bounce 1 [CS] tamer to hand, Activate 1 [On Play]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT23_101_WA");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] By returning 1 of your [CS] trait Tamers to the hand, activate 1 of this Digimon's [On Play] effects.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsCSTamer);
                }

                bool IsCSTamer(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.TopCard.HasCSTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsCSTamer))
                    {
                        Permanent selectedPermament = null;
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsCSTamer));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsCSTamer,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermament = permanent;
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        if (selectedPermament != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.BouncePeremanentAndProcessAccordingToResult(
                                targetPermanents: new List<Permanent>() { selectedPermament },
                                activateClass: activateClass,
                                successProcess: SuccessProcess(),
                                failureProcess: null));

                            IEnumerator SuccessProcess()
                            {
                                List<ICardEffect> candidateEffects = card.PermanentOfThisCard().EffectList(EffectTiming.OnEnterFieldAnyone)
                                .Clone()
                                .Filter(cardEffect => cardEffect != null && cardEffect is ActivateICardEffect && !cardEffect.IsSecurityEffect && cardEffect.IsOnPlay);

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
                                        Hashtable effectHashtable = CardEffectCommons.OnPlayCheckHashtableOfCard(selectedEffect.EffectSourceCard);

                                        if (!selectedEffect.IsDisabled)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(
                                            ((ActivateICardEffect)selectedEffect).Activate_Optional_Effect_Execute(effectHashtable));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}