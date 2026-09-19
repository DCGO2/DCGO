using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT7_040 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            ChangeCostClass changeCostClass = new ChangeCostClass();
            changeCostClass.SetUpICardEffect($"Digivolution Cost is", CanUseCondition, card);
            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
            cardEffects.Add(changeCostClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (card.Owner.HandCards.Contains(card))
                {
                    changeCostClass.SetEffectName($"Digivolution Cost is {card.Owner.SecurityCards.Count}");

                    return true;
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
                            int count = card.Owner.SecurityCards.Count;

                            if (count <= 0)
                            {
                                count = 1;
                            }

                            Cost = count;
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
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card);
            }

            bool CardSourceCondition(CardSource cardSource)
            {
                if (cardSource.Owner.HandCards.Contains(cardSource))
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
                return root == SelectCardEffect.Root.Hand;
            }

            bool isUpDown()
            {
                return false;
            }
        }

        if (timing == EffectTiming.OnDeclaration)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Reduce opponent's Digimon DP", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, false, EffectDiscription());
            activateClass.SetHashString("DP-_BT7_040");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] <Digi-Burst 2> (Trash 2 of this Digimon's digivolution cards to activate the effect below.)  - 1 of your opponent's Digimon gains <Security Attack -2> until the end of your opponent's next turn. (This Digimon checks 2 fewer security cards)";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (isExistOnField(card))
                {
                    if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                    {
                        IDigiBurst digiBurst = new IDigiBurst(card.PermanentOfThisCard(), 4, activateClass);

                        digiBurst.SetUpToMaxCount();

                        if (digiBurst.CanDigiBurst())
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
                        IDigiBurst digiBurst = new IDigiBurst(card.PermanentOfThisCard(), 4, activateClass);

                        digiBurst.SetUpToMaxCount();

                        if (digiBurst.CanDigiBurst())
                        {
                            yield return ContinuousController.instance.StartCoroutine(digiBurst.DigiBurst());

                            int minusDP = 3000 * digiBurst.discardedCards.Count;

                            if (minusDP >= 1)
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

                                    selectPermanentEffect.SetUpCustomMessage($"Select 1 Digimon that will get DP -{minusDP}.", $"The opponent is selecting 1 Digimon that will get DP -{minusDP}.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: -minusDP, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
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
