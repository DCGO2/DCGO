using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

// Ceresmon
namespace DCGO.CardEffects.BT25
{
    public class BT25_059 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.HasTSTraits
                        || permanent.TopCard.EqualsTraits("Vegetation");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, true, card, null, level: 5));
            }
            #endregion

            #region Shared Conditions
            bool IsSuspendedDigimon(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent)
                    && permanent.IsSuspended;
            }
            #endregion

            #region Reduce Play Cost
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.MatchConditionPermanentCount(IsSuspendedDigimon) >= 2;
                }

                cardEffects.Add(CardEffectFactory.MandatorySelfPlayCostReduction(5, card, Condition));
            }
            #endregion

            #region Shared OP / WD

            string SharedEffectName = "You may suspend 2 digimon. Your suspended [Vegetation]/[TS] Digimon are immune to opponent's Digimon Effects until end of opponents turn";

            string SharedEffectDescription(string tag)
                => $"[{tag}] You may suspend up to 2 Digimon. Then, none of your suspended [Vegetation] or [TS] trait Digimon are affected by your opponent's Digimon effects until their turn ends.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon));

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: true,
                    canEndNotMax: true,
                    selectPermanentCoroutine: null,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Tap,
                    cardEffect: activateClass);

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                #region Effect Immunity
                bool CanUseCondition1(Hashtable hashtable)
                {
                    return true;
                }

                bool CardCondition(CardSource cardSource)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(cardSource)
                        && cardSource.Owner == card.Owner
                        && cardSource == cardSource.PermanentOfThisCard().TopCard
                        && cardSource.PermanentOfThisCard().IsSuspended
                        && (cardSource.HasTSTraits || cardSource.EqualsTraits("Vegetation"));
                }

                bool SkillCondition(ICardEffect cardEffect)
                {
                    return CardEffectCommons.IsOpponentEffect(cardEffect, card)
                        && cardEffect.IsDigimonEffect;
                }

                CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                canNotAffectedClass.SetUpICardEffect("Not affected by opponent's Digimon's effects", CanUseCondition1, card);
                canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                CardEffectCommons.AddEffectToPlayer(EffectDuration.UntilOpponentTurnEnd, card, canNotAffectedClass, EffectTiming.None);
                #endregion
            }

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    onPlay: true,
                    whenDigivolving: true);
            #endregion

            #region All turns
            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("-3k to 1 enemy digimon per suspended digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription1());
                activateClass.SetHashString("BT25_059_AT");
                cardEffects.Add(activateClass);

                string EffectDescription1()
                {
                    return "[All Turns] [Once Per Turn] When any Digimon suspend, to 1 of your opponent's Digimon, give -3000 DP until their turn ends for each suspended Digimon.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanTriggerPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, CanTriggerPermanent);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int reduction = -3000 * CardEffectCommons.MatchConditionPermanentCount(IsSuspendedDigimon);

                    if (reduction < 0 && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentCondition))
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

                        selectPermanentEffect.SetUpCustomMessage($"Choose a digimon to give {reduction} DP.", $"Opponent is choosing a digimon to give {reduction} DP.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: permanent,
                                changeValue: reduction,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
