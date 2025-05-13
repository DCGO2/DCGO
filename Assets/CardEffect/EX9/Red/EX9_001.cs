using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;

//Koromon
namespace DCGO.CardEffects.EX9
{
    public class EX9_001 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region When Attacking - ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve this digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Digivolve_EX9_001");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] [Once Per Turn] This Digimon with face-down digivolution cards may digivolve into a [Ver.1] trait Digimon card in the hand with the digivolution cost reduced by 1.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerOnAttack(hashtable, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanSelectCardCondition(CardSource digivolveTarget, Permanent digivolvingTarget)
                {
                    if (digivolveTarget.EqualsTraits("Ver.1"))
                    {
                        if (digivolveTarget.CanPlayCardTargetFrame(digivolvingTarget.PermanentFrame, true, activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.PermanentOfThisCard().HasFaceDownDigivolutionCards;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: selectedPermanent,
                                cardCondition: (cardSource) => CanSelectCardCondition(cardSource, selectedPermanent),
                                payCost: true,
                                reduceCostTuple: (reduceCost: 1, reduceCostCardCondition: null),
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
