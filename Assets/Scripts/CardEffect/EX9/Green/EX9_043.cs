using System;
using System.Collections;
using System.Collections.Generic;

// MetalTyrannomon
namespace DCGO.CardEffects.EX9
{
    public class EX9_043 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Play Cost Reduction

            bool reducePlayCost = false;

            #region When Would be Played

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 Cyborg/Ver.5 to get Play Cost -2", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetHashString("PlayCost-2_EX9_043");
                activateClass.SetIsDigimonEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription() => "When this card would be played, by trashing 1 [Cyborg]/[Ver.5] trait card from your hand, reduce the play cost by 2.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition);
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card && CardEffectCommons.IsExistOnHand(cardSource);
                }

                bool TrashCardCondition(CardSource cardSource)
                {
                    if (cardSource.EqualsTraits("Cyborg") || cardSource.EqualsTraits("Ver.5"))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionOwnersHand(card, TrashCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canNoSelect = true;
                    CardSource cardFromHashtable = CardEffectCommons.GetCardFromHashtable(hashtable);

                    if (cardFromHashtable && cardFromHashtable.PayingCost(SelectCardEffect.Root.Hand, null, checkAvailability: false) >
                        cardFromHashtable.Owner.MaxMemoryCost)
                    {
                        canNoSelect = false;
                    }

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: TrashCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: canNoSelect,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    selectHandEffect.SetUpCustomMessage("Select 1 card to trash.", "The opponent is selecting 1 card to trash.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Trashed Card");
                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (CardEffectCommons.IsExistOnTrash(cardSources[0]))
                        {
                            if (card.Owner.CanReduceCost(null, card))
                            {
                                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                            }

                            int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                            {
                                if (CardSourceCondition(cardSource) && RootCondition(root)) cost -= 2;
                                return cost;
                            }

                            bool CardSourceCondition(CardSource cardSource)
                            {
                                return cardSource == card;
                            }

                            bool RootCondition(SelectCardEffect.Root root)
                            {
                                return true;
                            }

                            ChangeCostClass changeCostClass = new ChangeCostClass();
                            changeCostClass.SetUpICardEffect("Play Cost -2", _ => true, card);
                            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition,
                                rootCondition: RootCondition, isUpDown: () => true, isCheckAvailability: () => false,
                                isChangePayingCost: () => true);
                            card.Owner.UntilCalculateFixedCostEffect.Add(_ => changeCostClass);

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(hashtable));
                            reducePlayCost = true;
                        }
                    }
                }
            }

            #endregion

            #region Reduce Play Cost - Not Shown

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -2", CanUseCondition1, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition,
                    rootCondition: RootCondition, isUpDown: IsUpDown, isCheckAvailability: () => true,
                    isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition1(Hashtable hashtable1)
                {
                    return true;
                }

                int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource) && RootCondition(root) && reducePlayCost) cost -= 2;
                    return cost;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                bool IsUpDown()
                {
                    return true;
                }
            }

            #endregion

            #endregion

            #region Digivolution Cost Reduction

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.IsLevel4)
                    {
                        if (targetPermanent.TopCard.ContainsCardName("Tyrannomon") || targetPermanent.TopCard.EqualsTraits("DM"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition,
                    digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #endregion

            #region On Play/When Digivolving Shared

            bool SharedCardCondition(CardSource cardSource)
            {
                return cardSource.IsDigimon && CardEffectCommons.IsExistOnTrash(cardSource);
            }
            IEnumerator SharedActivateCoroutine(ActivateClass activateClass)
            {
                CardSource selectedCard = null;

                bool CanSelectPermanentDeleteCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                        permanent.DP <= card.Owner.MaxDP_DeleteEffect(3000, activateClass);
                }

                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, SharedCardCondition))
                {
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                    selectCardEffect.SetUp(
                        canTargetCondition: SharedCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 card to add as bottom digivolution card",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: false,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: card.Owner.TrashCards,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass
                    );

                    selectCardEffect.SetUpCustomMessage("Select 1 card to add as bottom digivolution card.", "The opponent is selecting 1 card to add as bottom digivolution card.");
                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }
                }

                if (selectedCard != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddExecutingCard(selectedCard));
                    yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass, isFacedown: true));

                    bool CanSelectPermanentCondition(Permanent permanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        Permanent selectedPermanent = null;
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to de-digivolve", "The opponent is selecting 1 Digimon to de-digivolve");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            var degenCount = card.PermanentOfThisCard().DigivolutionCards.Filter(x => x.IsFlipped).Count;
                            yield return ContinuousController.instance.StartCoroutine(new IDegeneration(selectedPermanent, degenCount, activateClass,false).Degeneration());
                        }
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentDeleteCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentDeleteCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete", "The opponent is selecting 1 Digimon to delete");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By adding 1 digimon flipped from trash to sources, De-Digi for each flipped card, then delete a 3K DP or lower", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, _ => SharedActivateCoroutine(activateClass), -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By placing 1 Digimon card from your trash face down as this Digimon's bottom digivolution card, to 1 of your opponent's Digimon, <De-Digivolve 1> (Trash the top card. You can't trash past level 3 cards) for each of this Digimon's face-down digivolution cards. Then, delete 1 of your opponent's 3000 DP or lower Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, SharedCardCondition);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By adding 1 digimon flipped from trash to sources, De-Digi for each flipped card, then delete a 3K DP or lower", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, _ => SharedActivateCoroutine(activateClass), -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By placing 1 Digimon card from your trash face down as this Digimon's bottom digivolution card, to 1 of your opponent's Digimon, <De-Digivolve 1> (Trash the top card. You can't trash past level 3 cards) for each of this Digimon's face-down digivolution cards. Then, delete 1 of your opponent's 3000 DP or lower Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, SharedCardCondition);
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}