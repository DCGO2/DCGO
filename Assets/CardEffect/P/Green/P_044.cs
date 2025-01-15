using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class P_044 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Suspend Digimons", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Suspend 1 of your opponent's Digimon, or 2 of your opponent's Digimon with 5000 DP or less.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanSelectPermanentCondition1(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.DP <= 5000)
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
                    bool canSuspend1 = CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                    bool canSuspend2 = CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1);

                    if (canSuspend1 || canSuspend2)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                bool canSuspend1 = CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                bool canSuspend2 = CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1);

                if (canSuspend1 || canSuspend2)
                {
                    if (canSuspend1 && canSuspend2)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Suspend 1", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Suspend 2", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Which effect do you choose?";
                        string notSelectPlayerMessage = "The opponent is choosing the effect.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                    }

                    else
                    {
                        GManager.instance.userSelectionManager.SetBool(canSuspend1);
                    }

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool _isSuspendOne = GManager.instance.userSelectionManager.SelectedBoolValue;

                    int maxCount = _isSuspendOne ? Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition)) : Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition1));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: _isSuspendOne ? CanSelectPermanentCondition : CanSelectPermanentCondition1,
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
            }
        }

        return cardEffects;
    }
}
