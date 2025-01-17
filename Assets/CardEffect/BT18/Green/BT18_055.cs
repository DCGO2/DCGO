using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT18
{
    public class BT18_055 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region DigiXros
            if (timing == EffectTiming.None)
            {
                AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
                addDigiXrosConditionClass.SetUpICardEffect($"DigiXros -2", CanUseCondition, card);
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
                        DigiXrosConditionElement elementArbormon =
                            new DigiXrosConditionElement(CanSelectCardCondition, "Arbormon");

                        bool CanSelectCardCondition(CardSource conditionCardSource)
                        {
                            if (conditionCardSource != null)
                            {
                                if (conditionCardSource.Owner == card.Owner)
                                {
                                    if (conditionCardSource.IsDigimon)
                                    {
                                        if (conditionCardSource.CardNames_DigiXros.Contains("Arbormon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        DigiXrosConditionElement elementPetaldramon =
                            new DigiXrosConditionElement(CanSelectCardCondition1, "Petaldramon");

                        bool CanSelectCardCondition1(CardSource conditionCardSource)
                        {
                            if (conditionCardSource != null)
                            {
                                if (conditionCardSource.Owner == card.Owner)
                                {
                                    if (conditionCardSource.IsDigimon)
                                    {
                                        if (conditionCardSource.CardNames_DigiXros.Contains("Petaldramon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>()
                            { elementArbormon, elementPetaldramon };

                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, null, 2);

                        return digiXrosCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region All turns Once Per Turn
            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash opponents top security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("TrashSecurity_BT18_055");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When an opponent's Digimon becomes suspended, trash their top security card.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, PermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (card.Owner.Enemy.SecurityCards.Count >= 0)
                        {
                            return true; 
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                    player: card.Owner.Enemy,
                    destroySecurityCount: 1,
                    cardEffect: activateClass,
                    fromTop: true).DestroySecurity());
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return a Digimon or play a Digimon from this Digimons digivolution sources to hand.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When this Digimon would leave the battle area, you may return to the hand or play 1 level 4 or lower green Digimon card from this Digimon's digivolution cards without paying the cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.IsDigimon)
                            {
                                if (cardSource.Level <= 4)
                                {
                                    if (cardSource.CardColors.Contains(CardColor.Green))
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
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canSelectDigivolutionCards = CardEffectCommons.IsExistOnBattleArea(card)
                    && card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1;

                    if (canSelectDigivolutionCards)
                    {
                        if (canSelectDigivolutionCards)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Return to hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Play a Digimon", value : false, spriteIndex: 1),
                        };

                            string selectPlayerMessage = "Do you want to return to hand or play 1 level 4 or lower Digimon from its digivolution sources";
                            string notSelectPlayerMessage = "The opponent is choosing which effect to activate.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool toHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                        List<CardSource> selectedCards = new List<CardSource>();

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        if (toHand)
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                       canTargetCondition: CanSelectCardCondition,
                                       canTargetCondition_ByPreSelecetedList: null,
                                       canEndSelectCondition: null,
                                       canNoSelect: () => true,
                                       selectCardCoroutine: SelectCardCoroutine,
                                       afterSelectCardCoroutine: null,
                                       message: "Select 1 card to return to hand.",
                                       maxCount: 1,
                                       canEndNotMax: false,
                                       isShowOpponent: true,
                                       mode: SelectCardEffect.Mode.AddHand,
                                       root: SelectCardEffect.Root.Custom,
                                       customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                       canLookReverseCard: true,
                                       selectPlayer: card.Owner,
                                       cardEffect: activateClass);

                            yield return StartCoroutine(selectCardEffect.Activate());
                        }

                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectCardCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => true,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 card to play.",
                                        maxCount: 1,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Custom,
                                        customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to play.", "The opponent is selecting 1 digivolution card to play.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                            yield return StartCoroutine(selectCardEffect.Activate());
                        }

                        SelectCardEffect.Root root = SelectCardEffect.Root.Hand;

                        if (!toHand)
                        {
                            root = SelectCardEffect.Root.DigivolutionCards;

                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: selectedCards[0], payCost: false, cardEffect: activateClass))
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                                            cardSources: selectedCards,
                                                            activateClass: activateClass,
                                                            payCost: false,
                                                            isTapped: false,
                                                            root: root,
                                                            activateETB: true));
                            }
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}