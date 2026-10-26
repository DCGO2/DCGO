using System.Collections;
using System.Collections.Generic;

// Ravemon: Burst Mode / Mourning Dance
namespace DCGO.CardEffects.BT27
{
    public class BT27_085 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Effects
            #region Alternate Digivolution Requirement - Lv.6 w/[DATA SQUAD] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasDataSquadTraits;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 5, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 6));
            }
            #endregion

            #region Burst Digivolution
            if (timing == EffectTiming.None)
            {
                AddBurstDigivolutionConditionClass addBurstDigivolutionConditionClass = new AddBurstDigivolutionConditionClass();
                addBurstDigivolutionConditionClass.SetUpICardEffect("Burst Digivolution", _ => true, card);
                addBurstDigivolutionConditionClass.SetUpAddBurstDigivolutionConditionClass(getBurstDigivolutionCondition: GetBurstDigivolution);
                addBurstDigivolutionConditionClass.SetNotShowUI(true);
                cardEffects.Add(addBurstDigivolutionConditionClass);

                BurstDigivolutionCondition GetBurstDigivolution(CardSource cardSource)
                {
                    if (cardSource != card) return null;

                    bool TamerCondition(Permanent permanent)
                        => permanent != null && permanent.TopCard != null
                            && permanent.TopCard.Owner == card.Owner
                            && card.Owner.GetBattleAreaPermanents().Contains(permanent)
                            && !permanent.CannotReturnToHand(null)
                            && permanent.TopCard.EqualsCardName("Keenan Crier");

                    bool DigimonCondition(Permanent permanent)
                        => permanent != null && permanent.TopCard != null
                            && permanent.TopCard.Owner == card.Owner
                            && card.Owner.GetFieldPermanents().Contains(permanent)
                            && !card.CanNotEvolve(permanent)
                            && permanent.TopCard.EqualsCardName("Ravemon");

                    return new BurstDigivolutionCondition(
                        tamerCondition: TamerCondition, selectTamerMessage: "1 [Keenan Crier]",
                        digimonCondition: DigimonCondition, selectDigimonMessage: "1 [Ravemon]",
                        cost: 0);
                }
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared When Digivolving / When Attacking

            string SharedEffectName = "May activate 1 [Main] effect on this card's Option side";

            string SharedEffectDescription(string tag)
                => $"[{tag}] You may activate 1 [Main] effect on this card's Option side.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ActivateMainOfOptionSide(card, activateClass));
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: true,
                whenDigivolving: true,
                whenAttacking: true);
            #endregion

            #region All Turns - OPT
            if (timing == EffectTiming.OnDiscardHand)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon may unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("BT27_085_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When your opponent's hand is trashed from, this Digimon may unsuspend.";

                bool OpponentHandCardCondition(CardSource cardSource)
                    => cardSource.Owner == card.Owner.Enemy;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnTrashHand(hashtable, null, OpponentHandCardCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && card.PermanentOfThisCard().CanUnsuspend;

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                        permanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        cardEffect: activateClass).Unsuspend());
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
                activateClass.SetUpICardEffect("Opponent trashes 1 card from hand or security, then delete 1 of their Digimon with level at least their hand size", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] Your opponent trashes any 1 card from their hand or security stack. Then, delete 1 of their Digimon with a level as high as the number of cards in their hand or higher.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectDeleteCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasLevel
                        && permanent.Level >= card.Owner.Enemy.HandCards.Count;

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    #region Opponent trashes 1 card from hand or security
                    bool canTrashHand = card.Owner.Enemy.HandCards.Count >= 1;
                    bool canTrashSecurity = card.Owner.Enemy.SecurityCards.Count >= 1;

                    if (canTrashHand || canTrashSecurity)
                    {
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
                    #endregion

                    #region Delete 1 Digimon with level at least their hand size
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
