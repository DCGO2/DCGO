using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT3_014 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Change origin DP to 1000", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Change the original DP of 1 of your opponent's level 4 or lower Digimon to 1000 for the turn.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.Level <= 4)
                    {
                        if (permanent.TopCard.HasLevel)
                        {
                            return true;
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
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
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

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to chagne origin DP.", "The opponent is selecting 1 Digimon to chagne origin DP.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                Permanent selectedPermanent = permanent;

                                if (selectedPermanent != null)
                                {
                                    ChangeBaseDPClass changeDPClass = new ChangeBaseDPClass();
                                    changeDPClass.SetUpICardEffect("Origin DP is 1000", CanUseCondition1, card);
                                    changeDPClass.SetUpChangeBaseDPClass(changeDPFunc: ChangeDP, permanentCondition: permanentCondition, isUpDownFunc: _isUpDown, isMinusDPFunc: () => false);
                                    selectedPermanent.UntilEachTurnEndEffects.Add((_timing) => changeDPClass);

                                    if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                                    }

                                    bool CanUseCondition1(Hashtable hashtable)
                                    {
                                        if (selectedPermanent.TopCard != null)
                                        {
                                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                            {
                                                return true;
                                            }
                                        }

                                        return false;
                                    }

                                    int ChangeDP(Permanent permanent, int DP)
                                    {
                                        if (permanentCondition(permanent))
                                        {
                                            DP = 1000;
                                        }

                                        return DP;
                                    }

                                    bool permanentCondition(Permanent permanent)
                                    {
                                        if (permanent != null)
                                        {
                                            if (permanent.TopCard != null)
                                            {
                                                if (permanent == selectedPermanent)
                                                {
                                                    return true;
                                                }
                                            }
                                        }

                                        return false;
                                    }

                                    bool _isUpDown()
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.None)
        {
            ChangeCardColorClass changeCardColorClass = new ChangeCardColorClass();
            changeCardColorClass.SetUpICardEffect($"Also treated as yellow", CanUseCondition, card);
            changeCardColorClass.SetUpChangeCardColorClass(ChangeCardColors: ChangeCardColors);

            cardEffects.Add(changeCardColorClass);

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



            List<CardColor> ChangeCardColors(CardSource cardSource, List<CardColor> CardColors)
            {
                if (cardSource == card)
                {
                    CardColors.Add(CardColor.Yellow);
                }

                return CardColors;
            }
        }

        return cardEffects;
    }
}
