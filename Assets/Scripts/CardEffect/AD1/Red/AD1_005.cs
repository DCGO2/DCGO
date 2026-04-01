using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Gaiamon
namespace DCGO.CardEffects.AD1
{
    public class AD1_005 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Alternative Digivolution Condition - Ult.
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Ult.");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region App Fusion (Globemon & Charismon)
            if (timing == EffectTiming.None) cardEffects.Add(CardEffectFactory.AddAppfuseMethodByName(new List<string>() { "Globemon", "Charismon" }, card));
            #endregion

            #region Blast Digivolve
            if (timing == EffectTiming.OnCounterTiming) cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            #endregion

            #region Security A. +1
            if (timing == EffectTiming.None) cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            #endregion

            #region Blocker
            if (timing == EffectTiming.None) cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            #endregion

            #region Link +1
            if (timing == EffectTiming.None) cardEffects.Add(CardEffectFactory.ChangeSelfLinkMaxStaticEffect(1, false, card, null));
            #endregion

            #endregion

            #region Shared OP / WD / WA

            string SharedHashString = "AD1_005_OP_WD_WA";

            string SharedEffectName = "You may link up to 2 cards from hand or digivolution cards. Then you may delete 1 Digimon with equal or less DP than this Digimon.";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] You may link up to 2 [Social], [Navi] or [Tool] trait cards from your hand or this Digimon’s digivolution cards to this Digimon without paying the cost. Then, you may delete 1 of your opponent’s Digimon with as much or less DP as this Digimon.";

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card)
                    && (CardEffectCommons.HasMatchConditionOwnersHand(card, IsProperTraitCard)
                        || card.PermanentOfThisCard().DigivolutionCards.Any(IsProperTraitCard)
                        || CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsWeakerOpponentsDigimon));
            }

            bool IsProperTraitCard(CardSource cardSource)
            {
                return (cardSource.EqualsTraits("Social")
                    || cardSource.EqualsTraits("Navi")
                    || cardSource.EqualsTraits("Tool"))
                        && cardSource.CanLinkToTargetPermanent(card.PermanentOfThisCard(), false);
            }

            bool IsWeakerOpponentsDigimon(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.HasDP
                    && permanent.DP < card.PermanentOfThisCard().DP;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Link 2 cards
                Permanent thisPermanent = card.PermanentOfThisCard();
                List<CardSource> selectedCards = new List<CardSource>();

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCards.Add(cardSource);

                    yield return null;
                }

                while (selectedCards.Count < 2)
                {
                    List<CardSource> validHandCards = card.Owner.HandCards.Filter(IsProperTraitCard).Except(selectedCards).ToList();
                    List<CardSource> validDigivolutionCards = thisPermanent.DigivolutionCards.Filter(IsProperTraitCard).Except(selectedCards).ToList();
                    int validHandCardCount = validHandCards.Count;
                    int validDigivolutionCardCount = validDigivolutionCards.Count;

                    if (validHandCardCount + validDigivolutionCardCount <= 0)//No cards left to pick
                    {
                        goto END_LOOP;
                    }

                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                    if (validHandCardCount > 0)
                    {
                        selectionElements.Add(new (message: $"Link from Hand", value : 1, spriteIndex: 0));
                    }
                    if (validDigivolutionCardCount > 0)
                    {
                        selectionElements.Add(new (message: $"Link from Digivolution Cards", value : 2, spriteIndex: 0));
                    }
                    selectionElements.Add( new (message: (selectedCards.Count == 0 ? $"Don't link" : $"Finish Linking"), value: 3, spriteIndex: 1));

                    string selectPlayerMessage = "From which area will you link a card?";
                    string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    switch (GManager.instance.userSelectionManager.SelectedIntValue)
                    {
                        case 1: // From Hand
                        {
                            int maxCount = Math.Min(2-selectedCards.Count, validHandCardCount);

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
                        case 2: // From Digivolution Cards
                        {
                            int maxCount = Math.Min(2-selectedCards.Count, validDigivolutionCardCount);

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
                                        root: SelectCardEffect.Root.DigivolutionCards,
                                        customRootCardList: thisPermanent.DigivolutionCards,
                                        canLookReverseCard: false,
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
                #endregion

                #region Delete 1 Digimon
                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsWeakerOpponentsDigimon))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsWeakerOpponentsDigimon,
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
                #endregion
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition,(hash) => SharedActivateCoroutine(hash, activateClass), 1, true, SharedEffectDescription("On Play"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition,(hash) => SharedActivateCoroutine(hash, activateClass), 1, true, SharedEffectDescription("When Digivolving"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition,(hash) => SharedActivateCoroutine(hash, activateClass), 1, true, SharedEffectDescription("When Attacking"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
