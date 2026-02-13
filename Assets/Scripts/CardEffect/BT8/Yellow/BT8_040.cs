using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;


public class BT8_040 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Trash 1 card from hand to get colors and draw 2", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] You may trash 1 card in your hand to treat this Digimon as also having the colors of the trashed card for the turn. Then, if this Digimon has 2 or more colors, <Draw 2>. (Draw 2 cards from your deck.)";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.HandCards.Count >= 1)
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
                        if (card.Owner.HandCards.Count >= 1)
                        {
                            bool discarded = false;

                            CardSource discardedCard = null;

                            int discardCount = 1;

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: (cardSource) => true,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: discardCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                mode: SelectHandEffect.Mode.Discard,
                                cardEffect: activateClass);

                            yield return StartCoroutine(selectHandEffect.Activate());

                            IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                            {
                                if (cardSources.Count >= 1)
                                {
                                    discarded = true;

                                    yield return null;

                                    if (cardSources.Count == 1)
                                    {
                                        discardedCard = cardSources[0];
                                    }
                                }
                            }

                            if (discarded)
                            {
                                if (discardedCard != null)
                                {
                                    List<CardColor> cardColors = new List<CardColor>();

                                    foreach (CardColor cardColor in discardedCard.CardColors)
                                    {
                                        cardColors.Add(cardColor);
                                    }

                                    Permanent selectedPermanent = card.PermanentOfThisCard();

                                    if (cardColors.Count >= 1)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(selectedPermanent));

                                        foreach (CardColor cardColor in cardColors)
                                        {
                                            ChangeCardColorClass changeCardColorClass = new ChangeCardColorClass();
                                            changeCardColorClass.SetUpICardEffect($"Also treated as {DataBase.CardColorNameDictionary[cardColor]}", CanUseCondition1, card);
                                            changeCardColorClass.SetUpChangeCardColorClass(ChangeCardColors: ChangeCardColors);
                                            selectedPermanent.UntilEachTurnEndEffects.Add((_timing) => changeCardColorClass);

                                            bool CanUseCondition1(Hashtable hashtable)
                                            {
                                                if (selectedPermanent.TopCard != null)
                                                {
                                                    return true;
                                                }

                                                return false;
                                            }

                                            List<CardColor> ChangeCardColors(CardSource cardSource, List<CardColor> CardColors)
                                            {
                                                if (cardSource == selectedPermanent.TopCard)
                                                {
                                                    CardColors.Add(cardColor);
                                                }

                                                return CardColors;
                                            }
                                        }
                                    }

                                    if (selectedPermanent.TopCard.CardColors.Count >= 2)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 2, activateClass).Draw());
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
