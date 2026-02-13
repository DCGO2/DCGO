using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT8_062 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
            changeCardNamesClass.SetUpICardEffect("Also treated as [SkullKnightmon]", CanUseCondition, card);
            changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: changeCardNames);

            cardEffects.Add(changeCardNamesClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                return true;
            }



            List<string> changeCardNames(CardSource cardSource, List<string> CardNames)
            {
                if (cardSource == card)
                {
                    CardNames.Add("SkullKnightmon");
                }

                return CardNames;
            }
        }

        if (timing == EffectTiming.None)
        {
            ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
            changeCardNamesClass.SetUpICardEffect("Also treated as [DeadlyAxemon]", CanUseCondition, card);
            changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: changeCardNames);

            cardEffects.Add(changeCardNamesClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                return true;
            }



            List<string> changeCardNames(CardSource cardSource, List<string> CardNames)
            {
                if (cardSource == card)
                {
                    CardNames.Add("DeadlyAxemon");
                }

                return CardNames;
            }
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Gain Jamming and Blocker", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] This Digimon gains <Jamming> (This Digimon can't be deleted in battles against Security Digimon) and <Blocker> (When an opponent's Digimon attacks, you may suspend this Digimon to force the opponent to attack it instead) until the end of your opponent's next turn.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainJamming(targetPermanent: card.PermanentOfThisCard(), effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(targetPermanent: card.PermanentOfThisCard(), effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
            }
        }

        return cardEffects;
    }
}
