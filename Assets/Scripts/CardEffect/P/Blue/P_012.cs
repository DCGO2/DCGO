using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class P_012 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnDeclaration)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Draw 1 or your 1 Digimon gains DP +1000", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] If you have a Digimon with [Veedramon] in its name, you may suspend this Tamer to activate one of the following effects: - Trigger <Draw 1>. (Draw 1 card from your deck. - 1 of your Digimon gets +1000 DP for the turn.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (isExistOnField(card))
                {
                    if (!card.PermanentOfThisCard().IsSuspended && card.PermanentOfThisCard().CanSuspend)
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent.TopCard.ContainsCardName("Veedramon")))
                        {
                            return true;
                        }
                    }
                }


                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (isExistOnField(card))
                {
                    if (!card.PermanentOfThisCard().IsSuspended && card.PermanentOfThisCard().CanSuspend)
                    {
                        Hashtable hashtable = new Hashtable();
                        hashtable.Add("CardEffect", activateClass);

                        yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, hashtable).Tap());

                        yield return GManager.instance.photonWaitController.StartWait("Taichi_Select");
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new SelectionElement<int>(message: $"Draw 1", value : 0, spriteIndex: 0),
                            new SelectionElement<int>(message: $"DP +1000", value : 1, spriteIndex: 0),
                        };

                        string selectPlayerMessage = "Which effect will you activate?";
                        string notSelectPlayerMessage = "The opponent is choosing which effect to activate.";

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        int actionID = GManager.instance.userSelectionManager.SelectedIntValue;

                        switch (actionID)
                        {
                            case 0:
                                yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                                break;

                            case 1:
                                bool CanSelectPermanentCondition(Permanent permanent)
                                {
                                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
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
                                        selectPermanentCoroutine: SelectPermanentCoroutine,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP +1000.", "The opponent is selecting 1 Digimon that will get DP +1000.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: 1000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
        }

        return cardEffects;
    }
}
