using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT9_018 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Suspend Digimons and gain Memory", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] For each Tamer your opponent has in play, suspend 1 of your opponent's Digimon and gain 1 memory.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.CanSelectBySkill(activateClass))
                    {
                        return true;
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
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (card.Owner.Enemy.GetBattleAreaPermanents().Count((permanent) => permanent.IsTamer) >= 1)
                {
                    int tamerCount = card.Owner.Enemy.GetBattleAreaPermanents().Count((permanent) => permanent.IsTamer);
    
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(tamerCount, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));
    
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
    
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);
    
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
    
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(tamerCount, activateClass));
                }
            }
        }

        if (timing == EffectTiming.OnTappedAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Delete 1 suspended Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
            activateClass.SetHashString("Delete_BT9_018");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[All Turns][Once Per Turn] When an opponent's Digimon with 6000 DP or less becomes suspended, you may delete that Digimon.";
            }

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.DP <= 6000)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, PermanentCondition))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (_hashtable != null)
                    {
                        if (_hashtable.ContainsKey("Permanents"))
                        {
                            if (_hashtable["Permanents"] is List<Permanent>)
                            {
                                List<Permanent> Permanents = (List<Permanent>)_hashtable["Permanents"];

                                if (Permanents != null)
                                {
                                    bool CanSelectPermanentCondition(Permanent permanent)
                                    {
                                        if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                                        {
                                            if (Permanents.Contains(permanent))
                                            {
                                                if (permanent.DPWhenSuspended <= 6000)
                                                {
                                                    return true;
                                                }
                                            }
                                        }

                                        return false;
                                    }

                                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                                    {
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
                                            selectPermanentCoroutine: null,
                                            afterSelectPermanentCoroutine: null,
                                            mode: SelectPermanentEffect.Mode.Destroy,
                                            cardEffect: activateClass);

                                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return cardEffects;
    }
}
