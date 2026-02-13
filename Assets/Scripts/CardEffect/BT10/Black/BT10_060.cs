using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.BT10
{
    public class BT10_060 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return (targetPermanent.TopCard.CardTraits.Contains("Xros Heart") || targetPermanent.TopCard.CardTraits.Contains("XrosHeart")) && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 2;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) =>
                            permanent != card.PermanentOfThisCard() &&
                            (permanent.IsDigimon || permanent.IsTamer) &&
                            (permanent.TopCard.CardTraits.Contains("Xros Heart") ||
                            permanent.TopCard.CardTraits.Contains("XrosHeart") || permanent.TopCard.CardTraits.Contains("Twilight"))))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 3000, isInheritedEffect: false, card: card, condition: Condition));
            }

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.SaveEffect(card: card));
            }

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            if (card.PermanentOfThisCard().TopCard.ContainsCardName("Shoutmon"))
                            {
                                return true;
                            }

                            if (card.PermanentOfThisCard().TopCard.ContainsCardName("Mervamon"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: true, card: card, condition: Condition));
            }

            return cardEffects;
        }
    }
}