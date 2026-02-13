using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    public static IEnumerator ShowReducedCost(Hashtable hashtable)
    {
        PlayCardClass playCard = GetPlayCardClassFromHashtable(hashtable);

        if (playCard != null)
        {
            if (playCard.PayCost)
            {
                CardSource Card = GetCardFromHashtable(hashtable);

                if (Card != null)
                {
                    List<Permanent> Permanents = GetPermanentsFromHashtable(hashtable);
                    {
                        GManager.instance.memoryObject.ShowMemoryPredictionLine(
                                                Card.Owner.ExpectedMemory(Card.PayingCost(playCard.Root, Permanents, checkAvailability: false)));

                        yield return new WaitForSeconds(0.2f);
                    }
                }
            }
        }
    }
}