using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
                bool Condition()
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

                cardEffects.Add(CardEffectFactory.MandatorySelfPlayCostReduction(5, card, Condition));
            }
            #endregion

            #region Shared OP / WD

            string SharedEffectName = "You may play 1 [TS] Digimon with 6k or less DP";

            string SharedEffectDescription(string tag)
                => $"[{tag}] You may play 1 [TS] trait Digimon card with 6000 DP or less from your hand without paying the cost.";

            bool CanPlayTSDigimonCondition(CardSource cardSource, ActivateClass activateClass)
            {
                return cardSource.IsDigimon
                    && cardSource.HasPlayCost
                    && cardSource.HasTSTraits
                    && cardSource.HasDP
                    && cardSource.CardDP <= 6000
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
            }

            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => CanPlayTSDigimonCondition(cardSource, activateClass));
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                    canTargetCondition: cardSource => CanPlayTSDigimonCondition(cardSource, activateClass),
                    SelectCardEffect.Root.Hand,
                    activateClass,
                    payCost: false
                ));
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
                activateClass.SetIsSkippableFunction(IsOptional);
                activateClass.SetHashString("BT25_077_AT");
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

                        string message = isByEffect ? "Select a Digimon to suspend. Triggered by effect: After you will delete 1 enemy digimon." : "Select a Digimon to suspend. Not triggered by effect: Select \"No Selection\" to not expend the once per turn.";

                        selectPermanentEffect.SetUpCustomMessage(message, "Opponent is selecting a Digimon to suspend.");

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
