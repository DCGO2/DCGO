using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;


public class P_074 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.BeforePayCost)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Trash your Security to reduce digivolution cost", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            activateClass.SetHashString("TrashSecurity_P_074");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Your Turn] When this Digimon would digivolve into a card with [Shaman] or [Wizard] in its traits, you may trash up to 3 of your security cards. For each security card trashed with this effect, reduce the digivolution cost by 1.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        bool CardCondition(CardSource cardSource)
                        {
                            return cardSource.CardTraits.Contains("Shaman") || cardSource.CardTraits.Contains("Wizard");
                        }

                        if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolveOfCard(hashtable, CardCondition, card))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        if (card.Owner.CanReduceSecurity())
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                int trashCount = 0;

                if (card.Owner.SecurityCards.Count >= 1 && card.Owner.CanReduceSecurity())
                {
                    int maxCount = Math.Min(3, card.Owner.SecurityCards.Count);

                    SelectCountEffect selectCountEffect = GManager.instance.GetComponent<SelectCountEffect>();

                    selectCountEffect.SetUp(
                        SelectPlayer: card.Owner,
                        targetPermanent: null,
                        MaxCount: maxCount,
                        CanNoSelect: true,
                        Message: "How many security cards will you trash?",
                        Message_Enemy: "The opponent is choosing how many security cards to trash.",
                        SelectCountCoroutine: SelectCountCoroutine);

                    yield return ContinuousController.instance.StartCoroutine(selectCountEffect.Activate());

                    IEnumerator SelectCountCoroutine(int count)
                    {
                        trashCount = count;
                        yield return null;
                    }
                }

                if (trashCount >= 1 && card.Owner.SecurityCards.Count >= 1)
                {
                    int reduceCost = 0;

                    if (card.Owner.SecurityCards.Count < trashCount)
                    {
                        trashCount = card.Owner.SecurityCards.Count;
                    }

                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                    player: card.Owner,
                    destroySecurityCount: trashCount,
                    cardEffect: activateClass,
                    fromTop: true).DestroySecurity());

                    reduceCost = trashCount;

                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect($"Digivolution Cost -{reduceCost}", CanUseCondition1, card);
                    changeCostClass.SetUpChangeCostClass(
                        changeCostFunc: ChangeCost,
                        cardSourceCondition: CardSourceCondition,
                        rootCondition: RootCondition,
                        isUpDown: isUpDown,
                        isCheckAvailability: () => false,
                        isChangePayingCost: () => true);
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
                                    Cost -= reduceCost;
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
                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            if (targetPermanent == card.PermanentOfThisCard())
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    bool CardSourceCondition(CardSource cardSource)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            return true;
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

        if (timing == EffectTiming.None)
        {
            ChangeCostClass changeCostClass = new ChangeCostClass();
            changeCostClass.SetUpICardEffect($"Digivolution Cost -", CanUseCondition, card);
            changeCostClass.SetUpChangeCostClass(
                changeCostFunc: ChangeCost,
                cardSourceCondition: CardSourceCondition,
                rootCondition: RootCondition,
                isUpDown: isUpDown,
                isCheckAvailability: () => true,
                isChangePayingCost: () => true);
            changeCostClass.SetNotShowUI(true);
            cardEffects.Add(changeCostClass);

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

            int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
            {
                if (CardSourceCondition(cardSource))
                {
                    if (RootCondition(root))
                    {
                        if (PermanentsCondition(targetPermanents))
                        {
                            int reduceCost = 3;

                            if (card.Owner.SecurityCards.Count < reduceCost)
                            {
                                reduceCost = card.Owner.SecurityCards.Count;
                            }

                            Cost -= reduceCost;
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
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (targetPermanent == card.PermanentOfThisCard())
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CardSourceCondition(CardSource cardSource)
            {
                if (cardSource.CardTraits.Contains("Shaman"))
                {
                    return true;
                }

                if (cardSource.CardTraits.Contains("Wizard"))
                {
                    return true;
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

        if (timing == EffectTiming.OnAllyAttack)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
            activateClass.SetIsInheritedEffect(true);
            activateClass.SetHashString("Unsuspend_P_074");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Attacking][Once Per Turn] If you have exactly 3 security cards, unsuspend this Digimon.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.SecurityCards.Count == 3)
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
