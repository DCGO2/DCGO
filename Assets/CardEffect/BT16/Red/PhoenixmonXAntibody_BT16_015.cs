using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.BT16
{
    public class Phoenixmon_XAntibody_BT16_015 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("Phoenixmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Add End of attack to on deletion", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When a card is removed from your opponentfs security stack, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnAttack(hashtable, card))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) =>
                        (cardSource.IsDigimon && cardSource.ContainsCardName("Phoenixmon"))
                        || cardSource.CardNames.Contains("X Antibody") || cardSource.CardNames.Contains("XAntibody")) >= 1)
                    {
                        return true;
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {

                    List<ICardEffect> candidateEffects = card.PermanentOfThisCard().EffectList(EffectTiming.OnDestroyedAnyone)
                        .Clone()
                        .Filter(cardEffect => cardEffect != null && cardEffect is ActivateICardEffect && !cardEffect.IsSecurityEffect && cardEffect.IsOnDeletion);

                    foreach (ICardEffect cardEffect in candidateEffects)
                    {
                        Debug.Log(candidateEffects);

                                CardEffectCommons.AddEffectToPermanent(
                                targetPermanent: card.PermanentOfThisCard(),
                                effectDuration: EffectDuration.UntilOwnerTurnEnd,
                                card: card,
                                cardEffect: cardEffect,
                                timing: EffectTiming.OnEndAttack);
                    }

                    yield return null;
                }
            
            }
            #endregion

            return cardEffects;
        }
    }
}