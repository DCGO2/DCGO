using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class P_056 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.BeforePayCost)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Digisorption -2", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            activateClass.SetHashString("Digisorption-3_BT2_045");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "<Digisorption -2> (When one of your Digimon digivolves into this card from your hand, you may suspend 1 of your Digimon to reduce the memory cost of the digivolution by 2.)";
            }

            bool CanSelectCondition_CheckAvailability(Permanent permanent)
            {
                if (card.Owner.CanTapWhenAbsorbEvolution_CheckAvailability(permanent, activateClass))
                {
                    if (permanent.CanSelectBySkill(activateClass))
                    {
                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (card.Owner.CanTapWhenAbsorbEvolution(permanent, activateClass))
                {
                    if (permanent.CanSelectBySkill(activateClass))
                    {
                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (hashtable != null)
                {
                    if (hashtable.ContainsKey("Card"))
                    {
                        if (hashtable["Card"] is CardSource)
                        {
                            CardSource Card = (CardSource)hashtable["Card"];

                            if (Card == card)
                            {
                                if (hashtable.ContainsKey("isEvolution"))
                                {
                                    if (hashtable["isEvolution"] is bool)
                                    {
                                        bool isEvolution = (bool)hashtable["isEvolution"];

                                        if (isEvolution)
                                        {
                                            if (hashtable.ContainsKey("Permanents"))
                                            {
                                                if (hashtable["Permanents"] is List<Permanent>)
                                                {
                                                    List<Permanent> Permanents = (List<Permanent>)hashtable["Permanents"];

                                                    if (Permanents != null)
                                                    {
                                                        if (Permanents.Count((permanent) => permanent.TopCard.Owner == card.Owner && permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent)) >= 1)
                                                        {
                                                            if (GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer.Count((player) => player.GetBattleAreaDigimons().Count(CanSelectCondition_CheckAvailability) >= 1) >= 1)
                                                            {
                                                                return true;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer.Count((player) => player.GetBattleAreaDigimons().Count(CanSelectCondition_CheckAvailability) >= 1) >= 1)
                {
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                #region 
                Hashtable hashtable = new Hashtable();
                hashtable.Add("CardEffect", activateClass);

                foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                {
                    #region 
                    foreach (Permanent permanent1 in player.GetFieldPermanents())
                    {
                        foreach (ICardEffect cardEffect in permanent1.EffectList(EffectTiming.WhenDigisorption))
                        {
                            if (cardEffect is ActivateICardEffect)
                            {
                                if (cardEffect.CanTrigger(hashtable))
                                {
                                    GManager.instance.autoProcessing_CutIn.PutStackedSkill(new SkillInfo(cardEffect, hashtable, EffectTiming.WhenDigisorption));
                                }
                            }
                        }
                    }
                    #endregion

                    #region 
                    foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.WhenDigisorption))
                    {
                        if (cardEffect is ActivateICardEffect)
                        {
                            if (cardEffect.CanTrigger(hashtable))
                            {
                                GManager.instance.autoProcessing_CutIn.PutStackedSkill(new SkillInfo(cardEffect, hashtable, EffectTiming.WhenDigisorption));
                            }
                        }
                    }
                    #endregion
                }

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.TriggeredSkillProcess(false, AutoProcessing.HasExecutedSameEffect));
                #endregion

                if (GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer.Count((player) => player.GetBattleAreaDigimons().Count(CanSelectPermanentCondition) >= 1) >= 1)
                {
                    int maxCount = 1;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        yield return null;

                        if (permanents.Count >= 1)
                        {
                            if (card.Owner.CanReduceCost(new List<Permanent>() { new Permanent(new List<CardSource>()) }, card))
                            {
                                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                            }

                            ChangeCostClass changeCostClass = new ChangeCostClass();
                            changeCostClass.SetUpICardEffect("Digivolution Cost -2", CanUseCondition1, card);
                            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                            card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                return true;
                            }

                            int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                            {
                                if (CardSourceCondition(cardSource))
                                {
                                    if (RootCondition(root))
                                    {
                                        if (PermanentsCondition(targetPermanents))
                                        {
                                            Cost -= 2;
                                        }
                                    }
                                }

                                return Cost;
                            }

                            bool PermanentsCondition(List<Permanent> targetPermanents)
                            {
                                if (targetPermanents != null)
                                {
                                    if (targetPermanents.Count(PermanentCondition) >= 1)
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            bool PermanentCondition(Permanent targetPermanent)
                            {
                                if (targetPermanent.TopCard != null)
                                {
                                    if (targetPermanent.TopCard.Owner == card.Owner)
                                    {
                                        if (targetPermanent.TopCard.Owner.GetBattleAreaPermanents().Contains(targetPermanent))
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
                            }

                            bool CardSourceCondition(CardSource cardSource)
                            {
                                if (cardSource != null)
                                {
                                    if (cardSource == card)
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            bool RootCondition(SelectCardEffect.Root root)
                            {
                                return true;
                            }

                            bool isUpDown()
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Opponent's 1 Digimon can't Attack or Block", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] If you have a Tamer in play, 1 of your opponent's Digimon can't attack or block until the end of their next turn.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsTamer))
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
                    if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                    {
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

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get effects.", "The opponent is selecting 1 Digimon that will get effects.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                Permanent selectedPermanent = permanent;

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotAttack(targetPermanent: selectedPermanent, defenderCondition: null, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass, effectName: "Can't Attack"));

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotBlock(targetPermanent: selectedPermanent, attackerCondition: null, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass, effectName: "Can't Block"));
                            }
                        }
                    }
                }
            }
        }

        return cardEffects;
    }
}
