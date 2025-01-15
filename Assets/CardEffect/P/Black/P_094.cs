using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class P_094 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Delete Digimon and Tamer so that their play cost total is up to 3", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] Delete your opponent's Digimon and Tamers with a total play cost of 3. For every [Vemmon] in this Digimon's digivolution cards, increase the maximum play cost you can choose by this effect by 1.";
            }

            int maxCost()
            {
                int maxCost = 3;

                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    maxCost += card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardNames.Contains("Vemmon"));
                }

                return maxCost;
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                {
                    if (permanent.IsDigimon || permanent.IsTamer)
                    {
                        if (permanent.TopCard.GetCostItself <= maxCost())
                        {
                            if (permanent.TopCard.HasPlayCost)
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
                return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
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
                if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                {
                    int _maxCost = maxCost();

                    List<Permanent> selectedPermanents = new List<Permanent>();

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

                        if (sumCost > _maxCost)
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

                        if (sumCost > _maxCost)
                        {
                            return false;
                        }

                        return true;
                    }
                }
            }
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Delete Digimon and Tamer so that their play cost total is up to 3", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Delete your opponent's Digimon and Tamers with a total play cost of 3. For every [Vemmon] in this Digimon's digivolution cards, increase the maximum play cost you can choose by this effect by 1.";
            }

            int maxCost()
            {
                int maxCost = 3;

                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    maxCost += card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardNames.Contains("Vemmon"));
                }

                return maxCost;
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                {
                    if (permanent.IsDigimon || permanent.IsTamer)
                    {
                        if (permanent.TopCard.GetCostItself <= maxCost())
                        {
                            if (permanent.TopCard.HasPlayCost)
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
                if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                {
                    int _maxCost = maxCost();

                    List<Permanent> selectedPermanents = new List<Permanent>();

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

                        if (sumCost > _maxCost)
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

                        if (sumCost > _maxCost)
                        {
                            return false;
                        }

                        return true;
                    }
                }
            }
        }

        if (timing == EffectTiming.OnAllyAttack)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Return 2 [Vemmon] from [Galacticmon]'s digivolution cards to the deck bottom to switch attack target to this Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
            activateClass.SetIsInheritedEffect(true);
            activateClass.SetHashString("SwitchAttackTarget_P_094");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Opponent's Turn] [Once Per Turn] When an opponent's Digimon attacks, by placing 2 [Vemmon] from 1 of your [Galacticmon]'s digivolution cards at the bottom of their owners' decks, switch the target of attack to this Digimon.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.CardNames.Contains("Vemmon");
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                {
                    if (permanent.TopCard.CardNames.Contains("Galacticmon"))
                    {
                        if (permanent.DigivolutionCards.Count(CanSelectCardCondition) >= 2)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool PermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsOpponentPermanent(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOpponentTurn(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, PermanentCondition))
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
                    if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                Permanent selectedPermanent = null;

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

                    selectPermanentEffect.SetUpCustomMessage("Select 1 [Galacticmon].", "The opponent is selecting 1 [Galacticmon].");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }
                }

                if (selectedPermanent != null)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition) >= 2)
                    {
                        int maxCount = 2;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 2 [Vemmon] that will return to the bottom of deck\n(cards will be placed back to the bottom of the deck so that cards with lower numbers are on top).",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: selectedPermanent.DigivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select digivolution cards to return to the bottom of deck.", "The opponent is selecting digivolution cards to return to the bottom of deck.");
                        selectCardEffect.SetNotShowCard();
                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        if (selectedCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new ReturnToLibraryBottomDigivolutionCardsClass(selectedPermanent, selectedCards, CardEffectCommons.CardEffectHashtable(activateClass)).ReturnToLibraryBottomDigivolutionCards());

                            if (selectedCards.Count == 2 && CardEffectCommons.IsExistOnBattleArea(card))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SwitchDefender(activateClass, false, card.PermanentOfThisCard()));
                            }
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.None)
        {
            AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
            addDigiXrosConditionClass.SetUpICardEffect($"DigiXros", CanUseCondition, card);
            addDigiXrosConditionClass.SetUpAddDigiXrosConditionClass(getDigiXrosCondition: GetDigiXros);
            addDigiXrosConditionClass.SetNotShowUI(true);
            cardEffects.Add(addDigiXrosConditionClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                return true;
            }



            DigiXrosCondition GetDigiXros(CardSource cardSource)
            {
                if (cardSource == card)
                {
                    List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>();

                    DigiXrosConditionElement element = new DigiXrosConditionElement(CanSelectCardCondition, "Snatchmon");

                    bool CanSelectCardCondition(CardSource cardSource)
                    {
                        if (cardSource != null)
                        {
                            if (cardSource.Owner == card.Owner)
                            {
                                if (cardSource.IsDigimon)
                                {
                                    if (cardSource.CardNames_DigiXros.Contains("Snatchmon"))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }

                        return false;
                    }

                    elements.Add(element);

                    DigiXrosConditionElement element1 = new DigiXrosConditionElement(CanSelectCardCondition1, "Vemmon", true);

                    bool CanSelectCardCondition1(CardSource cardSource)
                    {
                        if (cardSource != null)
                        {
                            if (cardSource.Owner == card.Owner)
                            {
                                if (cardSource.IsDigimon)
                                {
                                    if (cardSource.CardNames_DigiXros.Contains("Vemmon"))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }

                        return false;
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        elements.Add(element1);
                    }

                    DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, null, 1);

                    return digiXrosCondition;
                }

                return null;
            }
        }

        return cardEffects;
    }
}
