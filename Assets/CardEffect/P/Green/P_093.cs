using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class P_093 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnTappedAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Suspend 1 Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[All Turns] When this Digimon becomes suspended, suspend 1 of your opponent's Digimon.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, (permanent) => permanent == card.PermanentOfThisCard()))
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
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
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
                    mode: SelectPermanentEffect.Mode.Tap,
                    cardEffect: activateClass);

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
            }
        }

        ActivateClass activateClass2 = new ActivateClass();
        activateClass2.SetUpICardEffect("Digivolution Cost -1", CanUseCondition2, card);
        activateClass2.SetUpActivateClass(CanActivateCondition2, ActivateCoroutine2, 1, false, EffectDiscription2());
        activateClass2.SetIsInheritedEffect(true);
        activateClass2.SetNotShowUI(true);
        activateClass2.SetIsBackgroundProcess(true);
        activateClass2.SetHashString("DigivolutionCost-1_P-093");

        string EffectDiscription2()
        {
            return "";
        }

        bool CanUseCondition2(Hashtable hashtable)
        {
            if (CardEffectCommons.IsExistOnBattleArea(card))
            {
                if (CardEffectCommons.IsOwnerTurn(card))
                {
                    if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                    {
                        List<CardSource> evoRootTops = CardEffectCommons.GetEvoRootTopsFromEnterFieldHashtable(
                            hashtable,
                            permanent => permanent.cardSources.Contains(card));

                        if (evoRootTops != null)
                        {
                            if (!evoRootTops.Contains(card))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        bool CanActivateCondition2(Hashtable hashtable)
        {
            return CardEffectCommons.IsExistOnBattleArea(card);
        }

        IEnumerator ActivateCoroutine2(Hashtable _hashtable)
        {
            yield return null;
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            cardEffects.Add(activateClass2);
        }

        if (timing == EffectTiming.None)
        {
            ChangeCostClass changeCostClass = new ChangeCostClass();
            changeCostClass.SetUpICardEffect($"Digivolution Cost -1", CanUseCondition, card);
            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
            changeCostClass.SetIsInheritedEffect(true);
            cardEffects.Add(changeCostClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (!card.cEntity_EffectController.isOverMaxCountPerTurn(activateClass2, activateClass2.MaxCountPerTurn))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
            {
                if (CardSourceCondition(cardSource))
                {
                    if (RootCondition(root))
                    {
                        if (PermanentsCondition(targetPermanents))
                        {
                            Cost -= 1;
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
                return targetPermanent == card.PermanentOfThisCard();
            }

            bool CardSourceCondition(CardSource cardSource)
            {
                return cardSource.Owner == card.Owner;
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

        return cardEffects;
    }
}
