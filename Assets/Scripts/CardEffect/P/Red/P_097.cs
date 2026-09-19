using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class P_097 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Place this Digimon under your Digimon's digivolution cards to reveal deck tops and to gain Memory +2", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] By placing this card under 1 of your other Digimon in play as its bottom digivolution card, reveal the top 3 cards of your deck. Place those cards at either the top or bottom of your deck in any order. Then, if you have a Digimon with the [Legend-Arms] trait in play, gain 2 memory.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    if (permanent != card.PermanentOfThisCard())
                    {
                        if (!permanent.IsToken)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    bool added = false;

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get a digivolution card.", "The opponent is selecting 1 Digimon that will get a digivolution card.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        Permanent selectedPermanent = permanent;

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(new List<Permanent[]>() { new Permanent[] { card.PermanentOfThisCard(), selectedPermanent } }, false, activateClass).PlacePermanentToDigivolutionCards());

                            if (CardEffectCommons.IsExistOnBattleArea(card))
                            {
                                if (selectedPermanent.DigivolutionCards.Contains(card))
                                {
                                    added = true;
                                }
                            }
                        }
                    }

                    if (added)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndProcessForAll(
                            revealCount: 3,
                            simplifiedSelectCardCondition:
                            new SimplifiedSelectCardConditionClass(
                                    canTargetCondition: (cardSource) => false,
                                    message: "",
                                    mode: SelectCardEffect.Mode.Custom,
                                    maxCount: -1,
                                    selectCardCoroutine: null),
                            remainingCardsPlace: RemainingCardsPlace.DeckTopOrBottom,
                            activateClass: activateClass
                        ));

                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent.TopCard.CardTraits.Contains("Legend-Arms")))
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(2, activateClass));
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.OnAllyAttack)
        {
            bool Condition()
            {
                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent.TopCard.CardColors.Contains(CardColor.Black) || permanent.TopCard.CardTraits.Contains("Legend-Arms")))
                {
                    return true;
                }

                return false;
            }
            cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: true, card: card, condition: Condition));
        }

        return cardEffects;
    }
}
