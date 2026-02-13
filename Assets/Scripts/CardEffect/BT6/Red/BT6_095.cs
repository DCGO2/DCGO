using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT6_095 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
            ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
            ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);
            cardEffects.Add(ignoreColorConditionClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent.TopCard.CardTraits.Contains("Three Musketeers") || permanent.TopCard.CardTraits.Contains("ThreeMusketeers"));
            }

            bool CardCondition(CardSource cardSource)
            {
                return cardSource == card;
            }
        }

        if (timing == EffectTiming.OptionSkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] Delete all of your opponent's Digimon with the lowest DP.";
            }

            bool PermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                List<Permanent> destroyTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(PermanentCondition);
                yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(destroyTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects, effectName: $"Delete opponent's all Digimons with the lowest DP");
        }

        return cardEffects;
    }
}
