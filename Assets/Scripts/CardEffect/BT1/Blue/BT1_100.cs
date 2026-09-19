using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT1_100 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OptionSkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] Until the end of your opponent's next turn, their Digimon with no digivolution cards can't attack.";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                bool AttackerCondition(Permanent attacker)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(attacker, card))
                    {
                        if (attacker.HasNoDigivolutionCards)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool DefenderCondition(Permanent defender)
                {
                    return true;
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotAttackPlayerEffect(
                    attackerCondition: AttackerCondition,
                    defenderCondition: DefenderCondition,
                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                    activateClass: activateClass,
                    effectName: "Can't Attack"));
            }
        }


        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect($"Can't Attack", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] Your opponent's Digimon with no digivolution cards can't attack for the turn.";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                bool AttackerCondition(Permanent attacker)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(attacker, card))
                    {
                        if (attacker.HasNoDigivolutionCards)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool DefenderCondition(Permanent defender)
                {
                    return true;
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotAttackPlayerEffect(
                    attackerCondition: AttackerCondition,
                    defenderCondition: DefenderCondition,
                    effectDuration: EffectDuration.UntilEachTurnEnd,
                    activateClass: activateClass,
                    effectName: "Can't Attack"));
            }
        }

        return cardEffects;
    }
}
