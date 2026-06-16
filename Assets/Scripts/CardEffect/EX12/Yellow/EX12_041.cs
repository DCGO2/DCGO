using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Thundermon
namespace DCGO.CardEffects.EX12
{
    public class EX12_041 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("ME");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 3)
                );
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May play/use 1 card with [Mutant]/[ME] trait from hand for 2 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("EX12_041_Main");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] [Once Per Turn] You may play or use 1 card with the [Mutant] or [ME] trait from your hand with the cost reduced by 2.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return (cardSource.EqualsTraits("Mutant")
                            || cardSource.EqualsTraits("ME"))
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
                        isUsed = true;

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

            #region Rule
            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Also treated as including [Mamemon]", CanUseCondition, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: changeCardNames);

                cardEffects.Add(changeCardNamesClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                List<string> changeCardNames(CardSource cardSource, List<string> CardNames)
                {
                    if (cardSource == card)
                    {
                        CardNames.Add("Mamemon_EX12_041");
                    }

                    return CardNames;
                }
            }
            #endregion

            #region Inherited
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 enemy Digimon gets -2K DP for turn", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("EX12_041_Inherited");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] [Once Per Turn] 1 of your opponent's Digimon gets -2000 DP for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentCondition))
                    {
                        Permanent selectedPermament = null;

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

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermament = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to -2K DP", "The opponent is selecting 1 Digimon to -2K DP");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedPermament != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            targetPermanent: selectedPermament,
                            changeValue: -2000,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass
                            ));
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
