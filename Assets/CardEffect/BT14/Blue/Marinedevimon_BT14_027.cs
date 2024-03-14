using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class Marinedevimon_BT14_027 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Return all level 3 Digimons to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] Return all level 3 Digimon to the hand.";
            }

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                {
                    if (permanent.IsDigimon)
                    {
                        if (permanent.TopCard.HasLevel)
                        {
                            if (permanent.Level == 3)
                            {
                                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    if (!permanent.CannotReturnToHand(activateClass))
                                    {
                                        return true;
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
                return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
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
                List<Permanent> boounceTargetPermanents =
                GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                .Map(player => player.GetBattleAreaPermanents())
                .Flat()
                .Filter(PermanentCondition);

                yield return ContinuousController.instance.StartCoroutine(new HandBounceClaass(boounceTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Bounce());
            }
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Return all level 3 Digimons to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Return all level 3 Digimon to the hand.";
            }

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                {
                    if (permanent.IsDigimon)
                    {
                        if (permanent.TopCard.HasLevel)
                        {
                            if (permanent.Level == 3)
                            {
                                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    if (!permanent.CannotReturnToHand(activateClass))
                                    {
                                        return true;
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
                List<Permanent> boounceTargetPermanents =
                GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                .Map(player => player.GetBattleAreaPermanents())
                .Flat()
                .Filter(PermanentCondition);

                yield return ContinuousController.instance.StartCoroutine(new HandBounceClaass(boounceTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Bounce());
            }
        }

        return cardEffects;
    }
}
