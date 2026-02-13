using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT1_088 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnDeclaration)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Reveal the top card of deck", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] If you have a level 5 or higher green Digimon in playCyou can suspend this Tamer to reveal the top card of your deck. If that card is a Digimon cardCadd it to your hand. Otherwise place it at the bottom of your deck.";
            }

            bool CardCondition(CardSource cardSource)
            {
                return cardSource.IsDigimon;
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

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndProcessForAll(
                                    revealCount: 1,
                                    simplifiedSelectCardCondition:
                                    new SimplifiedSelectCardConditionClass(
                                            canTargetCondition: CardCondition,
                                            message: "",
                                            mode: SelectCardEffect.Mode.AddHand,
                                            maxCount: -1,
                                            selectCardCoroutine: null),
                                    remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                                    activateClass: activateClass
                                ));
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
        }

        return cardEffects;
    }
}
