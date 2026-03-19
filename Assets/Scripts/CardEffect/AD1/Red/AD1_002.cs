using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Aldamon
namespace DCGO.CardEffects.AD1
{
    public class AD1_002 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Takuya Kanbara")
                        && targetPermanent.DigivolutionCards.Count(cardSource => cardSource.EqualsTraits("Hybrid")) >= 2;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }
            #endregion

            #region Rush
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RushSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 enemy digimon with equal or less DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] Delete 1 of your opponent's Digimon with as much or less DP as this Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card)
                        && CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool ValidOpponentDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.HasDP
                        && permanent.DP <= card.PermanentOfThisCard().DP;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(ValidOpponentDigimon))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: ValidOpponentDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            #region Shared EoA/OD
            string SharedEffectName()
            {
                return "May trash 1 [Hybrid]/[Ten Warriors] card, if so <Draw 2>, then may play 1 R/U/G tamer with inherits from hand or trash for free";
            }

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] You may trash 1 [Hybrid] or [Ten Warriors] trait card from your hand. If this effect trashed, <Draw 2>. Then, you may play 1 red, blue or green Tamer card with inherited effects from your hand or trash without paying the cost.";
            }

            bool CanSelectTrashCondition(CardSource cardSource)
            {
                return cardSource.EqualsTraits("Hybrid")
                        || cardSource.EqualsTraits("Ten Warriors");
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool discarded = false;

                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectTrashCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: AfterSelectCardCoroutine,
                    mode: SelectHandEffect.Mode.Discard,
                    cardEffect: activateClass);

                selectHandEffect.SetUpCustomMessage("Select 1 Card to trash.", "The opponent is selecting 1 card to trash from their hand.");

                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

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
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 2, activateClass).Draw());
                }

                bool CanSelectPlayCondition(CardSource cardSource)
                {
                    return cardSource.IsTamer
                        && (cardSource.CardColors.Contains(CardColor.Red)
                            || cardSource.CardColors.Contains(CardColor.Blue)
                            || cardSource.CardColors.Contains(CardColor.Green))
                        && cardSource.HasInheritedEffect
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectPlayCondition);
                bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectPlayCondition);

                if (canSelectHand || canSelectTrash)
                {
                    #region Setup Location Selection
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                    if (canSelectHand)
                    {
                        selectionElements.Add(new(message: $"From hand", value: 1, spriteIndex: 0));
                    }
                    if (canSelectTrash)
                    {
                        selectionElements.Add(new(message: $"From trash", value: 2, spriteIndex: 0));
                    }
                    selectionElements.Add(new(message: $"Do not play", value: 3, spriteIndex: 1));

                    string selectPlayerMessage = "From which area do you select a card?";
                    string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                    #endregion

                    bool doPlay = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                    bool fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;

                    if (doPlay)
                    {
                        #region Hand/Trash Card Selection & Play
                        if (fromHand)
                        {
                            SelectHandEffect selectHandEffect2 = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect2.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPlayCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.PlayForFree,
                                cardEffect: activateClass);

                            yield return StartCoroutine(selectHandEffect2.Activate());
                        }
                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectPlayCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to play.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.PlayForFree,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);
                                
                            yield return StartCoroutine(selectCardEffect.Activate());
                        }
                        #endregion
                    }
                }
            }
            #endregion

            #region End of Attack
            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, SharedEffectDescription("End of Attack"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }
            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, SharedEffectDescription("On Deletion"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card);
                }
            }
            #endregion

            #region Your Turn - ESS
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 4000, isInheritedEffect: true,
                    card: card, condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}
