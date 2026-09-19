using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT6_061 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            ChangeCardColorClass changeCardColorClass = new ChangeCardColorClass();
            changeCardColorClass.SetUpICardEffect($"Also treated as red", CanUseCondition, card);
            changeCardColorClass.SetUpChangeCardColorClass(ChangeCardColors: ChangeCardColors);

            cardEffects.Add(changeCardColorClass);

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



            List<CardColor> ChangeCardColors(CardSource cardSource, List<CardColor> CardColors)
            {
                if (cardSource == card)
                {
                    CardColors.Add(CardColor.Red);
                }

                return CardColors;
            }
        }

        if (timing == EffectTiming.None)
        {
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

            cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 2000, isInheritedEffect: true, card: card, condition: Condition));
        }

        return cardEffects;
    }
}
