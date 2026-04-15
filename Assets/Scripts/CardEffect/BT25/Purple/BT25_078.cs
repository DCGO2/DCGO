using System.Collections;
using System.Collections.Generic;

// Gazimon
namespace DCGO.CardEffects.BT25
{
    public class BT25_078 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.HasTSTraits || permanent.TopCard.HasText("Three Musketeers");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 0, false, card, null, level: 2));
            }
            #endregion

            #region Shared WM / OP

            bool CanSelectCardCondition(CardSource cardSource)
                => cardSource.HasText("Three Musketeers");

            string SharedEffectName = "Reveal top 3, Add 1 card with [Three Musketeers] in text to hand or place 1 [Three Musketeers] trait as bottom digivolution card ";

            string SharedEffectDescription(string tag) => $"[{tag}] Reveal the top 3 cards of your deck. Among them, add 1 card with [Three Musketeers] in its text to the hand, or you may place 1 [Three Musketeers] trait card as this Digimon's bottom digivolution card. Return the rest to the bottom of the deck.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                CardSource selectedCard = null;

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                    revealCount: 3,
                    simplifiedSelectCardConditions:
                    new SimplifiedSelectCardConditionClass[]
                    {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 card with either [Three Musketeers] in text or in trait",
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
                    bool hasThreeMusketeersTrait = selectedCard.HasThreeMusketeersTraits;

                    if (hasThreeMusketeersTrait)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                            {
                                new SelectionElement<bool>(message: $"Add to hand", value : true, spriteIndex: 0),
                                new SelectionElement<bool>(message: $"Place on digivolution cards", value : false, spriteIndex: 0),
                            };

                        string selectPlayerMessage = "Which effect will you select?";
                        string notSelectPlayerMessage = "The opponent is choosing effects.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                    }
                    else
                    {
                        GManager.instance.userSelectionManager.SetBool(true);
                    }

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                    bool addToHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                    if (addToHand)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { selectedCard }, "Cards added to hand", true, true));
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { selectedCard }, false, activateClass));
                    }
                    else
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { selectedCard }, "Cards placed on digivolution cards", true, true));
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass));
                    }
                }
            }


            CardEffectFactory.ActivateClassesForSharedEffects
                    (ref cardEffects, timing, card,
                        SharedEffectName,
                        SharedActivateCoroutine,
                        SharedEffectDescription,
                        optional: false,
                        maxCountPerTurn: 1,
                        whenMoving: true,
                        onPlay: true);

            #endregion

            #region Inheritted
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.RetaliationSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}