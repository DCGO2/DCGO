using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT7_089 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.SecuritySkill)
        {
            cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
        }

        if (timing == EffectTiming.None)
        {
            bool Condition()
            {
                return CardEffectCommons.IsOwnerTurn(card) && CardEffectCommons.IsExistOnBattleArea(card);
            }

            bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent == card.PermanentOfThisCard();
            }

            bool CardSourceCondition(CardSource cardSource)
            {
                return cardSource.IsDigimon && cardSource.CardColors.Contains(CardColor.Green);
            }

            bool RootCondition(SelectCardEffect.Root root)
            {
                return true;
            }

            cardEffects.Add(CardEffectFactory.ChangeDigivolutionCostStaticEffect(
                changeValue: -1,
                permanentCondition: PermanentCondition,
                cardCondition: CardSourceCondition,
                rootCondition: RootCondition,
                isInheritedEffect: false,
                card: card,
                condition: Condition,
                setFixedCost: false));
        }

        if (timing == EffectTiming.OnDetermineDoSecurityCheck)
        {
            cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: true, card: card, condition: null));
        }

        #region Blank ESS

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Placeholder to mark as having inheritable", _ => false, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, "No Effect");
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetIsBackgroundProcess(true);
                cardEffects.Add(activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return null;
                }
            }

            #endregion

        return cardEffects;
    }
}
