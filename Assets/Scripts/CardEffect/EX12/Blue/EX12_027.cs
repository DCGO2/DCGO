using System.Collections;
using System.Collections.Generic;
using System.Linq;

// TeslaJellymon
namespace DCGO.CardEffects.EX12
{
    public class EX12_027 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Cost
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Jellymon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("DS");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 3));
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May play/use 1 card with [Jellymon] in text/[DS] trait from hand for 2 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("EX12_013_Main");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] [Once Per Turn] You may play or use 1 card with [Jellymon] in its text or the [DS] trait from your hand with the cost reduced by 2.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return (cardSource.HasText("Jellymon")
                            || cardSource.EqualsTraits("DS"))
                        && ((cardSource.IsOption
                                && !cardSource.CanNotPlayThisOption)
                            || (cardSource.HasPlayCost
                                && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: true, cardEffect: activateClass, fixedCost: cardSource.GetCostItself - 2)));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool isUsed = false;
                    CardSource selectedCard = null;

                    #region Selected card in hand to play/use
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectCardCondition,
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

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    selectHandEffect.SetUpCustomMessage("Select 1 card to play/use.", "The opponent is selecting 1 card to play/use.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");
                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    #endregion

                    if (selectedCard != null)
                    {
                        #region Reduce Cost
                        IEnumerator ReduceCost(string type)
                        {
                            if (card.Owner.CanReduceCost(null, card))
                            {
                                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                            }

                            Hashtable hashtable = new Hashtable
                            {
                                { "CardEffect", activateClass }
                            };

                            ChangeCostClass changeCostClass = new ChangeCostClass();
                            changeCostClass.SetUpICardEffect($"{type} cost: -2", CanUseCondition1, card);
                            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                            card.Owner.UntilCalculateFixedCostEffect.Add(_ => changeCostClass);
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(hashtable));

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                return true;
                            }

                            int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                            {
                                if (CardSourceCondition(cardSource) &&
                                    RootCondition(root) &&
                                    PermanentsCondition(targetPermanents))
                                {
                                    cost -= 2;
                                }

                                return cost;
                            }

                            bool PermanentsCondition(List<Permanent> targetPermanents)
                            {
                                return targetPermanents == null || targetPermanents.Count(targetPermanent => targetPermanent != null) == 0;
                            }

                            bool CardSourceCondition(CardSource cardSource)
                            {
                                return cardSource != null
                                    && cardSource.Owner == card.Owner;
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

                        if (selectedCard.IsOption)
                        {
                            yield return ContinuousController.instance.StartCoroutine(ReduceCost("Use"));
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                                cardSources: new List<CardSource>() { selectedCard },
                                activateClass: activateClass,
                                payCost: true,
                                root: SelectCardEffect.Root.Hand));

                        }
                        else
                        {
                            yield return ContinuousController.instance.StartCoroutine(ReduceCost("Play"));
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: new List<CardSource>() { selectedCard },
                                activateClass: activateClass,
                                payCost: true,
                                isTapped: false,
                                root: SelectCardEffect.Root.Hand,
                                activateETB: true));
                        }
                    }

                    if (!isUsed) activateClass.RemoveUse();
                }
            }
            #endregion

            #region Inherited
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<Draw 1>, then if 7 or more in hand, trash 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("EX12_0273_Inherited");
                cardEffects.Add(activateClass);

                string EffectDescription()
                 => "[When Attacking] [Once Per Turn] <Draw 1>. Then, if your hand has 7 or more cards, trash 1 card in your hand.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                    if (card.Owner.HandCards.Count >= 7)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: (cardSource) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return StartCoroutine(selectHandEffect.Activate());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
