using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT5_103 : CEntity_Effect
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
                return "[Main] Until the end of your opponent's next turn, all of your Digimon with <Reboot> get +1000 DP and <Blocker>. (When an opponent's Digimon attacks, you may suspend this Digimon to force the opponent to attack it instead.)";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.HasReboot)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDPPlayerEffect(
                    permanentCondition: PermanentCondition,
                    changeValue: 1000,
                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                    activateClass: activateClass));

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlockerPlayerEffect(
                    permanentCondition: PermanentCondition,
                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                    activateClass: activateClass));
            }
        }


        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect($"All opponent's Digimon can't attack to player and add this card to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] Your opponent's Digimon can't attack players for the turn. Then, add this card to your hand.";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                bool AttackerCondition(Permanent Attacker)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(Attacker, card))
                    {
                        return true;
                    }

                    return false;
                }

                bool DefenderCondition(Permanent Defender)
                {
                    return Defender == null;
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotAttackPlayerEffect(
                    attackerCondition: AttackerCondition,
                    defenderCondition: DefenderCondition,
                    effectDuration: EffectDuration.UntilEachTurnEnd,
                    activateClass: activateClass,
                    effectName: "Can't Attack to player"));

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
            }
        }

        return cardEffects;
    }
}
