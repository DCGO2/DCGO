using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT15
{
    public class BT15_065 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Numemon") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region On Play - Place a card under this Digimon to gain effects.

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place a card under this Digimon to gain effects.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("CantAttack_BT15_065");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By placing 1 card with [Numemon] in it's name from your trash as this Digimon's bottom digivolution card, all of your opponent's Digimon with a play cost of 5 or less can't attack players until the end of your opponent's turn.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.ContainsCardName("Numemon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (!card.PermanentOfThisCard().IsToken)
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
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        bool added = false;

                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            int maxCount = 1;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to place on bottom of digivolution cards.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");
                            selectCardEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            if (selectedCards.Count >= 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(selectedCards, activateClass));

                                added = true;
                            }
                        }

                        if (added)
                        {
                            bool AttackerCondition(Permanent Attacker)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(Attacker, card))
                                {
                                    if (Attacker.TopCard.GetCostItself <= 5)
                                    {
                                        if (Attacker.TopCard.HasPlayCost)
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
                            }

                            bool DefenderCondition(Permanent Defender)
                            {
                                return Defender == null;
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotAttackPlayerEffect(
                                attackerCondition: AttackerCondition,
                                defenderCondition: DefenderCondition,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass,
                                effectName: "Can't Attack"));
                        }
                    }
                }
            }

            #endregion

            #region On Play - Trash 1 card from hand to DeDigivolve an opponent's Digimon.

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 card from hand to DeDigivolve an opponent's Digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("DeDigivolve_BT15_065");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By trashing 1 card with [Numemon] in it's name in your hand or from the bottom of this Digimon's digivolution cards, [De-Digivolve 1] 1 of your opponent's Digimon.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.ContainsCardName("Numemon"))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.HandCards.Count >= 1)
                        {
                            return true;
                        }

                        if(card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canSelectHand = card.Owner.HandCards.Count(CanSelectCardCondition) >= 1;
                    bool canSelectDigivolutionCards = CardEffectCommons.IsExistOnBattleArea(card)
                    && card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1;

                    if (canSelectHand || canSelectDigivolutionCards)
                    {
                        if (canSelectHand && canSelectDigivolutionCards)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From digivolution cards", value : false, spriteIndex: 1),
                        };

                            string selectPlayerMessage = "From which area do you play a card?";
                            string notSelectPlayerMessage = "The opponent is choosing from which area to play a card.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectHand);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                        bool discarded = false;

                        int discardCount = 1;

                        if (fromHand)
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: discardCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                mode: SelectHandEffect.Mode.Discard,
                                cardEffect: activateClass);

                            yield return StartCoroutine(selectHandEffect.Activate());
                        }
                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                message: "Select 1 card to trash.",
                                maxCount: discardCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Discard,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                canLookReverseCard: false,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to trash.", "The opponent is selecting 1 digivolution card to trash.");

                            yield return StartCoroutine(selectCardEffect.Activate());
                        }

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                discarded = true;

                                yield return null;
                            }
                        }

                        if (discarded)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
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
                                    Permanent selectedPermanent = permanent;

                                    yield return ContinuousController.instance.StartCoroutine(new IDegeneration(selectedPermanent, 1, activateClass).Degeneration());
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving - Trash 1 card from hand to DeDigivolve an opponent's Digimon.

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 card from hand to DeDigivolve an opponent's Digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("DeDigivolve_BT15_065");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By trashing 1 card with [Numemon] in it's name in your hand or from the bottom of this Digimon's digivolution cards, [De-Digivolve 1] 1 of your opponent's Digimon.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.ContainsCardName("Numemon"))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.HandCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canSelectHand = card.Owner.HandCards.Count(CanSelectCardCondition) >= 1;
                    bool canSelectDigivolutionCards = CardEffectCommons.IsExistOnBattleArea(card)
                    && card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1;

                    if (canSelectHand || canSelectDigivolutionCards)
                    {
                        if (canSelectHand && canSelectDigivolutionCards)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From digivolution cards", value : false, spriteIndex: 1),
                        };

                            string selectPlayerMessage = "From which area do you play a card?";
                            string notSelectPlayerMessage = "The opponent is choosing from which area to play a card.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectHand);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                        bool discarded = false;

                        int discardCount = 1;

                        if (fromHand)
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: discardCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                mode: SelectHandEffect.Mode.Discard,
                                cardEffect: activateClass);

                            yield return StartCoroutine(selectHandEffect.Activate());
                        }
                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                message: "Select 1 card to trash.",
                                maxCount: discardCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Discard,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to trash.", "The opponent is selecting 1 digivolution card to trash.");

                            yield return StartCoroutine(selectCardEffect.Activate());
                        }

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                discarded = true;

                                yield return null;
                            }
                        }

                        if (discarded)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
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
                                    Permanent selectedPermanent = permanent;

                                    yield return ContinuousController.instance.StartCoroutine(new IDegeneration(selectedPermanent, 1, activateClass).Degeneration());
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Your Turn - When you have a [Monzaemon] or [Numemon] in play, this Digimon gains Security Atk +1.

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (card.PermanentOfThisCard().TopCard.ContainsCardName("Monzaemon") || card.PermanentOfThisCard().TopCard.ContainsCardName("Numemon"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: true, card: card, condition: Condition));
            }

            #endregion

            return cardEffects;
        }
    }
}