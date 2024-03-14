using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class Kurisarimon_EX1_046 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnDestroyedAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
            activateClass.SetIsInheritedEffect(true);
            activateClass.SetHashString("Unsuspend_EX6_046");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Your Turn][Once Per Turn] When one of your other Digimon with the same name as this Digimon is deleted, unsuspend this Digimon.";
            }

            bool PermanentCondition(Permanent permanent)
            {
                Debug.Log("A001");

                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    Debug.Log("A002");

                    if (permanent != card.PermanentOfThisCard())
                    {
                        Debug.Log("A003");

                        if (permanent.TopCard.HasSameCardName(card.PermanentOfThisCard().TopCard))
                        {
                            Debug.Log("A004");

                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                Debug.Log("B001");

                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    Debug.Log("B002");

                    if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition))
                    {
                        Debug.Log("B003");

                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            Debug.Log("B004");

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
                Permanent selectedPermanent = card.PermanentOfThisCard();

                yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());
            }
        }

        return cardEffects;
    }
}
