using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT8_070 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Delete Digimons and Tamers", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] If this Digimon has a red digivolution card, choose any number of your opponent's Digimon. If this Digimon has a black digivolution card, choose any number of your opponent's Tamers. The chosen cards' play costs must add up to 6 or less. Delete the chosen cards.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                {
                    if (permanent.TopCard.GetCostItself <= 6)
                    {
                        if (permanent.TopCard.HasPlayCost)
                        {
                            if (isExistOnField(card))
                            {
                                if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardColors.Contains(CardColor.Red)) >= 1)
                                {
                                    if (permanent.IsDigimon)
                                    {
                                        return true;
                                    }
                                }

                                if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardColors.Contains(CardColor.Black)) >= 1)
                                {
                                    if (permanent.IsTamer)
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
                    if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                int maxCount = card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition);

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                    canEndSelectCondition: CanEndSelectCondition,
                    maxCount: maxCount,
                    canNoSelect: false,
                    canEndNotMax: true,
                    selectPermanentCoroutine: null,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Destroy,
                    cardEffect: activateClass);

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                bool CanEndSelectCondition(List<Permanent> permanents)
                {
                    if (CardEffectCommons.HasNoElement(permanents))
                    {
                        return false;
                    }

                    int sumCost = 0;

                    foreach (Permanent permanent1 in permanents)
                    {
                        sumCost += permanent1.TopCard.GetCostItself;
                    }

                    if (sumCost > 6)
                    {
                        return false;
                    }

                    return true;
                }

                bool CanTargetCondition_ByPreSelecetedList(List<Permanent> permanents, Permanent permanent)
                {
                    int sumCost = 0;

                    foreach (Permanent permanent1 in permanents)
                    {
                        sumCost += permanent1.TopCard.GetCostItself;
                    }

                    sumCost += permanent.TopCard.GetCostItself;

                    if (sumCost > 6)
                    {
                        return false;
                    }

                    return true;
                }
            }
        }

        if (timing == EffectTiming.OnDestroyedAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
            activateClass.SetHashString("Unsuspend_BT8_070");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[All Turns][Once Per Turn] When an opponent's Digimon is deleted, you may unsuspend this Digimon.";
            }

            bool PermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition))
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
                    if (CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard()))
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                Permanent selectedPermanent = card.PermanentOfThisCard();

                yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                    new List<Permanent>() { selectedPermanent },
                    activateClass).Unsuspend());
            }
        }

        return cardEffects;
    }
}
