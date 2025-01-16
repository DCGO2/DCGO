using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT9_069 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Suspend Digimons and Tamers and gain Memory", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Unsuspend up to 2 Digimon and/or Tamers. Then, gain 1 memory for each of your opponent's unsuspended Digimon and Tamers.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                {
                    if (permanent.IsDigimon)
                    {
                        return true;
                    }

                    if (permanent.IsTamer)
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
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) + card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                {
                    int maxCount = Math.Min(2, card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) + card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: CanEndSelectCondition,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: true,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.UnTap,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    bool CanEndSelectCondition(List<Permanent> permanents)
                    {
                        if (CardEffectCommons.HasNoElement(permanents))
                        {
                            return false;
                        }

                        return true;
                    }
                }

                int plusMemory = card.Owner.Enemy.GetBattleAreaPermanents().Count((permanent) => !permanent.IsSuspended && (permanent.IsDigimon || permanent.IsTamer));

                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(plusMemory, activateClass));
            }
        }

        if (timing == EffectTiming.OnEndTurn)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Trash opponent's security", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
            activateClass.SetHashString("TrashSecurity_BT9_069");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[End of Your Turn][Once Per Turn] For every 2 unsuspended Digimon and/or Tamers your opponent has in play, trash 1 card from the top of your opponent's security stack.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
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
                if (card.Owner.SecurityCards.Count >= 1)
                {
                    int trashCount = card.Owner.Enemy.GetBattleAreaPermanents().Count((permanent) => !permanent.IsSuspended && (permanent.IsDigimon || permanent.IsTamer)) / 2;

                    if (trashCount >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner.Enemy,
                            destroySecurityCount: trashCount,
                            cardEffect: activateClass,
                            fromTop: true).DestroySecurity());
                    }
                }
            }
        }

        return cardEffects;
    }
}
