using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT8_057 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            CanNotPlayClass canNotPlayClass = new CanNotPlayClass();
            canNotPlayClass.SetUpICardEffect("Opponent can't us Option", CanUseCondition, card);
            canNotPlayClass.SetUpCanNotPlayClass(cardCondition: CardCondition);

            cardEffects.Add(canNotPlayClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.GetBattleAreaDigimons().Count == card.Owner.GetBattleAreaDigimons().Count((permanent) => permanent.IsSuspended))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }



            bool CardCondition(CardSource cardSource)
            {
                if (cardSource != null)
                {
                    if (cardSource.Owner == card.Owner.Enemy)
                    {
                        if (cardSource.IsOption)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        if (timing == EffectTiming.OnUnTappedAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Trash the top card of opponent's security", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Your Turn] When this Digimon becomes unsuspended during your unsuspend phase, trash the top card of your opponent's security stack.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (GManager.instance.turnStateMachine.gameContext.TurnPhase == GameContext.phase.Active)
                        {
                            if (CardEffectCommons.CanTriggerWhenPermanentUnsuspends(hashtable, permanent => permanent == card.PermanentOfThisCard()))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.Enemy.SecurityCards.Count >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                    player: card.Owner.Enemy,
                    destroySecurityCount: 1,
                    cardEffect: activateClass,
                    fromTop: true).DestroySecurity());
            }
        }

        return cardEffects;
    }
}
