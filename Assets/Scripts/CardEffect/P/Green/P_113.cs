using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class P_113 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent.TopCard.ContainsCardName("Tyrannomon") && targetPermanent.Level == 5 && targetPermanent.TopCard.HasLevel;
            }

            cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                permanentCondition: PermanentCondition,
                digivolutionCost: 3,
                ignoreDigivolutionRequirement: false,
                card: card,
                condition: null));
        }

        if (timing == EffectTiming.OnCounterTiming)
        {
            cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Suspend Digimons", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Suspend all of your opponent's Digimon with DP less or equal than this Digimon.";
            }

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().HasDP)
                        {
                            if (permanent.TopCard.HasDP)
                            {
                                if (permanent.DP <= card.PermanentOfThisCard().DP)
                                {
                                    if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                    {
                                        if (!permanent.IsSuspended)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                List<Permanent> suspendTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(PermanentCondition);

                yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(
                    suspendTargetPermanents,
                    CardEffectCommons.CardEffectHashtable(activateClass)).Tap());
            }
        }

        if (timing == EffectTiming.OnEndBattle)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Trash the top card of opponent's security", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
            activateClass.SetHashString("TrashSecurity_P_113");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[All Turns] [Once Per Turn] When an opponent's Digimon is deleted in battle, trash the top card of your opponent's security stack.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    bool LoserCondition(Permanent permanent) => CardEffectCommons.IsOpponentPermanent(permanent, card);

                    if (CardEffectCommons.CanTriggerWhenDeleteOpponentDigimonByBattle(
                        hashtable: hashtable,
                        winnerCondition: null,
                        loserCondition: LoserCondition,
                        isOnlyWinnerSurvive: false))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                    player: card.Owner.Enemy,
                    destroySecurityCount: 1,
                    cardEffect: activateClass,
                    fromTop: true).DestroySecurity());
            }
        }

        return cardEffects;
    }
}
