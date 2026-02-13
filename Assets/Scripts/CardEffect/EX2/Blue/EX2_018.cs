using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX2
{
    public class EX2_018 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Recovery (Deck)", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] For each of your opponent's Digimon with no digivolution cards, <Recovery +1 (Deck)>. (Place the top card of your deck on top of your security stack.) This effect can't increase the number of cards in your security stack to 6 or more.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            if (card.Owner.CanAddSecurity(activateClass))
                            {
                                int addSecurityCount = card.Owner.Enemy.GetBattleAreaDigimons().Count((permanent) => permanent.HasNoDigivolutionCards);

                                if (card.Owner.SecurityCards.Count + addSecurityCount >= 5)
                                {
                                    addSecurityCount = 5 - card.Owner.SecurityCards.Count;
                                }

                                if (addSecurityCount >= 1)
                                {
                                    activateClass.SetEffectName($"Recovery +{addSecurityCount} (Deck)");

                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    int addSecurityCount = card.Owner.Enemy.GetBattleAreaDigimons().Count((permanent) => permanent.HasNoDigivolutionCards);

                    if (card.Owner.SecurityCards.Count + addSecurityCount >= 5)
                    {
                        addSecurityCount = 5 - card.Owner.SecurityCards.Count;
                    }

                    if (addSecurityCount >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, addSecurityCount, activateClass).Recovery());
                    }
                }
            }

            return cardEffects;
        }
    }
}