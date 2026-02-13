using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Angewomon
namespace DCGO.CardEffects.BT23
{
    public class BT23_031 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.IsLevel4
                        && targetPermanent.TopCard.HasCSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region Reduce Cost

            bool IsLadyDevimonOrMirei(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                    && permanent.IsDigimon && permanent.TopCard.EqualsCardName("LadyDevimon") || permanent.IsTamer && permanent.TopCard.EqualsCardName("Mirei Mikagura");
            }

            #region Before Pay Cost - Condition Effect

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play cost reduction -3", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetHashString("BT23_031_ReducePlayCost");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When this card would be played from the hand, if you have [LadyDevimon] or [Mirei Mikagura], reduce the play cost by 3.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnHand(card)
                        && CardEffectCommons.HasMatchConditionPermanent(IsLadyDevimonOrMirei);
                }

                bool CardCondition(CardSource cardSource)
                {
                    return CardEffectCommons.IsExistOnHand(cardSource)
                        && cardSource == card;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect("Play Cost -3", CanUseCondition1, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                    card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

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

                                    if (CardEffectCommons.HasMatchConditionPermanent(IsLadyDevimonOrMirei))
                                    {
                                        Cost -= 3;
                                    }
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
                        return root == SelectCardEffect.Root.Hand;
                    }

                    bool isUpDown()
                    {
                        return true;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));
                }
            }

            #endregion

            #region Reduce Play Cost - Not Shown

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -3", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => true, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Contains(card))
                    {
                        ICardEffect activateClass = card.EffectList(EffectTiming.BeforePayCost).Find(cardEffect => cardEffect.EffectName == "Play cost reduction -3");

                        if (activateClass != null)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(IsLadyDevimonOrMirei))
                            {
                                return true;
                            }
                        }
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
                                if (CardEffectCommons.HasMatchConditionPermanent(IsLadyDevimonOrMirei))
                                {
                                    Cost -= 3;
                                }
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
                    return root == SelectCardEffect.Root.Hand;
                }

                bool isUpDown()
                {
                    return true;
                }
            }

            #endregion

            #endregion

            #region OP/WD Shared

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Add top security to hand

                if(card.Owner.SecurityCards.Count > 0)
                {
                    CardSource topCard = card.Owner.SecurityCards[0];

                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { topCard }, false, activateClass));

                    yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                        player: card.Owner,
                        refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());
                }                

                #endregion

                if (card.Owner.SecurityCards.Count <= 3) 
                    yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Add top security to hand. then if you have 3 or less security, <Recovery +1>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Add your top security card to the hand. Then, if you have 3 or fewer security cards, <Recovery +1 (Deck)>";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Add top security to hand. then if you have 3 or less security, <Recovery +1>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Add your top security card to the hand. Then, if you have 3 or fewer security cards, <Recovery +1 (Deck)>";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            #region Alliance - ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: true, card: card, null));
            }

            #endregion

            return cardEffects;
        }
    }
}