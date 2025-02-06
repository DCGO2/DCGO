using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;


public class BT5_017 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            cardEffects.Add(CardEffectFactory.BlitzSelfEffect(isInheritedEffect: false, card: card, condition: null, isWhenDigivolving: true));
        }

        if (timing == EffectTiming.None)
        {
            CanAttackTargetDefendingPermanentClass canAttackTargetDefendingPermanentClass = new CanAttackTargetDefendingPermanentClass();
            canAttackTargetDefendingPermanentClass.SetUpICardEffect($"Can attack to unsuspended Digimon during Blitz", CanUseCondition, card);
            canAttackTargetDefendingPermanentClass.SetUpCanAttackTargetDefendingPermanentClass(
                attackerCondition: AttackerCondition,
                defenderCondition: DefenderCondition,
                cardEffectCondition: CardEffectCondition);
            canAttackTargetDefendingPermanentClass.SetIsInheritedEffect(true);
            cardEffects.Add(canAttackTargetDefendingPermanentClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool AttackerCondition(Permanent permanent)
            {
                return permanent == card.PermanentOfThisCard();
            }

            bool DefenderCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (!permanent.IsSuspended)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CardEffectCondition(ICardEffect cardEffect)
            {
                if (cardEffect != null)
                {
                    if (!string.IsNullOrEmpty(cardEffect.EffectDiscription))
                    {
                        if (cardEffect.EffectDiscription.Contains("Blitz"))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        return cardEffects;
    }
}
