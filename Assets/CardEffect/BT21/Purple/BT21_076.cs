    using System.Collections;
using System.Collections.Generic;

//BT21-076 Wargrowlmon
namespace DCGO.CardEffects.BT21
{
    public class BT21_076 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Growlmon") && 
                           targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region On Play/When Digivolving shared
            bool CanActivateConditionShared(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }
            #endregion

            #region On Play
            if(timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("trash 2, get raid retal", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] Trash the top 2 cards of your deck. Then, this Digimon gains <Raid> and <Retaliation> until your opponent's turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.LibraryCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(2, card.Owner, activateClass).AddTrashCardsFromLibraryTop());
                    }
                    yield return CardEffectCommons.GainRaid(card.PermanentOfThisCard(), EffectDuration.UntilOpponentTurnEnd, activateClass);
                    yield return CardEffectCommons.GainRetaliation(card.PermanentOfThisCard(), EffectDuration.UntilOpponentTurnEnd, activateClass);
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("trash 2, get raid retal", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] Trash the top 2 cards of your deck. Then, this Digimon gains <Raid> and <Retaliation> until your opponent's turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.LibraryCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(2, card.Owner, activateClass).AddTrashCardsFromLibraryTop());
                    }
                    yield return CardEffectCommons.GainRaid(card.PermanentOfThisCard(), EffectDuration.UntilOpponentTurnEnd, activateClass);
                    yield return CardEffectCommons.GainRetaliation(card.PermanentOfThisCard(), EffectDuration.UntilOpponentTurnEnd, activateClass);
                }
            }
            #endregion

            #region When Attacking
            if(timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve to Megidra or ChaosGallant", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT21_076 when attacking");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking][Once Per Turn] This Digimon may digivolve into a Digimon card with [Megidramon] or [ChaosGallantmon] in its name in the hand. For every 10 total cards in both players' trashes, reduce this effect's digivolution cost by 1.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanDigivolveIntoCardCondition(CardSource cardsource)
                {
                    return cardsource.ContainsCardName("Megidramon") || cardsource.ContainsCardName("ChaosGallantmon");
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) && CardEffectCommons.HasMatchConditionOwnersHand(card, CanDigivolveIntoCardCondition);
                }

                int Count()
                {
                    return (card.Owner.TrashCards.Count + card.Owner.Enemy.TrashCards.Count) / 10;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if(CardEffectCommons.HasMatchConditionOwnersHand(card, CanDigivolveIntoCardCondition))
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: card.PermanentOfThisCard(),
                                cardCondition: CanDigivolveIntoCardCondition,
                                payCost: true,
                                reduceCostTuple: (reduceCost: Count(), reduceCostCardCondition: null),
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: true,
                                activateClass: activateClass,
                                successProcess: null));
                    }

                    
                }
            }
            #endregion

            #region On Deletion Inherit
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash the top card of your opponent's security stack.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Deletion] Trash the top card of your opponent's security stack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanActivateOnDeletionInherited(hashtable, card))
                    {
                        if (card.Owner.Enemy.SecurityCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}