using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.BT10
{
    public class BT10_093 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnAddDigivolutionCards)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1 and Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("Draw1Memory+1_BT10_093");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When a purple card is placed under this Tamer, <Draw 1> (Draw 1 card from your deck.) and memory +1.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerOnAddDigivolutionCard(
                                hashtable: hashtable,
                                permanentCondition: permanent => permanent == card.PermanentOfThisCard(),
                                cardEffectCondition: null,
                                cardCondition: cardSource => cardSource.CardColors.Contains(CardColor.Purple)))
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
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            ActivateClass activateClass2 = new ActivateClass();
            activateClass2.SetUpICardEffect("Reduce play cost", CanUseCondition2, card);
            activateClass2.SetUpActivateClass(CanActivateCondition2, ActivateCoroutine2, 1, true, EffectDiscription2());
            activateClass2.SetHashString("Reduce_BT10_093");

            string EffectDiscription2()
            {
                return "[Your turn] [Once per turn] When you would play 1 level 4 or higher Digimon card with [Bagra Army] in its traits, by placing up to 3 purple Digimon cards from under your Tamers in the digivolution cards of the Digimon card played, reduce the memory cost of that Digimon by 2 for each card placed.";
            }

            bool CardCondition(CardSource cardSource)
            {
                if (cardSource.Owner == card.Owner)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.Level >= 4 && cardSource.HasLevel)
                        {
                            if (cardSource.CardTraits.Contains("Bagra Army") || cardSource.CardTraits.Contains("BagraArmy"))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition2(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition))
                        {
                            if (CardEffectCommons.IsOnly1CardPlayed(hashtable))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            bool CanActivateCondition2(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsTamer && permanent.DigivolutionCards.Count((cardSource) => cardSource.IsDigimon && cardSource.CardColors.Contains(CardColor.Purple) && GManager.instance.GetComponent<SelectDigiXrosClass>().addDigivolutionCardInfos.Count((addDigivolutionCardInfo) => addDigivolutionCardInfo.cardSources.Contains(cardSource)) == 0) >= 1))
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine2(Hashtable _hashtable)
            {
                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsTamer && permanent.DigivolutionCards.Count((cardSource) => cardSource.IsDigimon && cardSource.CardColors.Contains(CardColor.Purple)) >= 1))
                {
                    List<CardSource> digivolutionCards = new List<CardSource>();

                    bool CanSelectCardCondition(CardSource cardSource)
                    {
                        if (!digivolutionCards.Contains(cardSource))
                        {
                            if (GManager.instance.GetComponent<SelectDigiXrosClass>().addDigivolutionCardInfos.Count((addDigivolutionCardInfo) => addDigivolutionCardInfo.cardSources.Contains(cardSource)) == 0)
                            {
                                if (cardSource.CardColors.Contains(CardColor.Purple))
                                {
                                    if (cardSource.IsDigimon)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }

                        return false;
                    }

                    bool CanSelectPermanentCondition(Permanent permanent)
                    {
                        if (permanent != null)
                        {
                            if (permanent.TopCard != null)
                            {
                                if (permanent.IsTamer)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                        {
                                            if (permanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
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

                    while (digivolutionCards.Count < 3 && card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                    {
                        if (!card.Owner.isYou && GManager.instance.IsAI)
                        {
                            break;
                        }

                        if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                        {
                            Permanent selectedPermanent = null;

                            int maxCount = Math.Min(1, card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass2);

                            selectPermanentEffect.SetUpCustomMessage("Select a Tamer.", "The opponent is selecting a Tamer.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;

                                yield return null;
                            }

                            if (selectedPermanent == null)
                            {
                                break;
                            }
                            else
                            {
                                if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                                {
                                    maxCount = 3 - digivolutionCards.Count;

                                    if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition) < maxCount)
                                    {
                                        maxCount = selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition);
                                    }

                                    List<CardSource> selectedCards = new List<CardSource>();

                                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                    selectCardEffect.SetUp(
                                                canTargetCondition: CanSelectCardCondition,
                                                canTargetCondition_ByPreSelecetedList: null,
                                                canEndSelectCondition: null,
                                                canNoSelect: () => true,
                                                selectCardCoroutine: SelectCardCoroutine,
                                                afterSelectCardCoroutine: null,
                                                message: "Select digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                                                maxCount: maxCount,
                                                canEndNotMax: true,
                                                isShowOpponent: true,
                                                mode: SelectCardEffect.Mode.Custom,
                                                root: SelectCardEffect.Root.Custom,
                                                customRootCardList: selectedPermanent.DigivolutionCards,
                                                canLookReverseCard: true,
                                                selectPlayer: card.Owner,
                                                cardEffect: activateClass2);

                                    selectCardEffect.SetUpCustomMessage("Select digivolution cards.", "The opponent is selecting digivolution cards.");
                                    //selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Cards");

                                    yield return StartCoroutine(selectCardEffect.Activate());

                                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                                    {
                                        selectedCards.Add(cardSource);

                                        yield return null;
                                    }

                                    if (selectedCards.Count >= 1)
                                    {
                                        foreach (CardSource cardSource in selectedCards)
                                        {
                                            digivolutionCards.Add(cardSource);
                                        }
                                    }
                                }
                            }

                            if (digivolutionCards.Count >= 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(digivolutionCards, "Digivolution Cards", true, true));
                            }
                        }
                    }

                    if (digivolutionCards.Count >= 1)
                    {
                        GManager.instance.GetComponent<SelectDigiXrosClass>().AddDigivolutionCardInfos(new AddDigivolutionCardsInfo(activateClass2, digivolutionCards));

                        int reduceCount = 2 * digivolutionCards.Count;

                        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect($"Play Cost -{reduceCount}", CanUseCondition1, card);
                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                        card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                        yield return new WaitForSeconds(0.2f);

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
                                        Cost -= reduceCount;
                                    }
                                }
                            }

                            return Cost;
                        }

                        bool PermanentsCondition(List<Permanent> targetPermanents)
                        {
                            if (targetPermanents == null)
                            {
                                return true;
                            }
                            else
                            {
                                if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        bool CardSourceCondition(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                if (cardSource.Owner == card.Owner)
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

            if (timing == EffectTiming.BeforePayCost)
            {
                cardEffects.Add(activateClass2);
            }

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => true, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (!card.Owner.isYou && GManager.instance.IsAI)
                    {
                        return false;
                    }

                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (activateClass2 != null)
                            {
                                if (!card.cEntity_EffectController.isOverMaxCountPerTurn(activateClass2, activateClass2.MaxCountPerTurn))
                                {
                                    return true;
                                }
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
                                if (!card.Owner.isYou && GManager.instance.IsAI)
                                {
                                    return Cost;
                                }

                                int maxCount = 0;

                                foreach (Permanent permanent in card.Owner.GetBattleAreaPermanents())
                                {
                                    if (permanent.IsTamer)
                                    {
                                        maxCount += permanent.DigivolutionCards.Count((cardSource) => cardSource.CardColors.Contains(CardColor.Purple) && cardSource.IsDigimon);
                                    }

                                    if (maxCount >= 3)
                                    {
                                        break;
                                    }
                                }

                                if (maxCount >= 3)
                                {
                                    maxCount = 3;
                                }

                                Cost -= 2 * maxCount;
                            }
                        }
                    }

                    return Cost;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    if (targetPermanents == null)
                    {
                        return true;
                    }
                    else
                    {
                        if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.Level >= 4 && cardSource.HasLevel)
                            {
                                if (cardSource.IsDigimon)
                                {
                                    if (cardSource.CardTraits.Contains("Bagra Army"))
                                    {
                                        return true;
                                    }

                                    if (cardSource.CardTraits.Contains("BagraArmy"))
                                    {
                                        return true;
                                    }
                                }
                            }
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

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            return cardEffects;
        }
    }
}