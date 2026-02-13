using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT7_085 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.SecuritySkill)
        {
            cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
        }

        if (timing == EffectTiming.OnDeclaration)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Place cards from trash to digivolution cards and digivolve", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, false, EffectDiscription());
            activateClass.SetHashString("Digivolve_BT7_085");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main][Once Per Turn] You may place 5 cards with [Hybrid] in their traits from your trash under this Tamer in any order to digivolve it into an [EmperorGreymon] in your hand for its digivolution cost as if this Tamer is a level 5 red Digimon.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.CardTraits.Contains("Hybrid");
            }

            bool CanSelectCardCondition1(CardSource cardSource)
            {
                return cardSource.CardNames.Contains("EmperorGreymon");
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.TrashCards.Count(CanSelectCardCondition) >= 5)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.TrashCards.Count(CanSelectCardCondition) >= 5)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 5;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
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

                        selectCardEffect.SetUpCustomMessage("Select cards to place in Digivolution cards.", "The opponent is selecting cards to place in Digivolution cards.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Cards");

                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        List<CardSource> digivolutionCards = new List<CardSource>();

                        if (selectedCards.Count == 5)
                        {
                            selectCardEffect.SetUp(
                                canTargetCondition: (cardSource) => true,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine1,
                                message: "Specify the order to place the cards in the digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                                maxCount: selectedCards.Count,
                                canEndNotMax: false,
                                isShowOpponent: false,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: selectedCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Cards");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                            {
                                foreach (CardSource cardSource in cardSources)
                                {
                                    digivolutionCards.Add(cardSource);
                                }

                                yield return null;
                            }
                        }

                        if (digivolutionCards.Count == 5)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(digivolutionCards, activateClass));

                            #region treat as red level 5 Digimon

                            CardSource topCard = card.PermanentOfThisCard().TopCard;

                            #region treat as red
                            ChangeCardColorClass changeCardColorClass = new ChangeCardColorClass();
                            changeCardColorClass.SetUpICardEffect($"Treated as red", CanUseCondition1, card);
                            changeCardColorClass.SetUpChangeCardColorClass(ChangeCardColors: ChangeCardColors);
                            changeCardColorClass.SetNotShowUI(true);

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                if (CardEffectCommons.IsExistOnBattleArea(card))
                                {
                                    if (card == topCard)
                                    {
                                        if (topCard == card.PermanentOfThisCard().TopCard)
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
                            }

                            List<CardColor> ChangeCardColors(CardSource cardSource, List<CardColor> CardColors)
                            {
                                if (cardSource == card)
                                {
                                    CardColors.Add(CardColor.Red);
                                }

                                return CardColors;
                            }
                            #endregion

                            #region treat as level 5
                            ChangePermanentLevelClass changePermanentLevelClass = new ChangePermanentLevelClass();
                            changePermanentLevelClass.SetUpICardEffect($"Treated as level 5", CanUseCondition1, card);
                            changePermanentLevelClass.SetUpChangePermanentLevelClass(GetLevel: GetLevel);
                            changePermanentLevelClass.SetNotShowUI(true);

                            int GetLevel(Permanent permanent, int level)
                            {
                                if (CardEffectCommons.IsExistOnBattleArea(card))
                                {
                                    if (permanent == card.PermanentOfThisCard())
                                    {
                                        level = 5;
                                    }
                                }

                                return level;
                            }
                            #endregion

                            #region treat as Digimon
                            TreatAsDigimonClass treatAsDigimonClass = new TreatAsDigimonClass();
                            treatAsDigimonClass.SetUpICardEffect($"Treated as Digimon", CanUseCondition1, card);
                            treatAsDigimonClass.SetUpTreatAsDigimonClass(permanentCondition: PermanentCondition);
                            treatAsDigimonClass.SetNotShowUI(true);

                            bool PermanentCondition(Permanent permanent)
                            {
                                if (CardEffectCommons.IsExistOnBattleArea(card))
                                {
                                    if (permanent == card.PermanentOfThisCard())
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }
                            #endregion

                            #region treat as not having DP(not to show on UI)
                            DontHaveDPClass dontHaveDPClass = new DontHaveDPClass();
                            dontHaveDPClass.SetUpICardEffect("Doesn't have DP", CanUseCondition1, card);
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
                                card.PermanentOfThisCard().PermanentEffects.Add(GetCardEffect);
                            }
                            #endregion

                            card.PermanentOfThisCard().IsPlaceToTrashDueToNotHavingDP = false;

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                                targetPermanent: card.PermanentOfThisCard(),
                                                cardCondition: CanSelectCardCondition1,
                                                payCost: true,
                                                reduceCostTuple: null,
                                                fixedCostTuple: null,
                                                ignoreDigivolutionRequirementFixedCost: -1,
                                                isHand: true,
                                                activateClass: activateClass,
                                                successProcess: null));

                            #region release effects

                            if (CardEffectCommons.IsExistOnBattleArea(card))
                            {
                                foreach (Func<EffectTiming, ICardEffect> GetCardEffect in GetCardEffects)
                                {
                                    card.PermanentOfThisCard().PermanentEffects.Remove(GetCardEffect);
                                }

                                card.PermanentOfThisCard().IsPlaceToTrashDueToNotHavingDP = true;
                            }

                            #endregion
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.None)
        {
            bool Condition()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        return true;
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 2000, isInheritedEffect: true, card: card, condition: Condition));
        }

        if (timing == EffectTiming.None)
        {
            bool Condition()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (card.PermanentOfThisCard().DP >= 10000)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: true, card: card, condition: Condition));
        }

        return cardEffects;
    }
}
