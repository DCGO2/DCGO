using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT12
{
    public class BT12_015 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Your [Takuya Kanbara] digivolves to this card", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Hand][Main] Place 1 [Agunimon] and 1 [BurningGreymon] from your trash under 1 of your [Takuya Kanbara] cards in any order and digivolve into this card as if the Tamer is a level 4 red Digimon for the digivolution cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.CardNames.Contains("Agunimon"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.CardNames.Contains("BurningGreymon"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (permanent != null)
                    {
                        if (permanent.TopCard != null)
                        {
                            if (permanent.TopCard.Owner == card.Owner)
                            {
                                if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                {
                                    if (!permanent.IsToken)
                                    {
                                        if (permanent.TopCard.CardNames.Contains("Takuya Kanbara"))
                                        {
                                            return true;
                                        }

                                        if (permanent.TopCard.CardNames.Contains("TakuyaKanbara"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Contains(card))
                    {
                        if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                            {
                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition1))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.HandCards.Contains(card))
                    {
                        if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                        {
                            Permanent selectedPermanent = null;

                            int maxCount = Math.Min(1, card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 [Takuya Kanbara].", "The opponent is selecting 1 [Takuya Kanbara].");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;

                                yield return null;
                            }

                            if (selectedPermanent != null)
                            {
                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource)))
                                {
                                    bool digivolve = false;

                                    maxCount = 2;

                                    if (card.Owner.TrashCards.Count((cardSource) => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource)) < maxCount)
                                    {
                                        maxCount = card.Owner.TrashCards.Count((cardSource) => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource));
                                    }

                                    List<CardSource> selectedCards = new List<CardSource>();

                                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                    selectCardEffect.SetUp(
                                        canTargetCondition: (cardSource) => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource),
                                        canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                        canEndSelectCondition: CanEndSelectCondition,
                                        canNoSelect: () => false,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select cards to place on the bottom of Digivolution cards\n(cards will be placed in the digivolution cards so that cards with lower numbers are on top).",
                                        maxCount: maxCount,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Trash,
                                        customRootCardList: null,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                    bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                                    {
                                        if (cardSources.Count(CanSelectCardCondition) >= 1)
                                        {
                                            if (CanSelectCardCondition(cardSource))
                                            {
                                                return false;
                                            }
                                        }

                                        if (cardSources.Count(CanSelectCardCondition1) >= 1)
                                        {
                                            if (CanSelectCardCondition1(cardSource))
                                            {
                                                return false;
                                            }
                                        }

                                        return true;
                                    }

                                    bool CanEndSelectCondition(List<CardSource> cardSources)
                                    {
                                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                                        {
                                            if (cardSources.Count(CanSelectCardCondition) == 0)
                                            {
                                                return false;
                                            }
                                        }

                                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition1))
                                        {
                                            if (cardSources.Count(CanSelectCardCondition1) == 0)
                                            {
                                                return false;
                                            }
                                        }

                                        if (cardSources.Count(CanSelectCardCondition) >= 2)
                                        {
                                            return false;
                                        }

                                        if (cardSources.Count(CanSelectCardCondition1) >= 2)
                                        {
                                            return false;
                                        }

                                        return true;
                                    }

                                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                                    {
                                        selectedCards.Add(cardSource);
                                        yield return null;
                                    }

                                    if (selectedCards.Count >= 1)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(selectedCards, activateClass));

                                        if (selectedCards.Count == 2)
                                        {
                                            digivolve = true;
                                        }
                                    }

                                    if (digivolve)
                                    {
                                        #region �Ԃ̃��x��4�f�W�����Ƃ��Ĉ���

                                        CardSource topCard = selectedPermanent.TopCard;

                                        #region �ԂƂ��Ĉ���

                                        ChangeCardColorClass changeCardColorClass = new ChangeCardColorClass();
                                        changeCardColorClass.SetUpICardEffect($"Treated as red", CanUseCondition1, card);
                                        changeCardColorClass.SetUpChangeCardColorClass(ChangeCardColors: ChangeCardColors);
                                        changeCardColorClass.SetNotShowUI(true);

                                        bool CanUseCondition1(Hashtable hashtable)
                                        {
                                            if (selectedPermanent.TopCard != null)
                                            {
                                                if (card.Owner.GetBattleAreaPermanents().Contains(selectedPermanent))
                                                {
                                                    if (topCard == selectedPermanent.TopCard)
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }

                                            return false;
                                        }

                                        List<CardColor> ChangeCardColors(CardSource cardSource, List<CardColor> CardColors)
                                        {
                                            if (cardSource == selectedPermanent.TopCard)
                                            {
                                                CardColors.Add(CardColor.Red);
                                            }

                                            return CardColors;
                                        }

                                        #endregion

                                        #region ���x��4�Ƃ��Ĉ���

                                        ChangePermanentLevelClass changePermanentLevelClass = new ChangePermanentLevelClass();
                                        changePermanentLevelClass.SetUpICardEffect($"Treated as level 4", CanUseCondition1, card);
                                        changePermanentLevelClass.SetUpChangePermanentLevelClass(GetLevel: GetLevel);
                                        changePermanentLevelClass.SetNotShowUI(true);

                                        int GetLevel(Permanent permanent, int level)
                                        {
                                            if (selectedPermanent.TopCard != null)
                                            {
                                                if (permanent == selectedPermanent)
                                                {
                                                    level = 4;
                                                }
                                            }

                                            return level;
                                        }

                                        #endregion

                                        #region �f�W�����Ƃ��Ĉ���

                                        TreatAsDigimonClass treatAsDigimonClass = new TreatAsDigimonClass();
                                        treatAsDigimonClass.SetUpICardEffect($"Treated as Digimon", CanUseCondition1, card);
                                        treatAsDigimonClass.SetUpTreatAsDigimonClass(permanentCondition: PermanentCondition);
                                        treatAsDigimonClass.SetNotShowUI(true);

                                        bool PermanentCondition(Permanent permanent)
                                        {
                                            if (selectedPermanent.TopCard != null)
                                            {
                                                if (permanent == selectedPermanent)
                                                {
                                                    return true;
                                                }
                                            }

                                            return false;
                                        }

                                        #endregion

                                        #region DP�������Ȃ�(UI�ɕ\������Ȃ��悤��

                                        DontHaveDPClass dontHaveDPClass = new DontHaveDPClass();
                                        dontHaveDPClass.SetUpICardEffect("Don't have DP", CanUseCondition1, card);
                                        dontHaveDPClass.SetUpDontHaveDPClass(PermanentCondition: PermanentCondition);
                                        dontHaveDPClass.SetNotShowUI(true);

                                        #endregion

                                        List<Func<EffectTiming, ICardEffect>> GetCardEffects = new List<Func<EffectTiming, ICardEffect>>()
                            {
                                (_timing) => changeCardColorClass,
                                (_timing) => changePermanentLevelClass,
                                (_timing) => treatAsDigimonClass,
                                (_timing) => dontHaveDPClass,
                            };

                                        foreach (Func<EffectTiming, ICardEffect> GetCardEffect in GetCardEffects)
                                        {
                                            card.Owner.PermanentEffects.Add(GetCardEffect);
                                        }

                                        #endregion

                                        if (card.CanPlayCardTargetFrame(selectedPermanent.PermanentFrame, true, activateClass))
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(new PlayCardClass(
                                                cardSources: new List<CardSource>() { card },
                                                hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                                payCost: true,
                                                targetPermanent: selectedPermanent,
                                                isTapped: false,
                                                root: SelectCardEffect.Root.Hand,
                                                activateETB: true).PlayCard());
                                        }

                                        #region �Ԃ̃��x��4�f�W�����Ƃ��Ĉ������ʂ�����

                                        foreach (Func<EffectTiming, ICardEffect> GetCardEffect in GetCardEffects)
                                        {
                                            card.Owner.PermanentEffects.Remove(GetCardEffect);
                                        }

                                        #endregion
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 [Takuya Kanbara] from trash to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] Return 1[Takuya Kanbara] card from your trash to your hand.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.CardNames.Contains("Takuya Kanbara"))
                            {
                                return true;
                            }

                            if (cardSource.CardNames.Contains("TakuyaKanbara"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnTrash(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        int maxCount = Math.Min(1, card.Owner.TrashCards.Count(CanSelectCardCondition));

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 [Takuya Kanbara] to add to your hand.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.AddHand,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }
                }
            }

            return cardEffects;
        }
    }
}