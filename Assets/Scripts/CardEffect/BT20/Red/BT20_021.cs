using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.BT20
{
    public class BT20_021 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region DNA Digivolution
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
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && permanent.TopCard.ContainsCardName("Jesmon")
                                && permanent.Levels_ForJogress(card).Contains(6);
                        }

                        bool PermanentCondition2(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && permanent.TopCard.ContainsCardName("Gankoomon")
                                && permanent.Levels_ForJogress(card).Contains(6);
                        }

                        JogressConditionElement[] elements =
                        {
                            new (PermanentCondition1, "a level 6 with [Jesmon] in name"),
                            new (PermanentCondition2, "a level 6 with [Gankoomon] in name")
                        };

                        JogressCondition jogress_condition = new JogressCondition(elements, 0);
                        return jogress_condition;
                    }

                    return null;
                }

            }
            #endregion

            #region BlastDigivolve
            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }
            #endregion


            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                #region OnPlay
                ActivateClass activate_class = new ActivateClass();
                activate_class.SetUpICardEffect("Select 1 card, delete 1 card", CanUseCondition, card);
                activate_class.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activate_class.SetHashString("Delete_BT20_021");
                cardEffects.Add(activate_class);

                string EffectDescription()
                {
                    return "[On Play] [Once Per Turn] Place 1 [Royal Knight] trait card from your hand or trash as this Digimon's bottom digivolution card, delete 1 of your opponent's Digimon with as much or less DP as this digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return (CardEffectCommons.CanTriggerOnPlay(hashtable, card));
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.HasRoyalKnightTraits)
                    {
                        return true;
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return true;
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        bool canSelectHand = card.Owner.HandCards.Count(CanSelectCardCondition) >= 1;
                        bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                        if (canSelectHand || canSelectTrash)
                        {
                            if (canSelectHand && canSelectTrash)
                            {
                                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                                {
                                    new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                                    new SelectionElement<bool>(message: $"From trash", value: false, spriteIndex: 1),
                                };

                                string selectPlayerMessage = "From which area do you select a card?";
                                string notSelectPlayerMessage = "The opponent is choosing from which are to select a card.";

                                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                            }
                            else
                            {
                                GManager.instance.userSelectionManager.SetBool(canSelectHand);
                            }

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                            List<CardSource> selectedCards = new List<CardSource>();

                            IEnumerator SelectCardCoroutine(CardSource card)
                            {
                                selectedCards.Add(card);

                                yield return null;
                            }

                            if (fromHand)
                            {
                                int maxCount = 1;

                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.Custom,
                                    cardEffect: activate_class);

                                selectHandEffect.SetUpCustomMessage(
                                    "Select 1 card to place on bottom of digivolution cards.",
                                    "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                            }
                            else
                            {
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
                                    cardEffect: activate_class);

                                selectCardEffect.SetUpCustomMessage(
                                    "Select 1 card to place on bottom of digivolution cards.",
                                    "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                                selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                            }

                            CardSource selectedCard = null;

                            if (selectedCards.Count >= 1)
                            {
                                if (CardEffectCommons.IsExistOnBattleArea(card))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                                        selectedCards,
                                        activate_class));

                                    selectedCard = selectedCards[0];
                                }
                            }

                            if (selectedCard != null)
                            {
                                List<Permanent> deleteTargets = new List<Permanent>();

                                bool CanSelectForDeletion(Permanent permanent)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                                    {
                                        if (permanent.DP <= card.PermanentOfThisCard().DP)
                                        {
                                            return true;
                                        }
                                    }
                                    return false;
                                }

                                IEnumerator AfterSelectionCoroutine(List<Permanent> permanents)
                                {
                                    deleteTargets = permanents.Clone();

                                    yield return null;
                                }

                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectForDeletion))
                                {
                                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectForDeletion));

                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectForDeletion,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: false,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: null,
                                        afterSelectPermanentCoroutine: AfterSelectionCoroutine,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activate_class);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                                }

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: deleteTargets, activateClass: activate_class, successProcess: null, failureProcess: null));

                            }
                        }
                    }
                }

                #endregion
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                #region When Digivolving
                ActivateClass activate_class = new ActivateClass();
                activate_class.SetUpICardEffect("Select 1 card, delete 1 card", CanUseCondition, card);
                activate_class.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activate_class.SetHashString("Delete_BT20_021");
                cardEffects.Add(activate_class);

                string EffectDescription()
                {
                    return "[When Digivolving] [Once Per Turn] Place 1 [Royal Knight] trait card from your hand or trash as this Digimon's bottom digivolution card, delete 1 of your opponent's Digimon with as much or less DP as this digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card));
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.HasRoyalKnightTraits)
                    {
                        return true;
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return true;
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        bool canSelectHand = card.Owner.HandCards.Count(CanSelectCardCondition) >= 1;
                        bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                        if (canSelectHand || canSelectTrash)
                        {
                            if (canSelectHand && canSelectTrash)
                            {
                                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                                {
                                    new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                                    new SelectionElement<bool>(message: $"From trash", value: false, spriteIndex: 1),
                                };

                                string selectPlayerMessage = "From which area do you select a card?";
                                string notSelectPlayerMessage = "The opponent is choosing from which are to select a card.";

                                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                            }
                            else
                            {
                                GManager.instance.userSelectionManager.SetBool(canSelectHand);
                            }

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                            List<CardSource> selectedCards = new List<CardSource>();

                            IEnumerator SelectCardCoroutine(CardSource card)
                            {
                                selectedCards.Add(card);

                                yield return null;
                            }

                            if (fromHand)
                            {
                                int maxCount = 1;

                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.Custom,
                                    cardEffect: activate_class);

                                selectHandEffect.SetUpCustomMessage(
                                    "Select 1 card to place on bottom of digivolution cards.",
                                    "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                            }
                            else
                            {
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
                                    cardEffect: activate_class);

                                selectCardEffect.SetUpCustomMessage(
                                    "Select 1 card to place on bottom of digivolution cards.",
                                    "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                                selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                            }

                            CardSource selectedCard = null;

                            if (selectedCards.Count >= 1)
                            {
                                if (CardEffectCommons.IsExistOnBattleArea(card))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                                        selectedCards,
                                        activate_class));

                                    selectedCard = selectedCards[0];
                                }
                            }

                            if (selectedCard != null)
                            {
                                List<Permanent> deleteTargets = new List<Permanent>();

                                bool CanSelectForDeletion(Permanent permanent)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                                    {
                                        if (permanent.DP <= card.PermanentOfThisCard().DP)
                                        {
                                            return true;
                                        }
                                    }
                                    return false;
                                }

                                IEnumerator AfterSelectionCoroutine(List<Permanent> permanents)
                                {
                                    deleteTargets = permanents.Clone();

                                    yield return null;
                                }

                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectForDeletion))
                                {
                                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectForDeletion));

                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectForDeletion,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: false,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: null,
                                        afterSelectPermanentCoroutine: AfterSelectionCoroutine,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activate_class);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                                }

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: deleteTargets, activateClass: activate_class, successProcess: null, failureProcess: null));

                            }
                        }
                    }
                }

                #endregion
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                #region When attacking 1
                ActivateClass activate_class = new ActivateClass();
                activate_class.SetUpICardEffect("Select 1 card, delete 1 card", CanUseCondition, card);
                activate_class.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activate_class.SetHashString("Delete_BT20_021");
                cardEffects.Add(activate_class);

                string EffectDescription()
                {
                    return "[When Attacking] [Once Per Turn] By placing 1 [Royal Knight] trait card from your hand or trash as this Digimon's bottom digivolution card, delete 1 of your opponent's Digimon with as much or less DP as this digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return (CardEffectCommons.CanTriggerOnAttack(hashtable, card));
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.HasRoyalKnightTraits)
                    {
                        return true;
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return true;
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        bool canSelectHand = card.Owner.HandCards.Count(CanSelectCardCondition) >= 1;
                        bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                        if (canSelectHand || canSelectTrash)
                        {
                            if (canSelectHand && canSelectTrash)
                            {
                                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                                {
                                    new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                                    new SelectionElement<bool>(message: $"From trash", value: false, spriteIndex: 1),
                                };

                                string selectPlayerMessage = "From which area do you select a card?";
                                string notSelectPlayerMessage = "The opponent is choosing from which are to select a card.";

                                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                            }
                            else
                            {
                                GManager.instance.userSelectionManager.SetBool(canSelectHand);
                            }

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                            List<CardSource> selectedCards = new List<CardSource>();

                            IEnumerator SelectCardCoroutine(CardSource card)
                            {
                                selectedCards.Add(card);

                                yield return null;
                            }

                            if (fromHand)
                            {
                                int maxCount = 1;

                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.Custom,
                                    cardEffect: activate_class);

                                selectHandEffect.SetUpCustomMessage(
                                    "Select 1 card to place on bottom of digivolution cards.",
                                    "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                            }
                            else
                            {
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
                                    cardEffect: activate_class);

                                selectCardEffect.SetUpCustomMessage(
                                    "Select 1 card to place on bottom of digivolution cards.",
                                    "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                                selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                            }

                            CardSource selectedCard = null;

                            if (selectedCards.Count >= 1)
                            {
                                if (CardEffectCommons.IsExistOnBattleArea(card))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                                        selectedCards,
                                        activate_class));

                                    selectedCard = selectedCards[0];
                                }
                            }

                            if (selectedCard != null)
                            {
                                List<Permanent> deleteTargets = new List<Permanent>();

                                bool CanSelectForDeletion(Permanent permanent)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                                    {
                                        if (permanent.DP <= card.PermanentOfThisCard().DP)
                                        {
                                            return true;
                                        }
                                    }
                                    return false;
                                }

                                IEnumerator AfterSelectionCoroutine(List<Permanent> permanents)
                                {
                                    deleteTargets = permanents.Clone();

                                    yield return null;
                                }

                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectForDeletion))
                                {
                                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectForDeletion));

                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectForDeletion,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: false,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: null,
                                        afterSelectPermanentCoroutine: AfterSelectionCoroutine,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activate_class);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                                }

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: deleteTargets, activateClass: activate_class, successProcess: null, failureProcess: null));

                            }
                        }
                    }
                }

                #endregion
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                #region when attacking 2
                ActivateClass activate_class = new ActivateClass();
                activate_class.SetUpICardEffect("Unsuspend, Then for every 2 [Royal Knight] traits in sources, trash opponent's top security", CanUseCondition, card);
                activate_class.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activate_class.SetHashString("Unsuspend_BT20_021");
                cardEffects.Add(activate_class);

                string EffectDescription()
                {
                    return "[When Attacking] [Once per Turn] This Digimon unsuspends. Then, for every 2 [Royal Knight] trait cards in this Digimon's digivolution cards, trash your opponent's top security card";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return (CardEffectCommons.CanTriggerOnAttack(hashtable, card));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activate_class).Unsuspend());

                    int num_security_deletes = Mathf.FloorToInt(selectedPermanent.DigivolutionCards.Count((card)=>(card.HasRoyalKnightTraits))/2);

                    if (num_security_deletes > 0)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner.Enemy,
                            destroySecurityCount: num_security_deletes,
                            cardEffect: activate_class,
                            fromTop: true).DestroySecurity());
                    }
                }
                #endregion
            }

                return cardEffects;
        }
    }
}