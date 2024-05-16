using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class Moonmon_EX5_002 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("This Digimon digivolves", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
            activateClass.SetHashString("Digivolve_EX5_002");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Your Turn] [Once Per Turn] When you play a tamer with the [Night Claw]/[Light Fang] trait, this Digimon may digivolve into a Digimon card in your hand.";
            }

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                {
                    if (permanent.IsTamer)
                    {
                        if (permanent.TopCard.ContainsTraits("Light Fang"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.ContainsTraits("LightFung"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.ContainsTraits("Night Claw"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.ContainsTraits("NightClaw"))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.IsDigimon;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition))
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
                    if (card.Owner.HandCards.Count >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                    targetPermanent: card.PermanentOfThisCard(),
                    cardCondition: CanSelectCardCondition,
                    payCost: true,
                    reduceCostTuple: null,
                    fixedCostTuple: null,
                    ignoreDigivolutionRequirementFixedCost: -1,
                    isHand: true,
                    activateClass: activateClass,
                    successProcess: null));
            }
        }

        return cardEffects;
    }
}
