using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// QueenBeemon
namespace DCGO.CardEffects.EX11
{
    public class EX11_034 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel5
                        && targetPermanent.TopCard.EqualsTraits("Royal Base");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false,
                    card: card, condition: null));
            }

            #endregion

            #region Shared OP/WD/WA

            string SharedEffectName()
            {
                return "May place [Royal Base] trait card from hand or trash face up at top or bot of sec, then delete opponent's Digimon";
            }

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] [Once Per Turn] You may place 1 [Royal Base] trait card from your hand or trash face up as the top or bottom security card. Then, delete up to 8 play cost's total worth of your opponent's Digimon. For each of your face-up security cards, add 2 to this effect's play cost maximum.";
            }

            string SharedHashString = "EX11_034_OP_WD_WA";

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.EqualsTraits("Royal Base");
            }

            int maxCost()
            {
                int maxCost = 8;

                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    maxCost += 2 * card.Owner.SecurityCards.Count(source => !source.IsFlipped);
                }

                return maxCost;
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && permanent.IsDigimon
                    && permanent.TopCard.HasPlayCost
                    && permanent.TopCard.GetCostItself <= maxCost();
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (card.Owner.CanAddSecurity(activateClass))
                {
                    #region Setup Location Selection

                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => CanSelectCardCondition(cardSource));
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, cardSource => CanSelectCardCondition(cardSource));
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                    if (canSelectHand)
                    {
                        selectionElements.Add(new(message: "From hand", value: 0, spriteIndex: 0));
                    }
                    if (canSelectTrash)
                    {
                        selectionElements.Add(new(message: "From trash", value: 1, spriteIndex: 0));
                    }
                    selectionElements.Add(new(message: "Do not place", value: 2, spriteIndex: 1));

                    string selectPlayerMessage = "From which area will you select?";
                    string notSelectPlayerMessage = "The opponent is choosing if they will activate their effect.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements,
                        selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                        notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                        .WaitForEndSelect());

                    int selection = GManager.instance.userSelectionManager.SelectedIntValue;

                    #endregion

                    #region Hand/Trash Card Selection

                    if (selection != 2)
                    {
                        CardSource selectedCard = null;

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;

                            yield return null;
                        }

                        if (selection == 0)
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: cardSource => CanSelectCardCondition(cardSource),
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

                            selectHandEffect.SetUpCustomMessage("Select 1 card to place.", "The opponent is selecting 1 card to place.");

                            yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                        }
                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: cardSource => CanSelectCardCondition(cardSource),
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to place.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 card to place.", "The opponent is selecting 1 card to place.");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }

                        #endregion

                        #region Top/Bottom Selection

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { selectedCard }, "Security Top card", true, true));

                        List<SelectionElement<bool>> selectionElements1 = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Security Top", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Security Bottom", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage1 = "Do you place the card on the top or bottom of the security?";
                        string notSelectPlayerMessage1 = "The opponent is choosing whether to place the card on the top or bottom of security.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage1, notSelectPlayerMessage: notSelectPlayerMessage1);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool toTop = GManager.instance.userSelectionManager.SelectedBoolValue;

                        #endregion

                        #region Send to Security

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(selectedCard, toTop: toTop, faceUp: true));

                        #endregion

                    }
                }

                #region Delete Opponent's Digimon

                int _maxCost = maxCost();

                List<Permanent> selectedPermanents = new List<Permanent>();

                int maxCount = card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition);

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                    canEndSelectCondition: CanEndSelectCondition,
                    maxCount: maxCount,
                    canNoSelect: false,
                    canEndNotMax: true,
                    selectPermanentCoroutine: null,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Destroy,
                    cardEffect: activateClass);

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                bool CanEndSelectCondition(List<Permanent> permanents)
                {
                    if (permanents.Count <= 0)
                    {
                        return false;
                    }

                    int sumCost = 0;

                    foreach (Permanent permanent1 in permanents)
                    {
                        sumCost += permanent1.TopCard.GetCostItself;
                    }

                    if (sumCost > _maxCost)
                    {
                        return false;
                    }

                    return true;
                }

                bool CanTargetCondition_ByPreSelecetedList(List<Permanent> permanents, Permanent permanent)
                {
                    int sumCost = 0;

                    foreach (Permanent permanent1 in permanents)
                    {
                        sumCost += permanent1.TopCard.GetCostItself;
                    }

                    sumCost += permanent.TopCard.GetCostItself;

                    if (sumCost > _maxCost)
                    {
                        return false;
                    }

                    return true;
                }

                #endregion

            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, (hash) => SharedActivateCoroutine(hash, activateClass), 1, false, SharedEffectDescription("On Play"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card)
                        && CardEffectCommons.IsExistOnBattleArea(card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, (hash) => SharedActivateCoroutine(hash, activateClass), 1, false, SharedEffectDescription("When Digivolving"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card)
                        && CardEffectCommons.IsExistOnBattleArea(card);
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), 1, false, SharedEffectDescription("When Attacking"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card) &&
                           CardEffectCommons.IsExistOnBattleArea(card);
                }
            }

            #endregion

            #region Shared1 WD/WA

            string SharedHashString1 = "EX11_034_WD_WA";

            int ReducedPlayCost() => Math.Max(0, card.Owner.SecurityCards.Count(source => !source.IsFlipped));

            string SharedEffectName1()
            {
                return "May play 1 card with [Royal Base] in it's text for 1 less per face-up card in your security."; 
                // saved until we can find a method to refresh between card activations - return $"May play 1 card with [Royal Base] in it's text for {ReducedPlayCost()} less.";
            }

            string SharedEffectDescription1(string tag)
            {
                return $"[{tag}] [Once Per Turn] You may play 1 card with [Royal Base] in its text from your hand. For each of your face-up security cards, reduce this effect's paid play cost by 1.";
            }

            bool SharedCanActivateCondition1(Hashtable hashtable, ActivateClass activateClass)
            {
                activateClass.SetEffectName(SharedEffectName1());
                return CardEffectCommons.IsExistOnBattleArea(card)
                    && CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => CanSelectCardCondition1(cardSource, activateClass));
            }

            bool CanSelectCardCondition1(CardSource cardSource, ActivateClass activateClass)
            {
                return cardSource.HasPlayCost
                    && cardSource.HasText("Royal Base")
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
            }

            IEnumerator SharedActivateCoroutine1(Hashtable hashtable, ActivateClass activateClass)
            {
                List<CardSource> selectedCards = new List<CardSource>();

                int maxCount = 1;

                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: cardSource => CanSelectCardCondition1(cardSource, activateClass),
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: false,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    selectCardCoroutine: SelectCardCoroutine,
                    afterSelectCardCoroutine: null,
                    mode: SelectHandEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                yield return StartCoroutine(selectHandEffect.Activate());

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCards.Add(cardSource);

                    yield return null;
                }

                #region Reduce play cost

                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect($"Play Cost -{ReducedPlayCost()}", CanUseCondition1, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                ICardEffect GetCardEffect(EffectTiming _timing)
                {
                    if (_timing == EffectTiming.None)
                    {
                        return changeCostClass;
                    }

                    return null;
                }

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
                                Cost -= ReducedPlayCost();
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
                    return cardSource != null
                        && cardSource.Owner == card.Owner
                        && cardSource.HasPlayCost
                        && cardSource.HasText("Royal Base");
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                bool isUpDown()
                {
                    return true;
                }

                #endregion

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                    cardSources: selectedCards,
                    activateClass: activateClass,
                    payCost: true,
                    isTapped: false,
                    root: SelectCardEffect.Root.Hand,
                    activateETB: true
                ));

                #region Release reducing play cost

                card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);

                #endregion
            }

            #endregion

            #region When Digivolving1

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName1(), CanUseCondition, card);
                activateClass.SetUpActivateClass(hash => SharedCanActivateCondition1(hash, activateClass), (hash) => SharedActivateCoroutine1(hash, activateClass), 1, true, SharedEffectDescription1("When Digivolving"));
                activateClass.SetHashString(SharedHashString1);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card)
                        && CardEffectCommons.IsExistOnBattleArea(card);
                }
            }

            #endregion

            #region When Attacking1

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName1(), CanUseCondition, card);
                activateClass.SetUpActivateClass(hash => SharedCanActivateCondition1(hash, activateClass), hash => SharedActivateCoroutine1(hash, activateClass), 1, true, SharedEffectDescription1("When Attacking"));
                activateClass.SetHashString(SharedHashString1);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card) &&
                           CardEffectCommons.IsExistOnBattleArea(card);
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
