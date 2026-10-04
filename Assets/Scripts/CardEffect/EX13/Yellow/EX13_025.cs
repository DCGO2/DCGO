using System.Collections;
using System.Collections.Generic;

// Candlemon
namespace DCGO.CardEffects.EX13
{
    public class EX13_025 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If 3 or more sec, trash top/bot sec, <Draw 1>, gain 1 memory. If 2 or less sec, place [Witchelny] from hand to bot sec.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Start of Your Main Phase] If you have 3 or more security cards, trash your top or bottom security card, <Draw 1> and gain 1 memory. Then, if you have 2 or fewer security cards, you may place 1 card with [Witchelny] in its text from your hand as the bottom security card.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("Witchelny");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 3)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Security Top", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Security Bottom", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Which will you trash the top or bottom card of the security?";
                        string notSelectPlayerMessage = "The opponent is selecting whether to trash the top or bottom card of security.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool fromTop = GManager.instance.userSelectionManager.SelectedBoolValue;

                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            card.Owner,
                            1,
                            activateClass,
                            fromTop).DestroySecurity());

                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));

                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(
                            card.Owner,
                            1,
                            activateClass).Draw());
                    }

                    if (card.Owner.SecurityCards.Count <= 2
                    && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                    {
                        List<CardSource> selectedCard = new List<CardSource>();

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
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

                        selectHandEffect.SetUpCustomMessage(
                            "Select 1 card to place at the bottom of security.",
                            "The opponent is selecting 1 card to place at the bottom of security.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Bottom Card");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(card, toTop: false));        
                        }
                    }
                }
            }
            #endregion

            #region All Turns - Inherited
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 security to stop this Digimon from leaving Battle Area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("EX13_025_Inherited");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon with [Dynasmon] or [Witchelny] in its text would leave the battle area by your opponent's effects, by trashing your top security card, it doesn't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card)
                        && CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOpponentEffect(cardEffect, card));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && (card.PermanentOfThisCard().TopCard.EqualsCardName("Dynasmon")
                            || card.PermanentOfThisCard().TopCard.HasText("Witchelny"))
                        && card.Owner.SecurityCards.Count >= 1;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());


                    Permanent thisCardPermanent = card.PermanentOfThisCard();

                    thisCardPermanent.willBeRemoveField = false;

                    thisCardPermanent.HideDeleteEffect();
                    thisCardPermanent.HideHandBounceEffect();
                    thisCardPermanent.HideDeckBounceEffect();
                    thisCardPermanent.HideWillRemoveFieldEffect();

                    yield return null;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
