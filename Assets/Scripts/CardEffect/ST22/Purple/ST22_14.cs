using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Barbamon 
namespace DCGO.CardEffects.ST22
{
    public class ST22_14 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static effects

            #region Alternate Digivolution Cost

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel5 && targetPermanent.TopCard.HasLevel &&
                        (targetPermanent.TopCard.HasFallenAngelTraits || targetPermanent.TopCard.HasCSTraits);
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

            #region Play Cost Reduction

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reduce play cost (5)", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When this card would be played, if your opponent has 10 or more cards in either their hand or trash, reduce the play cost by 5.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, cardSource => cardSource == card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return card.Owner.Enemy.HandCards.Count >= 10 ||
                           card.Owner.Enemy.TrashCards.Count >= 10;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.CanReduceCost(null, card))
                    {
                        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                    }

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect("Play Cost -5", hashtable => true, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                    card.Owner.UntilCalculateFixedCostEffect.Add(_ => changeCostClass);

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                    int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root,
                        List<Permanent> targetPermanents)
                    {
                        if (CardSourceCondition(cardSource) &&
                            RootCondition(root) &&
                            PermanentsCondition(targetPermanents))
                        {
                            cost -= 5;
                        }

                        return cost;
                    }

                    bool PermanentsCondition(List<Permanent> targetPermanents)
                    {
                        return targetPermanents == null || targetPermanents.Count(targetPermanent => targetPermanent != null) == 0;
                    }

                    bool CardSourceCondition(CardSource cardSource)
                    {
                        return cardSource == card;
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

            #region Reduce Play Cost - Not Shown

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -5", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => true, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return card.Owner.Enemy.HandCards.Count >= 10 || card.Owner.Enemy.TrashCards.Count >= 10;
                }

                int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root,
                        List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource) &&
                        RootCondition(root) &&
                        PermanentsCondition(targetPermanents))
                    {
                        cost -= 5;
                    }

                    return cost;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    return targetPermanents == null || targetPermanents.Count(targetPermanent => targetPermanent != null) == 0;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource == card;
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

            #endregion

            #endregion

            #region Shared (On Play/When Digivolving)

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool SharedCanSelectCard(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel && cardSource.Level <= 5
                        && cardSource.HasFallenAngelTraits
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                bool hasDeleted = false;
                int amountToTrash = card.Owner.Enemy.HandCards.Count - 6;

                #region Trash hand cards if more then 6

                if (amountToTrash > 0)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                    int discardCount = Math.Min(amountToTrash, card.Owner.Enemy.HandCards.Count);

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner.Enemy,
                        canTargetCondition: (cardSource) => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: discardCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Any()) hasDeleted = true;
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                }

                #endregion

                #region If no cards trashed, summon digimon from trash

                if (!hasDeleted && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, SharedCanSelectCard))
                {
                    CardSource selectedCard = null;
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, SharedCanSelectCard));

                    selectCardEffect.SetUp(
                        canTargetCondition: SharedCanSelectCard,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 card to play.",
                        maxCount: maxCount,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: new List<CardSource>() { selectedCard },
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Trash,
                        activateETB: true));
                }

                #endregion
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent trashes hand cards until 6 left, If none was trashed, Play 1 [Fallen Angel] from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[On Play] Your opponent trashes cards in their hand so that they have 6 cards left. If this effect didn't trash, you may play 1 level 5 or lower [Fallen Angel] trait Digimon card from your trash without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent trashes hand cards until 6 left, If none was trashed, Play 1 [Fallen Angel] from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[When Digivolving] Your opponent trashes cards in their hand so that they have 6 cards left. If this effect didn't trash, you may play 1 level 5 or lower [Fallen Angel] trait Digimon card from your trash without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            #region End of Your Turn

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If opponent has 6 or more hand cards, they trash one. Then, if they have 5 or less, delete 1 of their lowest level digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("ST22_08_EOYT");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[End of Your Turn] [Once Per Turn] If your opponent has 6 or more cards in their hand, they trash 1 of them. Then, if they have 5 or fewer, delete 1 of their lowest level Digimon.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    #region If 6 or more hand cards, trash 1

                    if (card.Owner.Enemy.HandCards.Count >= 6)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                        int discardCount = Math.Min(1, card.Owner.Enemy.HandCards.Count);

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner.Enemy,
                            canTargetCondition: (cardSource) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: discardCount,
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

                    #region If 5 or less hand cards, delete 1 lowest level digimon

                    if (card.Owner.Enemy.HandCards.Count <= 5 && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    #endregion
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
