using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ExMaquinamon
namespace DCGO.CardEffects.EX11
{
    public class EX11_073 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region DNA Digivolve
            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect($"DNA Digivolution", CanUseCondition, card);
                addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
                addJogressConditionClass.SetNotShowUI(true);
                cardEffects.Add(addJogressConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                JogressCondition GetJogress(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        bool PermanentCondition1(Permanent permanent)
                        {
                            return permanent != null
                                && permanent.TopCard != null
                                && CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && permanent.TopCard.CardColors.Contains(CardColor.Green)
                                && permanent.Levels_ForJogress(card).Contains(6);
                        }

                        bool PermanentCondition2(Permanent permanent)
                        {
                            return permanent != null
                                && permanent.TopCard != null
                                && CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && permanent.TopCard.CardColors.Contains(CardColor.Black)
                                && permanent.Levels_ForJogress(card).Contains(6);
                        }

                        JogressConditionElement[] elements =
                        {
                            new JogressConditionElement(PermanentCondition1, "a level 6 green Digimon"),

                            new JogressConditionElement(PermanentCondition2, "a level 6 black Digimon"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Security Attack +1
            if (timing == EffectTiming.None) cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            #endregion

            #region Blocker
            if (timing == EffectTiming.None) cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            #endregion

            #region Link +2
            if (timing == EffectTiming.None) cardEffects.Add(CardEffectFactory.ChangeSelfLinkMaxStaticEffect(2, false, card, null));
            #endregion
            
            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("You may link up to 3 [Maquinamon] from hand, trash or digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() => "[when Digivolving] If DNA Digivolving, you may link up to 3 [Maquinamon] from your hand, trash or this Digimon's digivolution cards to this Digimon without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsJogress(hashtable)
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition)
                            || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition)
                            || card.PermanentOfThisCard().DigivolutionCards.Some(CanSelectCardCondition));
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Maquinamon")
                        && cardSource.CanLinkToTargetPermanent(card.PermanentOfThisCard(), false);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();
                    List<CardSource> selectedCards = new List<CardSource>();

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    while (selectedCards.Count < 3)
                    {
                        List<CardSource> validHandCards = card.Owner.HandCards.Filter(CanSelectCardCondition).Except(selectedCards).ToList();
                        List<CardSource> validTrashCards = card.Owner.TrashCards.Filter(CanSelectCardCondition).Except(selectedCards).ToList();
                        List<CardSource> validDigivolutionCards = thisPermanent.DigivolutionCards.Filter(CanSelectCardCondition).Except(selectedCards).ToList();
                        int validHandCardCount = validHandCards.Count;
                        int validTrashCardCount = validTrashCards.Count;
                        int validDigivolutionCardCount = validDigivolutionCards.Count;

                        if (validHandCardCount + validTrashCardCount + validDigivolutionCardCount <= 0)//No cards left to pick
                        {
                            goto END_LOOP;
                        }
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                        if (validHandCardCount > 0)
                        {
                            selectionElements.Add(new (message: $"Link from Hand", value : 1, spriteIndex: 0));
                        }
                        if (validTrashCardCount > 0)
                        {
                            selectionElements.Add(new (message: $"Link from Trash", value : 2, spriteIndex: 0));
                        }
                        if (validDigivolutionCardCount > 0)
                        {
                            selectionElements.Add(new (message: $"Link from Digivolution Cards", value : 3, spriteIndex: 0));
                        }
                        selectionElements.Add( new (message: (selectedCards.Count == 0 ? $"Don't link" : $"Finish Linking"), value: 4, spriteIndex: 1));

                        string selectPlayerMessage = "From which area will you link a Maquinamon?";
                        string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                        Debug.Log("Selection Elements Count: " + selectionElements.Count);
                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        switch (GManager.instance.userSelectionManager.SelectedIntValue)
                        {
                            case 1: // From Hand
                            {
                                int maxCount = Math.Min(3-selectedCards.Count, validHandCardCount);

                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: cardSource => validHandCards.Contains(cardSource),
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: true,
                                    canEndNotMax: true,
                                    isShowOpponent: true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectHandEffect.SetUpCustomMessage($"Select up to {maxCount} cards to link.", "The opponent is selecting cards to link.");

                                yield return StartCoroutine(selectHandEffect.Activate());
                                break;
                            }
                            case 2: // From Trash
                            {
                                int maxCount = Math.Min(3-selectedCards.Count, validTrashCardCount);
                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: cardSource => validTrashCards.Contains(cardSource),
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select cards to link",
                                    maxCount: maxCount,
                                    canEndNotMax: true,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage($"Select up to {maxCount} cards to link.", "The opponent is selecting cards to link.");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                                break;
                            }
                            case 3: // From Digivolution Cards
                            {
                                int maxCount = Math.Min(3-selectedCards.Count, validDigivolutionCardCount);

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                            canTargetCondition: cardSource => validDigivolutionCards.Contains(cardSource),
                                            canTargetCondition_ByPreSelecetedList: null,
                                            canEndSelectCondition: null,
                                            canNoSelect: () => true,
                                            selectCardCoroutine: SelectCardCoroutine,
                                            afterSelectCardCoroutine: null,
                                            message: "Select cards to link.",
                                            maxCount: maxCount,
                                            canEndNotMax: true,
                                            isShowOpponent: true,
                                            mode: SelectCardEffect.Mode.Custom,
                                            root: SelectCardEffect.Root.Custom,
                                            customRootCardList: thisPermanent.DigivolutionCards,
                                            canLookReverseCard: true,
                                            selectPlayer: card.Owner,
                                            cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage($"Select up to {maxCount} cards to link.", "The opponent is selecting cards to link.");

                                yield return StartCoroutine(selectCardEffect.Activate());
                                break;
                            }
                            default:
                                goto END_LOOP;
                        }
                    }

                    END_LOOP:;
                    
                    foreach(CardSource linkCard in selectedCards)
                    {
                        yield return ContinuousController.instance.StartCoroutine(thisPermanent.AddLinkCard(linkCard, activateClass));
                    }
                }
            }
            #endregion

            #region End of Opponent's turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash a security and bottom deck a digimon per link card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("EX11_073_EOOT_TRASH_BOUNCE");
                cardEffects.Add(activateClass);

                string EffectDescription() => "[End of Opponent's Turn] [Once Per Turn] For each of this Digimon's link cards, trash your opponent's top security card and return 1 of their digimon to the bottom of the deck.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOpponentTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

                bool OpponentsDigimonCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int effectCount = card.PermanentOfThisCard().LinkedCards.Count;
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: effectCount,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());

                    int maxCount = Math.Min(effectCount, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, OpponentsDigimonCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: OpponentsDigimonCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage($"Select {maxCount} Digimon to return to bottom of deck.", "Opponent is selecting Digimon to return to bottom of deck.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}