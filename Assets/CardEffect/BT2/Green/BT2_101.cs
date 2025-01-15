using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT2_101 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OptionSkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] Suspend all of your opponent's Digimon with 6000 DP or less.";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                List<Permanent> tappedPermanents = new List<Permanent>();

                foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaDigimons())
                {
                    if (permanent.DP <= 6000)
                    {
                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            tappedPermanents.Add(permanent);
                        }
                    }
                }

                Hashtable hashtable = new Hashtable();
                hashtable.Add("CardEffect", activateClass);

                yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(tappedPermanents, hashtable).Tap());
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects, effectName: $"Suspend opponent's all Digimons with 6000 DP or less");
        }

        return cardEffects;
    }
}
