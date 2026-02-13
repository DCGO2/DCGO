using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT6_028 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnDeclaration)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Your all Digimons gain unblockable", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] <Digi-Burst 2> (Trash 2 of this Digimon's Digivolution cards to activate the effect below.) - Your Digimon can't be blocked by your opponent's Digimon this turn.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (new IDigiBurst(card.PermanentOfThisCard(), 2, activateClass).CanDigiBurst())
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(new IDigiBurst(card.PermanentOfThisCard(), 2, activateClass).DigiBurst());

                bool AttackerCondition(Permanent attacker)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(attacker, card);
                }

                bool DefenderCondition(Permanent defender)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(defender, card);
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotBlockPlayerEffect(attackerCondition: AttackerCondition, defenderCondition: DefenderCondition, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass, effectName: "Can't Block"));

                foreach (Permanent permanent in card.Owner.GetBattleAreaPermanents())
                {
                    if (AttackerCondition(permanent))
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(permanent));
                    }
                }
            }
        }

        return cardEffects;
    }
}
