using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Frimon
namespace DCGO.CardEffects.BT25
{
    public class BT25_003 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Inherited
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing top security, digivolve into card with [Glowing Dawn] trait for 1 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT25_003_Inherited");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] [Once Per Turn] By trashing your top security card, this Digimon may digivolve into a [Glowing Dawn] trait card in the hand with the cost reduced by 1.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.Owner.SecurityCards.Any();
                }

                bool CardCondition(CardSource cardSource)
                {
                    return CardEffectCommons.IsExistOnHand(cardSource)
                        && cardSource.EqualsTraits("Glowing Dawn");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    #region Make Selection for Trashing Security

                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());

                    #endregion

                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CardCondition))
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: card.PermanentOfThisCard(),
                                cardCondition: CardCondition,
                                payCost: true,
                                reduceCostTuple: (1, null),
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: true,
                                activateClass: activateClass,
                                successProcess: null));
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
