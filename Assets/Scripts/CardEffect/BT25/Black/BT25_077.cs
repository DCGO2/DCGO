using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

// Bacchusmon
namespace DCGO.CardEffects.BT25
{
    public class BT25_077 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, true, card, null, level: 5));
            }
            #endregion

            #region Reduce Play Cost
            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -5", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    int totalLevels = GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                        .Map(player => player.GetBattleAreaPermanents())
                        .Flat()
                        .Map(LevelFromPermanent)
                        .Sum();
                    return totalLevels >= 12;
                }

                int LevelFromPermanent(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent)
                        && permanent.TopCard.HasLevel)
                    {
                        return permanent.Level;
                    }

                    return 0;
                }

                int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root,
                        List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource) &&
                        RootCondition(root) &&
                        PermanentsCondition(targetPermanents))
                    {
                        cost -= 5;
                    }

                    return cost;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    return targetPermanents == null || targetPermanents.Count(targetPermanent => targetPermanent != null) == 0;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                bool isUpDown()
                {
                    return true;
                }
            }
            #endregion

            #region Shared OP / WD

            string SharedEffectName = "You may play 1 [TS] Dgiimon with 6k or less DP";

            string SharedEffectDescription(string tag)
                => $"[{tag}] You may play 1 [TS] trait Digimon card with 6000 DP or less from your hand without paying the cost.";

            bool CanPlayTSDigimonCondition(CardSource cardSource, ActivateClass activateClass)
            {
                return cardSource.IsDigimon
                    && cardSource.HasPlayCost
                    && cardSource.HasTSTraits
                    && cardSource.HasDP
                    && cardSource.CardDP <= 6000
                    && CardEffectCommons.CanPlayAsNewPermanent(card, false, activateClass);
            }

            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => CanPlayTSDigimonCondition(cardSource, activateClass));
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: cardSource => CanPlayTSDigimonCondition(cardSource, activateClass),
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: null,
                    mode: SelectHandEffect.Mode.PlayForFree,
                    cardEffect: activateClass);

                yield return StartCoroutine(selectHandEffect.Activate());
            }

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    additionalActivateCondition: AdditionalActivateCondition,
                    optional: false,
                    isSkippable: true,
                    onPlay: true,
                    whenDigivolving: true);
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May suspend 1 Digimon. By Effect: Delete lowest DP enemy digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("AD1_024_AT");
                activateClass.SetIsOptionalOverride(IsOptional);
                cardEffects.Add(activateClass);

                string EffectDescription() 
                    => "[All Turns] [Once Per Turn] When any Digimon are played or digivolve, you may suspend 1 Digimon. Then, if played or digivolved by an effect, delete 1 of your opponent's lowest DP Digimon.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, IsDigimonCondition)
                            || CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, IsDigimonCondition));
                }

                bool IsOptional(Hashtable hashtable)
                {
                    return !CardEffectCommons.IsByEffect(hashtable, null);
                }

                bool IsDigimonCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);

                bool IsOpponentsWeakestDigimon(Permanent permanent) => CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy);

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleArea(card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool isByEffect = CardEffectCommons.IsByEffect(hashtable, null);
                    bool executed = false;
                    
                    if (CardEffectCommons.HasMatchConditionPermanent(IsDigimonCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsDigimonCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            if (permanents.Count > 0)
                                executed = true;
                            yield return null;
                        }
                    }

                    if (isByEffect)
                    {
                        executed = true;

                        if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsOpponentsWeakestDigimon))
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsOpponentsWeakestDigimon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }

                    if (!executed) activateClass.RemoveUse();
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
