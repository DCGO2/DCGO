using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Negamon
namespace DCGO.CardEffects.EX9
{
    public class EX9_005 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Breeding - Main
            if(timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reduce play cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("Reduce_EX9_005");

                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Breeding] [Main] [Once Per Turn] You may play 1 Digimon card with [Negamon] in its text from your hand with the play cost reduced by 2. For each [Negamon] in your trash or your Digimon's digivolution cards, further reduce it by 1. Then, place this Digimon as the played Digimon's bottom digivolution card.";
                }

                int ReductionAmount()
                {
                    int value = 2;

                    value += CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, IsNegamon);

                    foreach (Permanent perm in card.Owner.GetBattleAreaDigimons())
                    {
                        value += perm.DigivolutionCards.Count(IsNegamon);
                    }

                    return value;
                }

                bool IsNegamon(CardSource source)
                {
                    return source.EqualsCardName("Negamon");
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource.Owner == card.Owner)
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (cardSource.HasText("Negamon"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBreedingArea(card))
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
                    return CardEffectCommons.IsExistOnBreedingArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    int reduceCount = ReductionAmount();

                    #region Reduce Play Cost

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect($"Play Cost -{reduceCount}", _ => true, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardCondition,
                        rootCondition: RootCondition, isUpDown: () => true, isCheckAvailability: () => false,
                        isChangePayingCost: () => true);
                    Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                    card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                    ICardEffect GetCardEffect(EffectTiming rcTiming)
                    {
                        if (rcTiming == EffectTiming.None)
                        {
                            return changeCostClass;
                        }

                        return null;
                    }

                    int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                    {
                        if (CardCondition(cardSource))
                        {
                            if (RootCondition(root))
                            {
                                if (PermanentsCondition(targetPermanents))
                                {
                                    cost -= reduceCount;
                                }
                            }
                        }

                        return cost;
                    }

                    bool PermanentsCondition(List<Permanent> targetPermanents)
                    {
                        if (targetPermanents == null)
                        {
                            return true;
                        }

                        return targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0;
                    }

                    bool RootCondition(SelectCardEffect.Root root)
                    {
                        return true;
                    }

                    #endregion

                    List<CardSource> selectedCards = new List<CardSource>();

                    if (card.Owner.HandCards.Count(CardCondition) >= 1)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 Digimon to play.",
                            "The opponent is selecting 1 Digimon to play.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass,
                                payCost: true, isTapped: false, root: SelectCardEffect.Root.Hand, activateETB: true));
                    }

                    #region Remove Reduce Cost effect

                    card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);

                    #endregion

                    if(selectedCards.Count > 0 && CardEffectCommons.IsExistOnBattleAreaDigimon(selectedCards[0]))
                    {
                        Permanent targetPermanent = selectedCards[0].PermanentOfThisCard();

                        yield return ContinuousController.instance.StartCoroutine(card.Owner.brainStormObject.CloseBrainstrorm(card));

                        yield return ContinuousController.instance.StartCoroutine(targetPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { card }, activateClass));
                    }
                }
            }
            #endregion

            #region Breeding - All Turns
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBreedingArea(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                bool CardEffectCondition(ICardEffect cardEffect)
                {
                    return cardEffect != null;
                }

                cardEffects.Add(CardEffectFactory.CanNotDigivolveStaticSelfEffect(
                    cardCondition: null,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition,
                    effectName: "Can't digivolve"
                ));

                cardEffects.Add(CardEffectFactory.CanNotBeDestroyedBySkillStaticEffect(
                    permanentCondition: PermanentCondition,
                    cardEffectCondition: CardEffectCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition,
                    effectName: "Can't be deleted by effects"
                ));

                cardEffects.Add(CardEffectFactory.CanNotBeTrashedBySkillStaticEffect(
                   permanentCondition: PermanentCondition,
                   cardEffectCondition: CardEffectCondition,
                   isInheritedEffect: false,
                   card: card,
                   condition: Condition,
                   effectName: "Can't be trashed by effects")
                   );
            }

            if (timing == EffectTiming.None)
            {

                
            }
            #endregion

            #region Opponents Turn - ESS
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Switch attack target", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("SwitchTarget_EX9-005");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Opponent's Turn] [Once Per Turn] When one of your opponent's Digimon attacks, you may change the attack target to 1 of your Digimon with [Negamon] in its text.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.HasText("Negamon");
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsOpponentPermanent(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, PermanentCondition))
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (GManager.instance.attackProcess.IsAttacking)
                        {
                            if (GManager.instance.attackProcess.AttackingPermanent.CanSwitchAttackTarget)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(GManager.instance.attackProcess.AttackingPermanent, card))
                        {
                            if (GManager.instance.attackProcess.IsAttacking)
                            {
                                if (GManager.instance.attackProcess.AttackingPermanent.CanSwitchAttackTarget)
                                {
                                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

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

                                    selectPermanentEffect.SetUpCustomMessage(
                                        "Select 1 Digimon to switch the attack to.",
                                        "The opponent is selecting 1 Digimon to switch the attack to.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SwitchDefender(
                                        activateClass,
                                        false,
                                        permanent));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}