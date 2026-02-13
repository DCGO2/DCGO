using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT6_111 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Add this card to hand at the end of the battle and opponent's Digimons can't attack", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] At the end of the battle, add this card to your hand. Then, if a Digimon with [Royal Knight] or [X-Antibody] in its type is in play, up to 12 of your opponent's Digimon can't attack players for the turn.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                {
                    if (permanent.IsDigimon)
                    {
                        if (permanent.TopCard.HasRoyalKnightTraits)
                        {
                            return true;
                        }

                        if (permanent.TopCard.HasXAntibodyTraits)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (card.Owner.ExecutingCards.Contains(card))
                {
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return null;

                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                ActivateClass activateClass1 = new ActivateClass();
                activateClass1.SetUpICardEffect("Add this card to hand and opponent's Digimons cannot attack", CanUseCondition1, card);
                activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                card.Owner.UntilEndBattleEffects.Add(GetCardEffect1);

                string EffectDiscription1()
                {
                    return "Add this card to your hand. Then, if a Digimon with [Royal Knight] or [X-Antibody] in its type is in play, up to 12 of your opponent's Digimon can't attack players for the turn";
                }

                bool CanUseCondition1(Hashtable hashtable)
                {
                    return true;
                }

                bool CanActivateCondition1(Hashtable hashtable)
                {
                    if (card.Owner.ExecutingCards.Contains(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                {
                    if (card.Owner.ExecutingCards.Contains(card))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(
                            new List<CardSource>() { card },
                            false,
                            activateClass1));

                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                int maxCount = Math.Min(12, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: CanEndSelectCondition,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: true,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage(
                                    "Select Digimon that will get effects.",
                                    "The opponent is selecting Digimon that will get effects.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                bool CanEndSelectCondition(List<Permanent> permanents)
                                {
                                    if (CardEffectCommons.HasNoElement(permanents))
                                    {
                                        return false;
                                    }

                                    return true;
                                }

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    bool DefenderCondition(Permanent defender)
                                    {
                                        return defender == null;
                                    }

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotAttack(
                                        targetPermanent: permanent,
                                        defenderCondition: DefenderCondition,
                                        effectDuration: EffectDuration.UntilEachTurnEnd,
                                        activateClass: activateClass,
                                        effectName: "Can't Attack to player"));
                                }
                            }
                        }
                    }
                }

                ICardEffect GetCardEffect1(EffectTiming _timing)
                {
                    if (_timing == EffectTiming.OnEndBattle)
                    {
                        return activateClass1;
                    }

                    return null;
                }
            }
        }

        if (timing == EffectTiming.OnAllyAttack)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Pay memory cost and this Digimon gets +DP", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Attacking] You may pay up to 5 memory. If you do, this Digimon gets +1000 DP for the turn for each memory paid.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.MaxMemoryCost >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (isExistOnField(card))
                {
                    if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                    {
                        int count = 0;

                        while (card.Owner.MaxMemoryCost >= 1 && count < 5)
                        {
                            yield return GManager.instance.photonWaitController.StartWait("PayMemory_Alphamon");

                            if (card.Owner.isYou)
                            {
                                GManager.instance.commandText.OpenCommandText($"Will you pay 1 memory cost?(already paid cost:{count})");

                                List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>()
                                {
                                    new Command_SelectCommand($"Pay Cost", () => photonView.RPC("SetPayCost", RpcTarget.All, true), 0),
                                    new Command_SelectCommand($"Not Pay Cost", () => photonView.RPC("SetPayCost", RpcTarget.All, false), 1),
                                };

                                GManager.instance.selectCommandPanel.SetUpCommandButton(command_SelectCommands);
                            }
                            else
                            {
                                GManager.instance.commandText.OpenCommandText("The opponent is choosing whether to pay memory cost.");

                                #region AIƒ‚[ƒh

                                if (GManager.instance.IsAI)
                                {
                                    SetPayCost(RandomUtility.IsSucceedProbability(0.5f));
                                }

                                #endregion
                            }

                            yield return new WaitWhile(() => !endSelect);
                            endSelect = false;

                            GManager.instance.commandText.CloseCommandText();
                            yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                            if (payCost)
                            {
                                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(-1, activateClass));

                                count++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (count >= 1)
                        {
                            int plusDP = 1000 * count;

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: card.PermanentOfThisCard(), changeValue: plusDP, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.OnEndAttack)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Memory +2", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[End of Attack] Gain 2 memory.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.CanAddMemory(activateClass))
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (isExistOnField(card))
                {
                    if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(2, activateClass));
                    }
                }
            }
        }

        return cardEffects;
    }

    bool endSelect = false;
    bool payCost = false;

    [PunRPC]
    public void SetPayCost(bool payCost)
    {
        this.payCost = payCost;
        endSelect = true;
    }
}