using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX7
{
    public class EX7_006 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region When Attaking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [Dark Dragon]/[Evil Dragon] trait trash card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("DigivolveTrash_EX7_006");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] If you have 4 or fewer cards in your hand, this Digimon may digivolve into a Digimon card with the [Dark Dragon]/[Evil Dragon] trait in the trash.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (card.Owner.HandCards.Count <= 4)
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                   if(cardSource.IsDigimon)
                   {
                       if(cardSource.ContainsTraits("Dark Dragon") || cardSource.ContainsTraits("Evil Dragon"))
                       {
                           if(cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, true, activateClass))
                           {
                               return true;
                           }
                       }
                   }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                       targetPermanent: card.PermanentOfThisCard(),
                       cardCondition: CanSelectCardCondition,
                       payCost: true,
                       reduceCostTuple: null,
                       fixedCostTuple: null,
                       ignoreDigivolutionRequirementFixedCost: -1,
                       isHand: false,
                       activateClass: activateClass,
                       successProcess: null));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}