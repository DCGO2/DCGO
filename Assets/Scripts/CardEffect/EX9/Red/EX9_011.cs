using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// MetalGreymon
namespace DCGO.CardEffects.EX9
{
    public class EX9_011 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.IsLevel4)
                    {
                        if (targetPermanent.TopCard.ContainsCardName("Greymon"))
                        {
                            return true;
                        }

                        if (targetPermanent.TopCard.EqualsTraits("DM"))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Play Cost Reduction

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 [Cyborg]/[Ver.1] card from hand, to get Play Cost -2", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("PlayCost-4_BT13_083");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When this card would be played, by trashing 1 [Cyborg]/[Ver.1] card from your hand, reduce the play cost by 2.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, cardSource => cardSource == card))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.IsExistOnHand(cardSource))
                    {
                        if (cardSource != card)
                        {
                            if (cardSource.EqualsTraits("Cyborg") || cardSource.EqualsTraits("Ver.1"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool CanNoSelect =
                    CardEffectCommons.GetPlayCardClassFromHashtable(_hashtable) != null &&
                        card.PayingCost(
                            CardEffectCommons.GetPlayCardClassFromHashtable(_hashtable).Root,
                            null,
                            checkAvailability: false)
                        <= card.Owner.MaxMemoryCost;

                    bool discarded = false;
                    int discardCount = 1;
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
                    selectHandEffect.SetUpCustomMessage("Choose a [Cyborg] or [Ver.1] card to discard", "The opponent is selecting 1 card in hand to discard.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Discarded Card");
                    yield return StartCoroutine(selectHandEffect.Activate());

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
                        if (card.Owner.CanReduceCost(null, card))
                        {
                            ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                        }

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect("Play Cost -2", hashtable => true, card);
                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                        card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                        int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                        {
                            if (CardSourceCondition(cardSource))
                            {
                                if (RootCondition(root))
                                {
                                    if (PermanentsCondition(targetPermanents))
                                    {
                                        Cost -= 2;
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
                            return true;
                        }

                        bool isUpDown()
                        {
                            return true;
                        }
                    }
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 digimon in trash as source, to delete digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[On Play] By placing 1 Digimon card from your trash face down as this Digimon's bottom digivolution card, delete up to 5000 DP total worth of your opponent's Digimon. For each of this Digimon's face-down digivolution cards, add 2000 to this effect's DP maximum.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPlay(hashtable, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectTrashCardCondition))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanSelectTrashCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon;
                }

                int DeletionMaxDP()
                {
                    return 5000 + (2000 * card.PermanentOfThisCard().DigivolutionCards.Filter(x => x.IsFlipped).Count);
                }

                bool CanSelectDeletePermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(DeletionMaxDP(), activateClass))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedCard = null;
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CanSelectTrashCardCondition));
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                                canTargetCondition: CanSelectTrashCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to place face down under digimon",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 card to place face down under digimon", "The opponent is selecting 1 card to place face down under digimon.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");
                    yield return StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (selectedCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddExecutingCard(selectedCard));
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass, isFacedown: true));

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDeletePermanentCondition))
                        {
                            int maxCount1 = Math.Min(card.Owner.Enemy.GetBattleAreaDigimons().Count(CanSelectDeletePermanentCondition),
                                CardEffectCommons.MatchConditionPermanentCount(CanSelectDeletePermanentCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectDeletePermanentCondition,
                                canTargetCondition_ByPreSelecetedList: CanTargetConditionByPreSelectedList,
                                canEndSelectCondition: CanEndSelectCondition,
                                maxCount: maxCount1,
                                canNoSelect: false,
                                canEndNotMax: true,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Choose digimon to delete, Opponent is choosing digimon to delete");
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            bool CanEndSelectCondition(List<Permanent> permanents)
                            {
                                if (permanents.Count <= 0)
                                {
                                    return false;
                                }

                                int sumDP = 0;

                                foreach (Permanent permanent in permanents)
                                {
                                    sumDP += permanent.DP;
                                }

                                if (sumDP > card.Owner.MaxDP_DeleteEffect(DeletionMaxDP(), activateClass))
                                {
                                    return false;
                                }

                                return true;
                            }

                            bool CanTargetConditionByPreSelectedList(List<Permanent> permanents, Permanent permanent)
                            {
                                int sumDP = 0;

                                foreach (Permanent permanent1 in permanents)
                                {
                                    sumDP += permanent1.DP;
                                }

                                sumDP += permanent.DP;

                                if (sumDP > card.Owner.MaxDP_DeleteEffect(DeletionMaxDP(), activateClass))
                                {
                                    return false;
                                }

                                return true;
                            }
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 digimon in trash as source, to delete digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[When Digivolving] By placing 1 Digimon card from your trash face down as this Digimon's bottom digivolution card, delete up to 5000 DP total worth of your opponent's Digimon. For each of this Digimon's face-down digivolution cards, add 2000 to this effect's DP maximum.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectTrashCardCondition))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanSelectTrashCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon;
                }

                int DeletionMaxDP()
                {
                    return 5000 + (2000 * card.PermanentOfThisCard().DigivolutionCards.Filter(x => x.IsFlipped).Count);
                }

                bool CanSelectDeletePermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(DeletionMaxDP(), activateClass))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedCard = null;
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CanSelectTrashCardCondition));
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                                canTargetCondition: CanSelectTrashCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to place face down under digimon",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 card to place face down under digimon", "The opponent is selecting 1 card to place face down under digimon.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");
                    yield return StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (selectedCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddExecutingCard(selectedCard));
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass, isFacedown: true));

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDeletePermanentCondition))
                        {
                            int maxCount1 = Math.Min(card.Owner.Enemy.GetBattleAreaDigimons().Count(CanSelectDeletePermanentCondition),
                                CardEffectCommons.MatchConditionPermanentCount(CanSelectDeletePermanentCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectDeletePermanentCondition,
                                canTargetCondition_ByPreSelecetedList: CanTargetConditionByPreSelectedList,
                                canEndSelectCondition: CanEndSelectCondition,
                                maxCount: maxCount1,
                                canNoSelect: false,
                                canEndNotMax: true,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Choose digimon to delete, Opponent is choosing digimon to delete");
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            bool CanEndSelectCondition(List<Permanent> permanents)
                            {
                                if (permanents.Count <= 0)
                                {
                                    return false;
                                }

                                int sumDP = 0;

                                foreach (Permanent permanent in permanents)
                                {
                                    sumDP += permanent.DP;
                                }

                                if (sumDP > card.Owner.MaxDP_DeleteEffect(DeletionMaxDP(), activateClass))
                                {
                                    return false;
                                }

                                return true;
                            }

                            bool CanTargetConditionByPreSelectedList(List<Permanent> permanents, Permanent permanent)
                            {
                                int sumDP = 0;

                                foreach (Permanent permanent1 in permanents)
                                {
                                    sumDP += permanent1.DP;
                                }

                                sumDP += permanent.DP;

                                if (sumDP > card.Owner.MaxDP_DeleteEffect(DeletionMaxDP(), activateClass))
                                {
                                    return false;
                                }

                                return true;
                            }
                        }
                    }
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: true, card: card, condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}
