using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT7_024 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Draw 1 for each opponent's Digimons with no digivolution cards", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] For each Digimon your opponent has with no digivolution cards, <Draw 1>. (Draw 1 card from your deck.)";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    int count = CardEffectCommons.MatchConditionOpponentsPermanentCount(card, (permanent) => permanent.IsDigimon && permanent.HasNoDigivolutionCards);

                    if (count >= 1)
                    {
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                int count = CardEffectCommons.MatchConditionOpponentsPermanentCount(card, (permanent) => permanent.IsDigimon && permanent.HasNoDigivolutionCards);

                yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, count, activateClass).Draw());
            }
        }

        if (timing == EffectTiming.None)
        {
            bool AttackerCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.Level == 3)
                    {
                        if (permanent.TopCard.HasLevel)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            bool Condition()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOpponentTurn(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardTraits.Contains("Hybrid")) >= 1)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.CanNotAttackStaticEffect(attackerCondition: AttackerCondition, defenderCondition: null, isInheritedEffect: false, card: card, condition: Condition, effectName: "Opponent's level 3 Digimon can't Attack"));
        }

        return cardEffects;
    }
}
