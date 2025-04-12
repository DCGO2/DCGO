using System.Collections;
using System.Collections.Generic;

//Aldamon
namespace DCGO.CardEffects.BT21
{
    public class BT21_020 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Reduced Digivolution Cost

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reduce the digivolution cost by 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "When any of your Digimon with [Agunimon]/[BurningGreymon] in their digivolution cards would digivolve into this card in the hand, reduce the digivolution cost by 1.";

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.cardSources.Find(source =>
                                                        (source.EqualsCardName("Agunimon") || source.EqualsCardName("BurningGreymon")) &&
                                                        source.CanPlayCardTargetFrame(permanent.PermanentFrame, true, activateClass)))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CardCondition(CardSource source)
                {
                    if (source == card) return true;
                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnHand(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, PermanentCondition))
                        {
                            if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(hashtable, PermanentCondition, CardCondition))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnHand(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Hashtable hashtable = new Hashtable();
                    hashtable.Add("CardEffect", activateClass);

                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect("Digivolution Cost -1", CanUseCondition1, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                    card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return true;
                    }

                    int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                    {
                        if (CardSourceCondition(cardSource))
                        {
                            if (RootCondition(root))
                            {
                                if (PermanentsCondition(targetPermanents))
                                {
                                    Cost -= 1;
                                }
                            }
                        }

                        return Cost;
                    }

                    bool PermanentsCondition(List<Permanent> targetPermanents)
                    {
                        if (targetPermanents != null)
                        {
                            if (targetPermanents.Exists(PermanentCondition))
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    bool PermanentCondition(Permanent targetPermanent)
                    {
                        if (targetPermanent.TopCard != null)
                        {
                            return PermanentCondition(targetPermanent);
                        }

                        return false;
                    }

                    bool CardSourceCondition(CardSource cardSource)
                    {
                        if (cardSource != null)
                        {
                            return CardCondition(cardSource);
                        }

                        return false;
                    }

                    bool RootCondition(SelectCardEffect.Root root)
                    {
                        return true;
                    }

                    bool isUpDown()
                    {
                        return true;
                    }
                }
            }

            #endregion

            #region Sec +1

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(
                    changeValue: 1,
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #endregion

            #region On Deletion / ESS Shared

            string OnDeletionEffectDiscription()
            {
                return "[On Deletion] You may play 1 red Tamer card with inherited effects from your hand or trash without paying the cost.";
            }

            bool CanUseOnDeletionCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
            }

            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play a Red tamer from hand or trash", CanUseOnDeletionCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, OnDeletionActivateCoroutine, -1, true, OnDeletionEffectDiscription());
                cardEffects.Add(activateClass);

                bool CanSelectCard(CardSource source)
                {
                    if (source.IsTamer)
                    {
                        if (source.CardColors.Contains(CardColor.Red))
                        {
                            if (source.HasInheritedEffect)
                            {
                                if (CardEffectCommons.CanPlayAsNewPermanent(source, false, activateClass, SelectCardEffect.Root.Hand) || CardEffectCommons.CanPlayAsNewPermanent(source, false, activateClass, SelectCardEffect.Root.Trash))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanActivateOnDeletion(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCard) || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCard))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator OnDeletionActivateCoroutine(Hashtable hashtable)
                {
                    SelectCardEffect.Root? rootmode = null!;
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCard);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCard);

                    if (canSelectHand && !canSelectTrash) rootmode = SelectCardEffect.Root.Hand;

                    if (!canSelectHand && canSelectTrash) rootmode = SelectCardEffect.Root.Trash;

                    if (canSelectHand && canSelectTrash)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                            {
                                new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                                new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                            };

                        string selectPlayerMessage = "From which area do you select a card?";
                        string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                        rootmode = GManager.instance.userSelectionManager.SelectedBoolValue ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                    }

                    if (rootmode != null)
                    {
                        CardSource selectedCard = null;
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCard,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 tamer.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: (SelectCardEffect.Root)rootmode!,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 tamer.", "The opponent is selecting 1 tamer.");
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        if (selectedCard)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: new List<CardSource>() { selectedCard }, activateClass: activateClass, payCost: false, isTapped: false, root: (SelectCardEffect.Root)rootmode, activateETB: true));
                        }
                    }
                }
            }
            #endregion

            #region On Deletion - ESS
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play a Red tamer from hand or trash", CanUseOnDeletionCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, OnDeletionActivateCoroutine, -1, true, OnDeletionEffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                bool CanSelectCard(CardSource source)
                {
                    if (source.IsTamer)
                    {
                        if (source.CardColors.Contains(CardColor.Red))
                        {
                            if (source.HasInheritedEffect)
                            {
                                if (CardEffectCommons.CanPlayAsNewPermanent(source, false, activateClass, SelectCardEffect.Root.Hand) || CardEffectCommons.CanPlayAsNewPermanent(source, false, activateClass, SelectCardEffect.Root.Trash))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanActivateOnDeletionInherited(hashtable, card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCard) || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCard))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator OnDeletionActivateCoroutine(Hashtable hashtable)
                {
                    SelectCardEffect.Root? rootmode = null!;
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCard);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCard);

                    if (canSelectHand && !canSelectTrash) rootmode = SelectCardEffect.Root.Hand;

                    if (!canSelectHand && canSelectTrash) rootmode = SelectCardEffect.Root.Trash;

                    if (canSelectHand && canSelectTrash)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                            {
                                new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                                new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                            };

                        string selectPlayerMessage = "From which area do you select a card?";
                        string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                        rootmode = GManager.instance.userSelectionManager.SelectedBoolValue ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                    }

                    if (rootmode != null)
                    {
                        CardSource selectedCard = null;
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCard,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 tamer.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: (SelectCardEffect.Root)rootmode!,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 tamer.", "The opponent is selecting 1 tamer.");
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        if (selectedCard)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: new List<CardSource>() { selectedCard }, activateClass: activateClass, payCost: false, isTapped: false, root: (SelectCardEffect.Root)rootmode, activateETB: true));
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
