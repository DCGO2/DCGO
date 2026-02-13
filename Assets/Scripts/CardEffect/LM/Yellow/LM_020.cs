using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCGO.CardEffects.LM
{
    public class LM_020 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 Digimon card to top of Security, and return 1 Security to top of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By placing 1 Digimon on the top of its owner's security stack, reveal all of your opponent's security cards, and place 1 card among them on top of your opponent's deck. Shuffle the rest and return them to the security stack.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return true;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent))
                    {
                        if (permanent.TopCard.Owner.CanAddSecurity(activateClass))
                        {
                            return true;
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
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool placedToSecurity = false;

                    List<Permanent> selectedPermanents = new List<Permanent>();

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        maxCount: 1,
                        canEndNotMax: false,
                        mode: SelectPermanentEffect.Mode.Custom,
                        selectPlayer: card.Owner,
                        cardEffect: null);

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 Digimon card to place on the top of Security.",
                        "The opponent is selecting 1 Digimon card to place on the top of Security.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanents.Add(permanent);

                        if (!permanent.IsToken)
                        {
                            Player selectedOwner = permanent.TopCard.Owner;
                            int beforeEffectSecurityCount = permanent.TopCard.Owner.SecurityCards.Count;

                            yield return ContinuousController.instance.StartCoroutine(new IPutSecurityPermanent(permanent, CardEffectCommons.CardEffectHashtable(activateClass), toTop: true).PutSecurity());

                            if (selectedOwner.SecurityCards.Count > beforeEffectSecurityCount)
                                placedToSecurity = true;
                        }

                        yield return null;
                    }

                    if (placedToSecurity)
                    {
                        if (card.Owner.Enemy.SecurityCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(card.Owner.Enemy.SecurityCards, "Security Cards", true, true));

                            int maxCount = 1;
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            CardSource selectedCard = null;

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                message: "Select 1 card to return to top of deck.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Security,
                                customRootCardList: card.Owner.Enemy.SecurityCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage_ShowCard("Return card to top of deck");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCard = cardSource;
                                yield return null;
                            }

                            IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                            {
                                if (cardSources.Count >= 1)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                                        player: card.Owner.Enemy,
                                        refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());
                                }
                            }

                            if (selectedCard != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryTopCards(new List<CardSource>() { selectedCard }));
                            }

                            ContinuousController.instance.PlaySE(GManager.instance.ShuffleSE);

                            card.Owner.Enemy.SecurityCards = RandomUtility.ShuffledDeckCards(card.Owner.Enemy.SecurityCards);
                        }
                    }
                }
            }

            #endregion

            #region Start of Opponent's Turn
            if(timing == EffectTiming.OnStartTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Choose 1 category", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                CardKind selectedCategory = CardKind.Digimon;

                string EffectDiscription()
                {
                    return "[Start of Opponent's Turn] Declare 1 card category. Then, reveal the top card of your opponent's deck. If that card is of the declared category, this Digimon isn't affected by the effects of that card category for the turn. Return the revealed card to the top or bottom of your opponent's deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    yield return GManager.instance.photonWaitController.StartWait("Quantumon_Select_ETB");

                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new SelectionElement<int>(message: $"Digimon", value : 0, spriteIndex: 0),
                            new SelectionElement<int>(message: $"Tamer", value : 1, spriteIndex: 0),
                            new SelectionElement<int>(message: $"Option", value : 2, spriteIndex: 0),
                            new SelectionElement<int>(message: $"Digi-Egg", value: 3, spriteIndex: 0)
                        };

                    string selectPlayerMessage = "Choose a card category you will use?";
                    string notSelectPlayerMessage = "The opponent is choosing which card category to use.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    int actionID = GManager.instance.userSelectionManager.SelectedIntValue;
                    selectedCategory = (CardKind)actionID;

                    if(!card.Owner.isYou)
                        GManager.instance.commandText.OpenCommandText($"The opponent has choosen to use: {selectedCategory}.");

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndProcessForAll(
                        revealCount: 1,
                        simplifiedSelectCardCondition:
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition: (cardSource) => false,
                            message: "",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: -1,
                            selectCardCoroutine: null),
                        remainingCardsPlace: RemainingCardsPlace.DeckTopOrBottom,
                        activateClass: activateClass,
                        revealedCardsCoroutine: RevealedCardsCoroutine,
                        refSelectedCards: null,
                        isOpponentDeck: true
                    ));

                    IEnumerator RevealedCardsCoroutine(List<CardSource> revealedCards)
                    {
                        GManager.instance.commandText.CloseCommandText();

                        if (revealedCards[0].CardKind.Equals(selectedCategory))
                        {
                            CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                            canNotAffectedClass.SetUpICardEffect($"Isn't affected by opponent's {selectedCategory}'s effects", CanUseCondition, card);
                            canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                            selectedPermanent.UntilEachTurnEndEffects.Add((_timing) => canNotAffectedClass);

                            #region Log
                            string log = "";

                            
                            log += $"\n{card.BaseENGCardNameFromEntity}({card.CardID}) Immunity:";
                            log += $"\n{selectedCategory} effects";

                            log += "\n";

                            PlayLog.OnAddLog?.Invoke(log);
                            #endregion

                            bool CardCondition(CardSource cardSource)
                            {
                                return CardEffectCommons.IsExistOnBattleAreaDigimon(cardSource) &&
                                       cardSource.PermanentOfThisCard() == selectedPermanent;
                            }

                            bool SkillCondition(ICardEffect cardEffect)
                            {
                                switch (selectedCategory)
                                {
                                    case CardKind.Digimon:
                                        if (cardEffect.IsDigimonEffect || cardEffect.EffectSourceCard.IsDigimon)
                                        {
                                            return true;
                                        }
                                        break;
                                    case CardKind.Tamer:
                                        if (cardEffect.IsTamerEffect || cardEffect.EffectSourceCard.IsTamer)
                                        {
                                            return true;
                                        }
                                        break;
                                    case CardKind.Option:
                                        if (cardEffect.EffectSourceCard.IsOption || cardEffect.EffectSourceCard.IsOption)
                                        {
                                            return true;
                                        }
                                        break;
                                    case CardKind.DigiEgg:
                                        if (cardEffect.IsDigimonEffect || cardEffect.EffectSourceCard.IsDigiEgg)
                                        {
                                            return true;
                                        }
                                        break;
                                }
                                return false;
                            }
                            yield return null;
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}