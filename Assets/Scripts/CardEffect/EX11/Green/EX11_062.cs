using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.EX11
{
    // Shoto Kazama
    public class EX11_062 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of turn set to 3
            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend. If effects suspended those digimon, Draw 1. 1 digimon gains +3k DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[All Turns] When any Digimon suspend, by suspending this Tamer, if effects suspended those Digimon, <Draw 1>. After, 1 of your digimon with [Avian] or [Bird] in any of its traits or the [Vortex Warrior] trait gets +3000 DP until your opponent's turn ends.";

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.IsDigimon;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && (permanent.TopCard.ContainsTraits("Avian")
                        || permanent.TopCard.ContainsTraits("Bird")
                        || permanent.TopCard.EqualsTraits("Vortex Warrior"));
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    if (CardEffectCommons.IsByEffect(_hashtable, null))
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    }

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

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

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 Digimon that will gain 3k DP.",
                        "The opponent is selecting 1 Digimon that will gain 3k DP.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            targetPermanent: permanent,
                            changeValue: 3000,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));
                    }
                }    
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.None)
            {
                bool AttackerCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && !card.Owner.Enemy.GetBattleAreaDigimons().Any(permanent => !permanent.IsSuspended);
                }

                string effectName = "While your opponent has no unsuspended digimon, your <Vortex> can also attack players.";

                cardEffects.Add(CardEffectFactory.VortexCanAttackPlayersStaticEffect(
                    attackerCondition: AttackerCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition,
                    effectName: effectName
                ));
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
