using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT5_096 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OptionSkill)
        {
            int maxDP()
            {
                int maxDP = 3000;

                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent.TopCard.ContainsCardName("Omnimon") || permanent.TopCard.HasGarurumonName))
                {
                    maxDP = 5000;
                }

                return maxDP;
            }

            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] Return all of your opponent's Digimon with 3000 DP or less to their owners' hands. If you have a Digimon in play with [Garurumon] or [Omnimon] in its name, return all of your opponent's Digimon with 5000 DP or less to their owners' hands instead. Trash all of the digivolution cards of those Digimon.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.DP <= maxDP())
                    {
                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    List<Permanent> bouncePermanents = new List<Permanent>();

                    foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaDigimons())
                    {
                        if (CanSelectPermanentCondition(permanent))
                        {
                            if (!permanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                bouncePermanents.Add(permanent);
                            }
                        }
                    }

                    if (bouncePermanents.Count >= 1)
                    {
                        Hashtable hashtable = new Hashtable();
                        hashtable.Add("CardEffect", activateClass);

                        yield return ContinuousController.instance.StartCoroutine(new HandBounceClaass(bouncePermanents, hashtable).Bounce());
                    }
                }
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects, effectName: $"Return opponent's Digimons to hand");
        }

        return cardEffects;
    }
}
