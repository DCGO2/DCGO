using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX10
{
    public class EX10_023 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Requirements
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.HasMatchConditionPermanent(HasRyomaMogami);
                }

                bool HasRyomaMogami(Permanent permanent)
                {
                    return CardEffectCommons.IsOwnerPermanent(permanent, card) &&
                           permanent.TopCard.EqualsCardName("Ryoma Mogami");
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Astamon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 7, ignoreDigivolutionRequirement: false, card: card, condition: Condition));
            }
            #endregion

            #region Blast Digivolve
            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }
            #endregion

            #region Shared OP/WD

            string EffectDiscriptionShared(string tag)
            {
                return $"[{tag}] Suspend all other Digimon and Tamers.";
            }

            bool CanActivateConditionShared(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            IEnumerator ActivateCoroutineShared(Hashtable hashtable, ActivateClass activateClass)
            {
                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent) &&
                            permanent != card.PermanentOfThisCard() &&
                            !permanent.TopCard.CanNotBeAffected(activateClass) &&
                            (permanent.IsDigimon || permanent.IsTamer));
                }

                List<Permanent> suspendTargetPermanents =
                    GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                    .Map(player => player.GetBattleAreaPermanents().Filter(CanSelectPermanentCondition))
                    .Flat();

                yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(suspendTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend all other Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, hash => ActivateCoroutineShared(hash, activateClass), -1, false, EffectDiscriptionShared("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend all other Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, hash => ActivateCoroutineShared(hash, activateClass), -1, false, EffectDiscriptionShared("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }
            #endregion

            #region When Digivolving/When Attacking Shared
            string SharedEffectDiscription(string tag)
            {
                return $"[{tag}] Delete 1 of your opponent's suspended Digimon.";
            }

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            bool SharedCanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                       permanent.IsSuspended;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(SharedCanSelectPermanentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: SharedCanSelectPermanentCondition,
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
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 Suspended Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), 1, false, SharedEffectDiscription("When Digivolving"));
                activateClass.SetHashString("WD_EX10-023");
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 Suspended Digimon/Tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), 1, false, SharedEffectDiscription("When Attacking"));
                activateClass.SetHashString("WD_EX10-023");
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                    {
                        if (permanent != card.PermanentOfThisCard())
                        {
                            if (permanent.IsDigimon || permanent.IsTamer)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           GManager.instance.turnStateMachine.gameContext.TurnPhase == GameContext.phase.Active;
                }

                string effectName = "[All Turns] Other than this Digimon, no Digimon or Tamers can unsuspend in the unsuspend phase.";

                cardEffects.Add(CardEffectFactory.CantUnsuspendStaticEffect(
                    permanentCondition: PermanentCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition,
                    effectName: effectName));
            }
            #endregion

            return cardEffects;
        }
    }
}
