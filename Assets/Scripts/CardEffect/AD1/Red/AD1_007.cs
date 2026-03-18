using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Siriusmon
namespace DCGO.CardEffects.AD1
{
    public class AD1_007 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasText("Gammamon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    level: 5,
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region Raid

            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region Security A. +1

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region Blocker

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region Shared WD/WA

            string SharedHashString = "AD1_007_WD_WA";

            string SharedEffectName = "By placing 3 digivolution sources under this Digimon, delete 1 opponent's Digimon with equal or less DP";

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] By placing 3 Digimon cards with [Gammamon] in their texts from your hand or trash as this Digimon's top or bottom digivolution cards, delete 1 of your opponent's Digimon with as much or less DP as this Digimon.";

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                    && (CardEffectCommons.MatchConditionOwnersCardCountInHand(card, GammamonInText) + CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, GammamonInText) >= 3);
            }

            bool GammamonInText(CardSource cardSource)
            {
                return cardSource.IsDigimon
                    && cardSource.HasText("Gammamon");
            }

            bool ValidOpponentDigimon(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.HasDP
                    && permanent.DP <= card.PermanentOfThisCard().DP;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
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
                    List<CardSource> validHandCards = card.Owner.HandCards.Filter(GammamonInText).Except(selectedCards).ToList();
                    List<CardSource> validTrashCards = card.Owner.TrashCards.Filter(GammamonInText).Except(selectedCards).ToList();
                    int validHandCardCount = validHandCards.Count;
                    int validTrashCardCount = validTrashCards.Count;

                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                    if (validHandCardCount > 0)
                    {
                        selectionElements.Add(new(message: $"from Hand", value: 1, spriteIndex: 0));
                    }
                    if (validTrashCardCount > 0)
                    {
                        selectionElements.Add(new(message: $"from Trash", value: 2, spriteIndex: 0));
                    }

                    string selectPlayerMessage = "From which area will you select a card to place under as digivolution cards?";
                    string notSelectPlayerMessage = "The opponent is choosing from which area to select a card to place under as digivolution cards.";

                    Debug.Log("Selection Elements Count: " + selectionElements.Count);
                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    switch (GManager.instance.userSelectionManager.SelectedIntValue)
                    {
                        case 1: // From Hand
                            {
                                int maxCount = Math.Min(3 - selectedCards.Count, validHandCardCount);

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

                                selectHandEffect.SetUpCustomMessage($"Select up to {maxCount} cards to place under as digivolution cards.", "The opponent is selecting cards to place under as digivolution cards.");

                                yield return StartCoroutine(selectHandEffect.Activate());
                                break;
                            }
                        case 2: // From Trash
                            {
                                int maxCount = Math.Min(3 - selectedCards.Count, validTrashCardCount);
                                SelectCardEffect selectCardEffect1 = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect1.SetUp(
                                    canTargetCondition: cardSource => validTrashCards.Contains(cardSource),
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select cards",
                                    maxCount: maxCount,
                                    canEndNotMax: true,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect1.SetUpCustomMessage($"Select up to {maxCount} cards to place under as digivolution cards.", "The opponent is selecting cards to place under as digivolution cards.");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect1.Activate());
                                break;
                            }
                    }
                }

                List<SelectionElement<bool>> selectionElements1 = new List<SelectionElement<bool>>()
                    {
                        new SelectionElement<bool>(message: $"To Top", value : true, spriteIndex: 0),
                        new SelectionElement<bool>(message: $"To Bottom", value : false, spriteIndex: 1),
                    };

                string selectPlayerMessage1 = "From which area do you select a card?";
                string notSelectPlayerMessage1 = "The opponent is choosing from which area to select a card.";

                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage1, notSelectPlayerMessage: notSelectPlayerMessage1);

                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                bool ToTop = GManager.instance.userSelectionManager.SelectedBoolValue;

                selectCardEffect.SetUp(
                    canTargetCondition: (cardSource) => true,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    canNoSelect: () => false,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: AfterSelectCardCoroutine1,
                    message: "Specify the order to place the cards in the digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                    maxCount: selectedCards.Count,
                    canEndNotMax: false,
                    isShowOpponent: false,
                    mode: SelectCardEffect.Mode.Custom,
                    root: SelectCardEffect.Root.Custom,
                    customRootCardList: selectedCards,
                    canLookReverseCard: true,
                    selectPlayer: card.Owner,
                    cardEffect: activateClass);

                selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Cards");

                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                {
                    if (ToTop)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsTop(selectedCards, activateClass));
                    }
                    else
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(selectedCards, activateClass));
                    }

                    yield return null;
                }

                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, ValidOpponentDigimon))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: ValidOpponentDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), 1, true, SharedEffectDescription("When Digivolving"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), 1, true, SharedEffectDescription("When Attacking"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
            }

            #endregion

            #region End of Your Turn

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Attack with this Digimon without suspending", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("AD1_007_EoYT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[End of Your Turn] [Once Per Turn] This Digimon with 5 or more digivolution cards may attack without suspending.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && card.PermanentOfThisCard().DigivolutionCards.Count >= 5
                        && card.PermanentOfThisCard().CanAttack(activateClass, withoutTap: true);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                    selectAttackEffect.SetUp(
                        attacker: card.PermanentOfThisCard(),
                        canAttackPlayerCondition: () => true,
                        defenderCondition: (permanent) => true,
                        cardEffect: activateClass);

                    selectAttackEffect.SetWithoutTap();
                    yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
