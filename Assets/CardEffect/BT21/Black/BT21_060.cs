using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT21
{
    //Destromon
    public class BT21_060 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if ((targetPermanent.TopCard.EqualsCardName("Vemmon")))
                    {
                        return true;
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 6, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Can't trash stacked cards, then de-digivolve", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Until your opponent's turn ends, their effects can't trash this Digimon's top stacked cards. Then, to 1 of your opponent's Digimon, <De-Digivolve 1> for every 2 [Vemmon] in this Digimon's digivolution cards.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool SelectableOpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                int VemmonInSourceDividedBy2()
                {
                    int vemmonCounter = 0;

                    foreach (CardSource cardSource in card.PermanentOfThisCard().DigivolutionCards)
                    {
                        if (cardSource.EqualsCardName("Vemmon"))
                        {
                            vemmonCounter++;
                        }
                    }

                    return vemmonCounter / 2;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    if (selectedPermanent != null)
                    {
                        ImmuneStackTrashingClass immuneFromStackTrashingClass = new ImmuneStackTrashingClass();
                        immuneFromStackTrashingClass.SetUpICardEffect("Isn't affected by trashing any stacked card", CanUseCondition1, selectedPermanent.TopCard);
                        immuneFromStackTrashingClass.SetUpImmuneFromStackTrashingClass(PermanentCondition: PermanentCondition, EffectCondition: EffectCondition);
                        selectedPermanent.UntilOpponentTurnEndEffects.Add((_timing) => immuneFromStackTrashingClass);

                        bool CanUseCondition1(Hashtable hashtable1)
                        {
                            return selectedPermanent.TopCard != null;
                        }

                        bool EffectCondition(ICardEffect effect)
                        {
                            return CardEffectCommons.IsOpponentEffect(effect, card);
                        }

                        bool PermanentCondition(Permanent permanent)
                        {
                            return permanent == selectedPermanent;
                        }
                    }

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: SelectableOpponentsDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve.", "The opponent is selecting 1 Digimon to De-Digivolve.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        for (int i = 0; i < VemmonInSourceDividedBy2(); i++)
                            yield return ContinuousController.instance.StartCoroutine(new IDegeneration(permanent, 1, activateClass).Degeneration());
                    }
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Vemmon] from source without paying the cost when leaving battle area.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When this Digimon would leave the battle area, you may play 1 [Vemmon] from its digivolution cards without paying the cost.";
                }

                bool CanSelectSourceCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Vemmon") &&
                        CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                        CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                        card.PermanentOfThisCard().DigivolutionCards.Some(CanSelectSourceCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectSourceCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 digivolution card to play.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Custom,
                        customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage(
                        "Select 1 digivolution card to play.",
                        "The opponent is selecting 1 digivolution card to play.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.DigivolutionCards,
                            activateETB: true));
                }
            }

            #endregion

            #region Opponent's Turn Inherited
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 2 [Vemmon] from source to bottom of deck to end the attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("end-attack-BT21-060");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Opponent's Turn] [Once Per Turn] When one of your opponent's Digimon attacks, by returning 2 [Vemmon] from this Digimon's digivolution cards to the bottom of the deck, end that attack.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsOpponentPermanent(permanent, card);
                }

                bool CanSelectVemmon(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Vemmon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                        CardEffectCommons.IsOpponentTurn(card) &&
                        CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                        card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectVemmon) >= 2;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectVemmon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => false,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        message: "Select 2 [Vemmon] to place at the bottom of the deck.",
                        maxCount: 2,
                        canEndNotMax: false,
                        isShowOpponent: false,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Custom,
                        customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetNotShowCard();
                    selectCardEffect.SetNotAddLog();

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        selectedCards = cardSources;

                        yield return null;
                    }

                    if(selectedCards.Count > 0)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new ReturnToLibraryBottomDigivolutionCardsClass(card.PermanentOfThisCard(), selectedCards, CardEffectCommons.CardEffectHashtable(activateClass)).ReturnToLibraryBottomDigivolutionCards());

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(selectedCards, "Deck Bottom Cards", true, true));

                        GManager.instance.attackProcess.IsEndAttack = true;
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}