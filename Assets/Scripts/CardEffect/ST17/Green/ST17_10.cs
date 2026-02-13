using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.ST17
{
    public class ST17_10 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Turn
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] If your opponent has a Digimon, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
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

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.Enemy.GetBattleAreaDigimons().Count >= 1)
                        {
                            if (card.Owner.CanAddMemory(activateClass))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve your [Terriermon] into [MegaGargomon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("Digivolution_ST17_10");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main][Once Per Turn] By placing this Tamer and 1 [Gargomon] and 1 [Rapidmon] from your trash in any order as one of your [Terriermon]'s bottom digivolution cards, that Digimon may digivolve into [MegaGargomon] in the hand for a digivolution cost of 4, ignoring its digivolution requirements. If this effect digivolved, that Digimon gains [Rush] (This Digimon can attack the turn it comes into play) for the turn.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.CardNames.Contains("Terriermon"))
                        {
                            if (!permanent.IsToken)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.CardNames.Contains("Gargomon");
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    return cardSource.CardNames.Contains("Rapidmon");
                }

                bool CanSelectCardCondition2(CardSource cardSource)
                {
                    return cardSource.CardNames.Contains("MegaGargomon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (!card.PermanentOfThisCard().IsToken)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
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
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        List<CardSource> selectedCards = new List<CardSource>() { card };

                        bool added = false;

                        Permanent selectedPermanent = null;

                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            int maxCount = 1;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectCardCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => false,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 [Gargomon] to place in Digivolution cards.",
                                        maxCount: maxCount,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Trash,
                                        customRootCardList: null,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 [Gargomon] to place in Digivolution cards.", "The opponent is selecting 1 [Gargomon] to place in Digivolution cards.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                            yield return StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }
                        }

                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition1))
                        {
                            int maxCount = 1;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectCardCondition1,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => false,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 [Rapidmon] to place in Digivolution cards.",
                                        maxCount: maxCount,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Trash,
                                        customRootCardList: null,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 [Rapidmon] to place in Digivolution cards.", "The opponent is selecting 1 [Rapidmon] to place in Digivolution cards.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                            yield return StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }
                        }

                        if (selectedCards.Count == 3)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                int maxCount = 1;

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

                                selectPermanentEffect.SetUpCustomMessage("Select 1 [Terriermon].", "The opponent is selecting 1 [Terriermon].");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    selectedPermanent = permanent;

                                    yield return null;
                                }
                            }

                            if (selectedPermanent != null)
                            {
                                if (selectedCards.Count == 3)
                                {
                                    List<CardSource> digivolutionCards = new List<CardSource>();

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
                                        digivolutionCards = cardSources.Clone();

                                        yield return null;
                                    }

                                    if (selectedPermanent.TopCard != null)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(digivolutionCards, activateClass));

                                        added = true;
                                    }
                                }
                            }
                        }

                        if (added)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: selectedPermanent,
                                cardCondition: CanSelectCardCondition2,
                                payCost: true,
                                reduceCostTuple: null,
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: 4,
                                isHand: true,
                                activateClass: activateClass,
                                successProcess: SuccessProcess(),
                                ignoreRequirements: CardEffectCommons.IgnoreRequirement.All));

                            IEnumerator SuccessProcess()
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRush(
                                    targetPermanent: selectedPermanent,
                                    effectDuration: EffectDuration.UntilEachTurnEnd,
                                    activateClass: activateClass));
                            }
                        }
                    } 
                }
            }
            #endregion

            #region Security Skill
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}