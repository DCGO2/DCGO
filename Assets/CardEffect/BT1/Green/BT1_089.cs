using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT1_089 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnStartTurn)
        {
            cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
        }

        if (timing == EffectTiming.OnDeclaration)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Hatch 1 Digiegg or move", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] If you have a level 5 or higher green Digimon in play, you can suspend this Tamer to hatch 1 Digi-Egg card to an empty space in your breeding area, or move 1 level 3 or higher Digimon from your breeding area to your battle area. ";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.CanActivateSuspendCostEffect(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent.TopCard.CardColors.Contains(CardColor.Green) && permanent.Level >= 5 && permanent.TopCard.HasLevel))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                if (card.Owner.CanHatch)
                {
                    yield return ContinuousController.instance.StartCoroutine(new HatchDigiEggClass(player: card.Owner, hashtable: CardEffectCommons.CardEffectHashtable(activateClass)).Hatch());
                }

                else if (card.Owner.GetBreedingAreaPermanents().Count((permanent) => permanent.TopCard.HasLevel && permanent.Level >= 3 && permanent.CanMove) >= 1 && card.Owner.fieldCardFrames.Count((frame) => frame.IsEmptyFrame()) >= 1)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.MovePermanent(card.Owner.GetBreedingAreaPermanents()[0].PermanentFrame));
                }
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
        }

        return cardEffects;
    }
}
