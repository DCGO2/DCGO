using System;
using System.Collections;
using System.Collections.Generic;

// Nyaromon
namespace DCGO.CardEffects.EX12
{
    public class EX12_001 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Inherit
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May DNA this [VB] Digimon and 1 other into a [VB] in hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[End of Your Turn] This [VB] trait Digimon and any of your other Digimon may DNA digivolve into a [VB] trait Digimon card in the hand. Then, that DNA digivolved Digimon may attack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectDNACondition)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent != card.PermanentOfThisCard())
                        && card.PermanentOfThisCard().TopCard.EqualsTraits("VB");
                }

                bool CanSelectDNACondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("VB")
                        && cardSource.jogressCondition.Count > 0;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Func<Permanent, bool>[] permanentConditions = new Func<Permanent, bool>[] { permanent => permanent == card.PermanentOfThisCard() };

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DNADigivolvePermanentsIntoHandOrTrashCard(
                        canSelectDNACardCondition: CanSelectDNACondition,
                        payCost: true,
                        isHand: true,
                        activateClass,
                        permanentConditions
                    ));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
