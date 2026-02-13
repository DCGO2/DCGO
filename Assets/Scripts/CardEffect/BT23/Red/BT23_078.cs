using System;
using System.Collections;
using System.Collections.Generic;

// Goro Matayoshi
namespace DCGO.CardEffects.BT23
{
    public class BT23_078 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                cardEffects.Add(CardEffectFactory.Gain1MemoryTamerOpponentDigimonEffect(card));
            }

            #endregion

            #region Your Turn

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By returning tamer to hand, 1 digimon gains +3K DP. then 1 digimon may attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When your Digimon are played or digivolve, if any of them have [Avian], [Bird], [Beast], [Animal] or [Sovereign] in any of their traits (other than [Sea Animal]) or the [CS] trait, by returning this Tamer to the hand, 1 of your Digimon gets +3000 DP for the turn. Then, 1 of your Digimon may attack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, IsValidDigimon) || CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, IsValidDigimon);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool IsValidDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.HasBeastTraits || permanent.TopCard.HasBirdTraits || permanent.TopCard.HasCSTraits);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    return permanent.CanAttack(activateClass) && CanSelectPermanentCondition(permanent);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.BouncePeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass,
                        successProcess: SuccessProcess(),
                        failureProcess: null)
                    );

                    IEnumerator SuccessProcess()
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            Permanent selectedPermament = null;
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to gain 3k DP.", "The opponent is selecting 1 Digimon to gain 3k DP.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermament = permanent;
                                yield return null;
                            }

                            if (selectedPermament != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: selectedPermament,
                                changeValue: 3000,
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass));
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                        {
                            Permanent selectedPermament = null;
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that may attack", "The opponent is selecting 1 Digimon that may attack");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermament = permanent;
                                yield return null;
                            }

                            if (selectedPermament != null)
                            {
                                SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                selectAttackEffect.SetUp(
                                    attacker: selectedPermament,
                                    canAttackPlayerCondition: () => true,
                                    defenderCondition: (permanent) => true,
                                    cardEffect: activateClass);

                                selectAttackEffect.SetCanNotSelectNotAttack();

                                yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                            }
                        }
                    }
                }
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}