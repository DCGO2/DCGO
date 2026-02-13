using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;


public class BT9_103 : CEntity_Effect
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
                return "[Main] Until the end of your opponent's turn, your opponent's Digimon with play costs of 7 or less can't attack players, and cards can't be added to security stacks by your opponent's effects.";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                {
                    bool AttackerCondition(Permanent Attacker)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(Attacker, card))
                        {
                            if (Attacker.TopCard.GetCostItself <= 7)
                            {
                                if (Attacker.TopCard.HasPlayCost)
                                {
                                    return true;
                                }
                            }
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
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass,
                        effectName: "Can't Attack to player"));
                }

                {
                    CannotAddSecurityClass cannotAddSecurityClass = new CannotAddSecurityClass();
                    cannotAddSecurityClass.SetUpICardEffect("Can't Add Security", CanUseCondition1, card);
                    cannotAddSecurityClass.SetUpCannotAddSecurityClass(PlayerCondition: PlayerCondition, CardEffectCondition: CardEffectCondition);
                    card.Owner.UntilOpponentTurnEndEffects.Add((_timing) => cannotAddSecurityClass);

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return true;
                    }

                    bool PlayerCondition(Player player)
                    {
                        return true;
                    }

                    bool CardEffectCondition(ICardEffect cardEffect)
                    {
                        return CardEffectCommons.IsOpponentEffect(cardEffect, card);
                    }
                }
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects, effectName: $"Opponent's Digimon can't attack and players can't add security");
        }

        return cardEffects;
    }
}
