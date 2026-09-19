using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT5_044 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnMove)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("The moved Digimon gains Security Attack -3", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Opponent's Turn] When an opponent's Digimon moves from the breeding area to the battle area, it gains <Security Attack -3> (This Digimon checks 3 fewer security cards) for the turn.";
            }

            bool PermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOpponentTurn(card))
                    {
                        if (CardEffectCommons.CanTriggerOnMove(hashtable, PermanentCondition))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    Permanent movedPermanent = CardEffectCommons.GetMovedPermanentFromHashtable(hashtable);

                    if (movedPermanent != null)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(movedPermanent))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                Permanent movedPermanent = CardEffectCommons.GetMovedPermanentFromHashtable(_hashtable);

                if (movedPermanent != null)
                {
                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(movedPermanent))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(targetPermanent: movedPermanent, changeValue: -3, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                    }
                }
            }
        }

        if (timing == EffectTiming.None)
        {
            bool CardCondition(CardSource cardSource)
            {
                return cardSource.Owner == card.Owner.Enemy;
            }

            bool Condition()
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

            cardEffects.Add(CardEffectFactory.ChangeSecurityDigimonCardDPStaticEffect(
                cardCondition: CardCondition,
                changeValue: -3000,
                isInheritedEffect: false,
                card: card,
                condition: Condition,
                effectName: "Opponent's Security Digimon gains DP -3000"));
        }

        return cardEffects;
    }
}
