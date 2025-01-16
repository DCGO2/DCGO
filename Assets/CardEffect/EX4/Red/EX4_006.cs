using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX4
{
    public class EX4_006 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("Gigimon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon gains Rush", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] If the total number of cards in both players' trashes is 20 or more, this Digimon gains <Rush> for the turn. (This Digimon may attack the turn it was played.)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            int trashSum = 0;

                            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                            {
                                trashSum += player.TrashCards.Count;
                            }

                            if (trashSum >= 20)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRush(
                                    targetPermanent: card.PermanentOfThisCard(),
                                    effectDuration: EffectDuration.UntilEachTurnEnd,
                                    activateClass: activateClass));
                }
            }

            return cardEffects;
        }
    }
}