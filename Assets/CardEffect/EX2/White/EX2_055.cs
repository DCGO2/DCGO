using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX2
{
    public class EX2_055 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash digivolution cards from [Mother D-Reaper] to make play cost 0", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("PlayCost0_EX2_055");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When you would play this Digimon, you may trash 7 or more digivolution cards from the bottom of 1 of your [Mother D-Reaper]s to set this Digimon's play cost to 0.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            if (permanent.DigivolutionCards.Count((cardSource) => !cardSource.CanNotTrashFromDigivolutionCards(activateClass)) >= 7)
                            {
                                if (permanent.TopCard.CardNames.Contains("Mother D-Reaper"))
                                {
                                    return true;
                                }

                                if (permanent.TopCard.CardNames.Contains("MotherD-Reaper"))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        bool CanNoSelect = true;

                        int maxCount = 1;

                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: CanNoSelect,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 [Mother D-Reaper].", "The opponent is selecting 1 [Mother D-Reaper].");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            maxCount = selectedPermanent.DigivolutionCards.Count((cardSource) => !cardSource.CanNotTrashFromDigivolutionCards(activateClass));

                            if (maxCount >= 7)
                            {
                                int trashCount = maxCount;

                                SelectCountEffect selectCountEffect = GManager.instance.GetComponent<SelectCountEffect>();

                                List<int> trashCountList = new List<int>();

                                for (int i = 0; i < maxCount + 1; i++)
                                {
                                    if (i >= 7)
                                    {
                                        int k = i;

                                        trashCountList.Add(k);
                                    }
                                }

                                if (selectCountEffect != null)
                                {
                                    selectCountEffect.SetUp(
                                        SelectPlayer: card.Owner,
                                        targetPermanent: null,
                                        MaxCount: 1,
                                        CanNoSelect: false,
                                        Message: "How many digivolution cards will you trash?",
                                        Message_Enemy: "The opponent is choosing how many digivolution cards to trash.",
                                        SelectCountCoroutine: SelectCountCoroutine);

                                    selectCountEffect.SetCandidates(trashCountList);
                                    selectCountEffect.SetPreferMin(true);

                                    yield return ContinuousController.instance.StartCoroutine(selectCountEffect.Activate());

                                    IEnumerator SelectCountCoroutine(int count)
                                    {
                                        trashCount = count;
                                        yield return null;
                                    }

                                    if (trashCount >= 7)
                                    {
                                        List<CardSource> trashCards = new List<CardSource>();
                                        List<CardSource> digivolutionCards_reverse = selectedPermanent.DigivolutionCards.Clone();

                                        digivolutionCards_reverse.Reverse();

                                        for (int i = 0; i < trashCount; i++)
                                        {
                                            foreach (CardSource cardSource in digivolutionCards_reverse)
                                            {
                                                if (!cardSource.CanNotTrashFromDigivolutionCards(activateClass))
                                                {
                                                    if (!trashCards.Contains(cardSource))
                                                    {
                                                        trashCards.Add(cardSource);
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        if (trashCards.Count >= 1)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(
                                                selectedPermanent,
                                                trashCards,
                                                activateClass).TrashDigivolutionCards());

                                            if (trashCards.Count >= 7)
                                            {
                                                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                                                ChangeCostClass changeCostClass = new ChangeCostClass();
                                                changeCostClass.SetUpICardEffect("Play Cost -12", CanUseCondition1, card);
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
                                                                Cost = 0;
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
                                                    return true;
                                                }

                                                bool isUpDown()
                                                {
                                                    return false;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost is 0", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => true, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        ActivateClass activateClass = new ActivateClass();
                        activateClass.SetUpICardEffect("", (hashtable) => true, card);

                        if (activateClass != null)
                        {
                            if (permanent.DigivolutionCards.Count((cardSource) => !cardSource.CanNotTrashFromDigivolutionCards(activateClass)) >= 7)
                            {
                                if (permanent.TopCard.CardNames.Contains("Mother D-Reaper"))
                                {
                                    return true;
                                }

                                if (permanent.TopCard.CardNames.Contains("MotherD-Reaper"))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (!card.Owner.isYou && GManager.instance.IsAI)
                    {
                        return false;
                    }

                    if (card.Owner.GetBattleAreaPermanents().Some(CanSelectPermanentCondition))
                    {
                        return true;
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
                                Cost = 0;
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
                    return true;
                }

                bool isUpDown()
                {
                    return false;
                }
            }

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RushSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 2 [ADR-02 Searcher] under this Digimon from trash to unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] You may place 2 [ADR-02 Searcher]s from your trash under this Digimon in any order as its bottom digivolution cards to unsuspend this Digimon.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (!cardSource.IsToken)
                    {
                        if (cardSource.CardNames.Contains("ADR-02 Searcher"))
                        {
                            return true;
                        }

                        if (cardSource.CardNames.Contains("ADR-02Searcher"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (!card.PermanentOfThisCard().IsToken)
                        {
                            if (card.Owner.TrashCards.Count(CanSelectCardCondition) >= 2)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            if (card.Owner.TrashCards.Count(CanSelectCardCondition) >= 2)
                            {
                                List<CardSource> selectedCards = new List<CardSource>();

                                int maxCount = 2;

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select cards to place in Digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Cards");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCards.Add(cardSource);
                                    yield return null;
                                }

                                if (selectedCards.Count >= 1)
                                {
                                    if (CardEffectCommons.IsExistOnBattleArea(card))
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().
                                        AddDigivolutionCardsBottom(
                                        selectedCards,
                                        activateClass));

                                        if (selectedCards.Count == 2)
                                        {
                                            Permanent selectedPermanent = card.PermanentOfThisCard();

                                            yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                                                new List<Permanent>() { selectedPermanent },
                                                activateClass).Unsuspend());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}