using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT18
{
    public class BT18_033 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 Digimon from trash to play 1 Digimon from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] If your breeding area is empty, by returning 1 Digimon card with the [Three Great Angels] trait from your trash to the bottom of the deck, play this card in your breeding area without paying the cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.HasAngelTraitRestrictive)
                    {
                        if (cardSource.IsDigimon)
                        {                           
                            return true;                            
                        }
                    }                   

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOwnerTurn(card) &&
                           card.Owner.GetBreedingAreaPermanents().Count == 0 &&
                           CardEffectCommons.HasMatchConditionOwnersCardInTrash(card,CanSelectCardCondition) &&
                           card.Owner.HandCards.Contains(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (card.Owner.GetBreedingAreaPermanents().Count == 0)
                    {
                        if (CardEffectCommons.IsExistOnHand(card))
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    CardSource deckBottomCard = null;

                    List<CardSource> cardSourcesPatamon = new List<CardSource>
                    {
                        card
                    };

                    if (card.Owner.TrashCards.Count(CanSelectCardCondition) >= 1)
                    {
                        int maxCount = 1;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                        canTargetCondition: (cardSource) => CanSelectCardCondition(cardSource),
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        message: "Select a card to place at the bottom of the deck",
                        maxCount: maxCount,
                        canEndNotMax: false,
                        isShowOpponent: false,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                        selectCardEffect.SetNotShowCard();
                        selectCardEffect.SetNotAddLog();

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count == 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(cardSources));

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(cardSources, "Deck Bottom Card", true, true));

                                deckBottomCard = cardSources[0];
                            }
                        }
                    }

                    if (deckBottomCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                          cardSources: cardSourcesPatamon,
                          activateClass: activateClass,
                          payCost: false,
                          isTapped: false,
                          root: SelectCardEffect.Root.Hand,
                          activateETB: true,
                          isBreedingArea: true));

                        if (!card.Owner.GetBreedingAreaPermanents().Contains(card.PermanentOfThisCard()))
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(card));
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}