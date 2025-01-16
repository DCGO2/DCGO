using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.BT10
{
    public class BT10_042 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Security Attack -1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] All of your opponent's Digimon gain <Security Attack -1> until the end of your opponent's turn. (This Digimon checks 1 fewer security cards.)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool PermanentCondition(Permanent permanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttackPlayerEffect(
                        permanentCondition: PermanentCondition,
                        changeValue: -1,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass));
                }
            }

            if (timing == EffectTiming.None)
            {
                bool AttackerCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.HasSecurityAttackChanges)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool DefenderCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                string effectName = "Opponent's Digimon that has <Security Attack> can't attack to this Digimon";

                cardEffects.Add(CardEffectFactory.CanNotAttackStaticEffect(
                    attackerCondition: AttackerCondition,
                    defenderCondition: DefenderCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition,
                    effectName: effectName
                ));
            }

            if (timing == EffectTiming.None)
            {
                DisableEffectClass invalidationClass = new DisableEffectClass();
                invalidationClass.SetUpICardEffect("Ignore [When Attacking] and [When Digivolving] Effect of opponent's Digimon that has <Security Attack>", CanUseCondition, card);
                invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                cardEffects.Add(invalidationClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                bool InvalidateCondition(ICardEffect cardEffect)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            if (cardEffect != null)
                            {
                                if (cardEffect is ActivateICardEffect)
                                {
                                    if (cardEffect.EffectSourceCard != null)
                                    {
                                        if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(cardEffect.EffectSourceCard.PermanentOfThisCard(), card))
                                        {
                                            if (cardEffect.EffectSourceCard.PermanentOfThisCard().HasSecurityAttackChanges)
                                            {
                                                if (!cardEffect.EffectSourceCard.PermanentOfThisCard().TopCard.CanNotBeAffected(invalidationClass))
                                                {
                                                    if (cardEffect.IsWhenDigivolving)
                                                    {
                                                        return true;
                                                    }

                                                    if (cardEffect.IsOnAttack)
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }
            }

            return cardEffects;
        }
    }
}