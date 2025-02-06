using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT1_078 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnAllyAttack)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Attacking] Reveal 3 cards from the top of your deck. You can digivolve this card into 1 level 6 green Digimon card among them without paying its memory cost. Place the remaining cards at the bottom of your deck in any order. ";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.IsDigimon)
                {
                    if (cardSource.Level == 6)
                    {
                        if (cardSource.CardColors.Contains(CardColor.Green))
                        {
                            if (CardEffectCommons.IsExistOnBattleArea(card))
                            {
                                if (cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, false, activateClass))
                                {
                                    if (cardSource.HasLevel)
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
                return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.LibraryCards.Count >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                CardSource selectedCard = null;

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                    revealCount: 3,
                    simplifiedSelectCardConditions:
                    new SimplifiedSelectCardConditionClass[]
                    {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 level 6 green Digimon card.",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: 1,
                            selectCardCoroutine: SelectCardCoroutine),
                    },
                    remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                    activateClass: activateClass,
                    canNoSelect: true
                ));

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCard = cardSource;
                    yield return null;
                }

                if (selectedCard != null)
                {
                    if (selectedCard.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, false, activateClass))
                    {
                        yield return ContinuousController.instance.StartCoroutine(new PlayCardClass(
                            cardSources: new List<CardSource>(){selectedCard},
                            hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                            payCost: false,
                            targetPermanent: card.PermanentOfThisCard(),
                            isTapped: false,
                            root: SelectCardEffect.Root.Library,
                            activateETB: true).PlayCard());
                    }
                }
            }
        }

        return cardEffects;
    }
}
