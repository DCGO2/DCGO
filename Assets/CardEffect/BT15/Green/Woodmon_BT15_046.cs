using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class Woodmon_BT15_046 : CEntity_Effect
{
  public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
  {
    List<ICardEffect> cardEffects = new List<ICardEffect>();

    if (timing == EffectTiming.OnTappedAnyone)
    {
      ActivateClass activateClass = new ActivateClass();
      activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
      activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
      activateClass.SetHashString("Draw1_BT15_046");
      cardEffects.Add(activateClass);

      string EffectDiscription()
      {
        return "[Opponent's Turn][Once Per Turn] When one of your Digimon becomes suspended, trash the top card of your opponent's security stack.";
      }

      bool PermanentCondition(Permanent permanent)
      {
        return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
      }

      bool CanUseCondition(Hashtable hashtable)
      {
        if (CardEffectCommons.IsExistOnBattleArea(card))
        {
          if (CardEffectCommons.IsOwnerTurn(card))
          {
            if (CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, PermanentCondition))
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
          return true;
        }

        return false;
      }

      IEnumerator ActivateCoroutine(Hashtable _hashtable)
      {
        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
      }
    }

    return cardEffects;
  }
}
