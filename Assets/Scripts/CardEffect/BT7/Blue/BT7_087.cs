using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT7_087 : CEntity_Effect
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
            activateClass.SetUpICardEffect("Place cards from hand to digivolution cards and digivolve", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, false, EffectDiscription());
            activateClass.SetHashString("Digivolve_BT7_085");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main][Once Per Turn] You may place 5 cards with [Hybrid] in their traits from your hand under this Tamer in any order to digivolve it into a [MagnaGarurumon] in your hand for its digivolution cost as if this Tamer is a level 5 blue Digimon.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.CardTraits.Contains("Hybrid");
            }

            bool CanSelectCardCondition1(CardSource cardSource)
            {
                return cardSource.CardNames.Contains("MagnaGarurumon");
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 5)
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
                    if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 5)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 5;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
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

                        selectHandEffect.SetUpCustomMessage("Select cards to place in Digivolution cards.", "The opponent is selecting cards to place in Digivolution cards.");
                        selectHandEffect.SetNotShowCard();

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        List<CardSource> digivolutionCards = new List<CardSource>();

                        if (selectedCards.Count == 5)
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

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

                            #region treat as blue level 5 Digimon

                            CardSource topCard = card.PermanentOfThisCard().TopCard;

                            #region treat as blue
                            ChangeCardColorClass changeCardColorClass = new ChangeCardColorClass();
                            changeCardColorClass.SetUpICardEffect($"Treated as blue", CanUseCondition1, card);
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
                                    CardColors.Add(CardColor.Blue);
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

                            card.PermanentOfThisCard().IsPlaceToTrashDueToNotHavingDP = false;

                            #endregion

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

        if (timing == EffectTiming.OnAddHand)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Memory +1 and getin unblockable", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
            activateClass.SetIsInheritedEffect(true);
            activateClass.SetHashString("Memory+1_BT7_087");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Your Turn][Once Per Turn] When an effect adds a card to your hand, gain 1 memory. Then, this Digimon can't be blocked for the turn.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (CardEffectCommons.CanTriggerOnHandAdded(hashtable, card.Owner, null))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));

                Permanent selectedPermanent = card.PermanentOfThisCard();

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotBeBlocked(
                    targetPermanent: selectedPermanent,
                    defenderCondition: null,
                    effectDuration: EffectDuration.UntilEachTurnEnd,
                    activateClass: activateClass,
                    effectName: "Unblockable"));
            }
        }

        return cardEffects;
    }
}
