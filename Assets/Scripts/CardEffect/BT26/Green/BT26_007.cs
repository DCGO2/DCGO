using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Swipemon
namespace DCGO.CardEffects.BT26
{
    public class BT26_007 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Inherit
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Link 1 [Seven Code] card to this Digimon for -2", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT26_007_Inherit");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] [Once Per Turn] You may link 1 [Seven Code] trait Digimon card from your hand or this Digimon's digivolution cards to this Digimon with the cost reduced by 2.";

                bool CanLinkCardCondition(CardSource cardSource, bool payCost)
                    => cardSource.IsDigimon
                        && cardSource.EqualsTraits("Seven Code")
                        && cardSource.CanLink(payCost);

                bool CanLinkCardActivateCondition(CardSource cardSource) => CanLinkCardCondition(cardSource, false);
                bool CanLinkCardEffectCondition(CardSource cardSource) => CanLinkCardCondition(cardSource, true);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, CanLinkCardActivateCondition)
                            || card.PermanentOfThisCard().DigivolutionCards.Any(CanLinkCardActivateCondition));

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanLinkCardEffectCondition);
                    bool canSelectSource = card.PermanentOfThisCard().DigivolutionCards.Any(CanLinkCardEffectCondition);

                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                    if (canSelectHand)
                        selectionElements.Add(new SelectionElement<int>(message: "From hand", value: 1, spriteIndex: 0));
                    if (canSelectSource)
                        selectionElements.Add(new SelectionElement<int>(message: "From this Digimon's digivolution cards", value: 2, spriteIndex: 0));
                    selectionElements.Add(new SelectionElement<int>(message: "Do not link", value: 3, spriteIndex: 1));

                    GManager.instance.userSelectionManager.SetIntSelection(
                        selectionElements: selectionElements,
                        selectPlayer: card.Owner,
                        selectPlayerMessage: "From which area will you link a card?",
                        notSelectPlayerMessage: "The opponent is choosing from which area to select a card.");

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool doLink = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                    bool fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;

                    if (!doLink) yield break;

                    ICardEffect GetReduceLinkCostEffect(EffectTiming _timing) => _timing == EffectTiming.None
                        ? CardEffectFactory.GrantedReduceLinkCostClass(card: card, reducedCost: 2, cardSourceCondition: _ => true, permanentCondition: _ => true, rootCondition: _ => true)
                        : null;

                    card.Owner.UntilCalculateFixedCostEffect.Add(GetReduceLinkCostEffect);

                    CardSource selectedCard = null;

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (fromHand)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanLinkCardEffectCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 card to link.", "The opponent is selecting 1 card to link.");

                        yield return StartCoroutine(selectHandEffect.Activate());
                    }
                    else
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanLinkCardEffectCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 digivolution card to link.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.DigivolutionCards,
                            customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to link.", "The opponent is selecting 1 card to link.");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }

                    if (selectedCard != null && card.PermanentOfThisCard() != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            new ILinkCard(true, selectedCard, card.PermanentOfThisCard(), activateClass).LinkCard());
                    }

                    card.Owner.UntilCalculateFixedCostEffect.Remove(GetReduceLinkCostEffect);
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
