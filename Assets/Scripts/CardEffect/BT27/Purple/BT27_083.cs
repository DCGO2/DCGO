using System.Collections;
using System.Collections.Generic;

// Ravemon / Ame-no-Ohabari
namespace DCGO.CardEffects.BT27
{
    public class BT27_083 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Effects
            #region Alternate Digivolution Requirement - [Crowmon]
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Crowmon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Alternate Digivolution Requirement - Lv.5 w/[DATA SQUAD] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasDataSquadTraits;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Retaliation
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.RetaliationSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent trashes 1 card from hand or security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Digivolving] Your opponent trashes any 1 card from their hand or security stack.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && (card.Owner.Enemy.HandCards.Count >= 1 || card.Owner.Enemy.SecurityCards.Count >= 1);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canTrashHand = card.Owner.Enemy.HandCards.Count >= 1;
                    bool canTrashSecurity = card.Owner.Enemy.SecurityCards.Count >= 1;
                    bool trashFromHand = canTrashHand;

                    if (canTrashHand && canTrashSecurity)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: "From hand", value: true, spriteIndex: 0),
                            new SelectionElement<bool>(message: "From security", value: false, spriteIndex: 1),
                        };

                        GManager.instance.userSelectionManager.SetBoolSelection(
                            selectionElements: selectionElements,
                            selectPlayer: card.Owner.Enemy,
                            selectPlayerMessage: "From which area will you trash 1 card?",
                            notSelectPlayerMessage: "The opponent is choosing from which area to trash 1 card.");

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        trashFromHand = GManager.instance.userSelectionManager.SelectedBoolValue;
                    }

                    if (trashFromHand)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner.Enemy,
                            canTargetCondition: _ => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    }
                    else
                    {
                        // Security selection follows BT24_018, but the opponent picks which of their own security cards to trash.
                        CardSource selectedSecurityCard = null;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: _ => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: SelectSecurityCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 security card to trash.\n (cards to the left are at the top and cards to the right are at the bottom)",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Security,
                            customRootCardList: card.Owner.Enemy.SecurityCards,
                            canLookReverseCard: false,
                            selectPlayer: card.Owner.Enemy,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage_ShowCard("Send to trash");

                        selectCardEffect.SetUseFaceDown();

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectSecurityCardCoroutine(CardSource cardSource)
                        {
                            selectedSecurityCard = cardSource;
                            yield return null;
                        }

                        if (selectedSecurityCard != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                player: card.Owner.Enemy,
                                card: selectedSecurityCard,
                                cardEffect: activateClass).DestroySecurity());
                        }
                    }
                }
            }
            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If opponent has 7 or fewer hand cards, play 1 cost 7 or lower purple [Avian]/[Bird] Digimon from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Deletion] If your opponent's hand has 7 or fewer cards, you may play 1 play cost 7 or lower purple Digimon card with [Avian] or [Bird] in any of its traits from your trash without paying the cost.";

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.IsDigimon
                        && cardSource.HasCardColor(CardColor.Purple)
                        && (cardSource.ContainsTraits("Avian") || cardSource.ContainsTraits("Bird"))
                        && cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 7
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.Trash);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.CanActivateOnDeletion(card, activateClass)
                        && card.Owner.Enemy.HandCards.Count <= 7
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                        canTargetCondition: CanSelectCardCondition,
                        root: SelectCardEffect.Root.Trash,
                        cardEffect: activateClass,
                        payCost: false));
                }
            }
            #endregion
            #endregion

            #region Option Effects
            #region Use Req
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, cardSource => cardSource.HasDataSquadTraits));
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent trashes hand cards until 7 remain, if none trashed, delete 1 of their Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] Your opponent trashes cards in their hand so that they have 7 left. If this effect didn't trash, delete 1 of their Digimon.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectDeleteCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    int handCountBefore = card.Owner.Enemy.HandCards.Count;

                    #region Trash down to 7
                    if (handCountBefore > 7)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner.Enemy,
                            canTargetCondition: _ => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: handCountBefore - 7,
                            canNoSelect: false,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    }
                    #endregion

                    #region If this effect didn't trash, delete 1 of their Digimon
                    if (card.Owner.Enemy.HandCards.Count < handCountBefore) yield break;

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDeleteCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                    #endregion
                }
            }
            #endregion

            #region Arts Digivolution
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }
            #endregion
            #endregion

            return cardEffects;
        }
    }
}
