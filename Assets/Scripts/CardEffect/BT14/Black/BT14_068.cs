using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT14
{
    public class BT14_068 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                int maxCost = 7;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Delete up to 7 play cost's total worth of your opponent's Digimon.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.GetCostItself <= maxCost)
                        {
                            if (permanent.TopCard.HasPlayCost)
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
                    if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
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
                            if (permanents.Count <= 0)
                            {
                                return false;
                            }

                            int sumCost = 0;

                            foreach (Permanent permanent1 in permanents)
                            {
                                sumCost += permanent1.TopCard.GetCostItself;
                            }

                            if (sumCost > maxCost)
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

                            if (sumCost > maxCost)
                            {
                                return false;
                            }

                            return true;
                        }
                    }
                }
            }

            if (timing == EffectTiming.None)
            {
                bool CanUseCondition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.CardTraits.Contains("D-Brigade"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.BlockerStaticEffect(
                    permanentCondition: PermanentCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition));
            }

            if (timing == EffectTiming.OnEndTurn)
            {
                int maxCost = 7;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("Reveal_BT14_068");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn][Once Per Turn] Reveal the top 3 cards of your deck. You may play up to 7 play cost's total worth of cards with the [D-Brigade] or [DigiPolice] trait among them without paying the costs. Trash the rest.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                    {
                        if (cardSource.GetCostItself <= maxCost)
                        {
                            if (cardSource.HasPlayCost)
                            {
                                if (cardSource.CardTraits.Contains("D-Brigade"))
                                {
                                    return true;
                                }

                                if (cardSource.CardTraits.Contains("DigiPolice"))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

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
                    int maxCount = card.Owner.fieldCardFrames.Count((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame());

                    List<CardSource> selectedCards = new List<CardSource>();

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select cards with the [D-Brigade] or [DigiPolice] trait whose play costs add up to 7 or less.",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: maxCount,
                            selectCardCoroutine: SelectCardCoroutine),
                        },
                        remainingCardsPlace: RemainingCardsPlace.Trash,
                        activateClass: activateClass,
                        canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                        canEndSelectCondition: CanEndSelectCondition,
                        canNoSelect: true,
                        canEndNotMax: true
                    ));

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);
                        yield return null;
                    }

                    bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                    {
                        int sumCost = 0;

                        foreach (CardSource cardSource1 in cardSources)
                        {
                            sumCost += cardSource1.GetCostItself;
                        }

                        sumCost += cardSource.GetCostItself;

                        if (sumCost > maxCost)
                        {
                            return false;
                        }

                        return true;
                    }

                    bool CanEndSelectCondition(List<CardSource> cardSources)
                    {
                        if (cardSources.Count <= 0)
                        {
                            return false;
                        }

                        int sumCost = 0;

                        foreach (CardSource cardSource1 in cardSources)
                        {
                            sumCost += cardSource1.GetCostItself;
                        }

                        if (sumCost > maxCost)
                        {
                            return false;
                        }

                        return true;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: selectedCards,
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Library,
                        activateETB: true));
                }
            }

            return cardEffects;
        }
    }
}