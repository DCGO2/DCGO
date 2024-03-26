using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class Sora_Takenouchi_BT15_082 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnStartTurn)
        {
            cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
        }
        /*
        if(timing == EffectTiming.None)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Return this Tamer to your hand to play a Digimon from your hand.", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[All Turns] When a red Digimon card returns from your trash to the hand, by returning this Tamer to the hand, you may play 1 13000 DP or lower red Digimon card with [Avian], [Bird], [Beast], [Animal], [Sovereign] in one of its traits, other than [Sea Animal] from your hand without paying the cost. For each of your opponent's security cards, remove 2000 from this effect's playable card's DP maximum.";
            }


        }
        */
        return cardEffects;    
    }
}
