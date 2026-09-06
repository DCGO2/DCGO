using System.Collections;
using System.Collections.Generic;

// Gypt Particle Cannon
namespace DCGO.CardEffects.BT12
{
    public class BT12_106 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Ignore Color Requirement
            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);

                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent)
                        => permanent.IsTamer
                        && permanent.TopCard.CardTraits.Contains("Hunter"));
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
                    return "[Main] Suspend all of your opponent's Digimon and Tamers. Your opponent's cards don't unsuspend during their next unsuspend phase.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                        && (permanent.IsDigimon
                            || permanent.IsTamer)
                            && !permanent.TopCard.CanNotBeAffected(activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        List<Permanent> suspendTargetPermanents = card.Owner.Enemy.GetBattleAreaPermanents().Filter(CanSelectPermanentCondition);
                        yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(suspendTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotUnsuspendPlayerEffect(
                        permanentCondition: CanSelectPermanentCondition,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass,
                        isOnlyActivePhase: true,
                        effectName: "Your card can't unsuspend"));
                }
            }
            #endregion

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Suspend opponent's all Digimon and Tamers", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Security] Suspend all of your opponent's Digimon and Tamers.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                        && (permanent.IsDigimon
                            || permanent.IsTamer)
                        && !permanent.TopCard.CanNotBeAffected(activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        List<Permanent> suspendTargetPermanents = card.Owner.Enemy.GetBattleAreaPermanents().Filter(CanSelectPermanentCondition);
                        yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(suspendTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}