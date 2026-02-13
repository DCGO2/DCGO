using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.P
{
    // Kuzuhamon
    public class P_223 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Maid Mode") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 6;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Reduced play cost

            #region Before Pay Cost - Condition Effect

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play cost reduction -4", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetHashString("P_223_ReducePlayCost");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When this card would be played from the hand, if you have 3 or fewer security, reduce the play cost by 4.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnHand(card)
                        && CardEffectCommons.HasMatchConditionPermanent(x => card.Owner.SecurityCards.Count <= 3);
                }

                bool CardCondition(CardSource cardSource)
                {
                    return CardEffectCommons.IsExistOnHand(cardSource)
                        && cardSource == card;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect("Play Cost -4", CanUseCondition1, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                    card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

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

                                    if (cardSource.Owner.SecurityCards.Count <= 3)
                                    {
                                        Cost -= 4;
                                    }
                                }
                            }
                        }

                        return Cost;
                    }

                    bool PermanentsCondition(List<Permanent> targetPermanents)
                    {
                        if (targetPermanents == null)
                        {
                            return true;
                        }
                        else
                        {
                            if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    bool CardSourceCondition(CardSource cardSource)
                    {
                        return cardSource == card;
                    }

                    bool RootCondition(SelectCardEffect.Root root)
                    {
                        return root == SelectCardEffect.Root.Hand;
                    }

                    bool isUpDown()
                    {
                        return true;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));
                }
            }

            #endregion

            #region Reduce Play Cost - Not Shown

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -4", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => true, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Contains(card))
                    {
                        ICardEffect activateClass = card.EffectList(EffectTiming.BeforePayCost).Find(cardEffect => cardEffect.EffectName == "Play cost reduction -4");

                        if (activateClass != null)
                        {
                            if (card.Owner.SecurityCards.Count <= 3)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource))
                    {
                        if (RootCondition(root))
                        {
                            if (PermanentsCondition(targetPermanents))
                            {
                                if (cardSource.Owner.SecurityCards.Count <= 3)
                                {
                                    Cost -= 4;
                                }
                            }
                        }
                    }

                    return Cost;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    if (targetPermanents == null)
                    {
                        return true;
                    }
                    else
                    {
                        if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource == card)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return root == SelectCardEffect.Root.Hand;
                }

                bool isUpDown()
                {
                    return true;
                }
            }

            #endregion

            #endregion

            #region Name Rule

            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Also treated as [Sakuyamon]", CanUseCondition, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: changeCardNames);
                cardEffects.Add(changeCardNamesClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                List<string> changeCardNames(CardSource cardSource, List<string> CardNames)
                {
                    if (cardSource == card)
                    {
                        CardNames.Add("Sakuyamon");
                    }

                    return CardNames;
                }
            }

            #endregion

            #region On Play / When Digivolving Shared

            string SharedEffectDiscription(string tag)
            {
                return $"[{tag}] You may use 1 [Onmyōjutsu] or [Plug-In] trait Option card from your hand or trash without paying the cost.";
            }

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            bool CanSelectOptionCard(CardSource cardSource)
            {
                if (cardSource.IsOption)
                {
                    if (cardSource.HasOnmyoOrPluginTraits)
                    {
                        if (!cardSource.CanNotPlayThisOption)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectOptionCard);
                bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectOptionCard);
                bool playFromHand = false;
                bool playFromTrash = false;
                if (canSelectHand || canSelectTrash)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    int maxCount = 1;

                    #region #region User Selection - Select Card Location

                    if (canSelectHand && canSelectTrash)
                    {
                        List<SelectionElement<bool>> selectionElements1 = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage1 = "From which area do you select a card?";
                        string notSelectPlayerMessage1 = "The opponent is choosing from which area to select a card.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage1, notSelectPlayerMessage: notSelectPlayerMessage1);
                    }
                    else
                    {
                        GManager.instance.userSelectionManager.SetBool(canSelectHand);
                    }

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    var handOrTrashSelection = GManager.instance.userSelectionManager.SelectedBoolValue;
                    if (handOrTrashSelection) playFromHand = true;
                    else playFromTrash = true;

                    #endregion

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);
                        yield return null;
                    }

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    if (playFromHand)
                    {
                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOptionCard,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 option to use", "The opponent is selecting 1 option to use");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                        yield return ContinuousController.instance.StartCoroutine(
                                        CardEffectCommons.PlayOptionCards(
                                        cardSources: selectedCards,
                                        activateClass: activateClass,
                                        payCost: false,
                                        root: SelectCardEffect.Root.Hand));
                    }
                    if (playFromTrash)
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectOptionCard,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 Option to use",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 option to use", "The opponent is selecting 1 option to use");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Used Card");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        yield return ContinuousController.instance.StartCoroutine(
                                        CardEffectCommons.PlayOptionCards(
                                        cardSources: selectedCards,
                                        activateClass: activateClass,
                                        payCost: false,
                                        root: SelectCardEffect.Root.Trash));
                    }
                }               
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("You may use an [Onmyōjutsu] or [Plug-In] Option from hand or trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, true, SharedEffectDiscription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("You may use an [Onmyōjutsu] or [Plug-In] Option from hand or trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, true, SharedEffectDiscription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card) && CardEffectCommons.IsExistOnBattleArea(card);
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnUseOption)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play a [Pipe Fox] Token", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("PlayToken_P_223");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns][Once Per Turn] When you use Option cards, you may play 1 [Pipe Fox] Token (Digimon/Yellow/6000 DP/<Blocker>).";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenOwnerUseOption(hashtable, null, null, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPipeFox(activateClass));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
