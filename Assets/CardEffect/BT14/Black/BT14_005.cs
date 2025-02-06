using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT14
{
    public class BT14_005 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return cards from trash to gain DP +2000", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("ReturnCards_BT14_005");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking][Once Per Turn] By returning 3 cards with the [D-Brigade] or [DigiPolice] trait from your trash to the top of the deck, this Digimon gets +2000 DP for the turn.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("D-Brigade"))
                    {
                        return true;
                    }

                    if (cardSource.CardTraits.Contains("DigiPolice"))
                    {
                        return true;
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
                        if (card.Owner.TrashCards.Count(CanSelectCardCondition) >= 3)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.TrashCards.Count(CanSelectCardCondition) >= 3)
                    {
                        int maxCount = 3;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                        canTargetCondition: (cardSource) => CanSelectCardCondition(cardSource),
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        message: "Select cards to place at the top of the deck\n(cards will be placed back to the top of the deck so that cards with lower numbers are on top).",
                        maxCount: maxCount,
                        canEndNotMax: false,
                        isShowOpponent: false,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                        selectCardEffect.SetNotAddLog();
                        selectCardEffect.SetUpCustomMessage_ShowCard("Deck Top Cards");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count == 3)
                            {
                                cardSources.Reverse();

                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryTopCards(cardSources));

                                if (CardEffectCommons.IsExistOnBattleArea(card))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                        targetPermanent: card.PermanentOfThisCard(),
                                        changeValue: 2000,
                                        effectDuration: EffectDuration.UntilEachTurnEnd,
                                        activateClass: activateClass));
                                }
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}