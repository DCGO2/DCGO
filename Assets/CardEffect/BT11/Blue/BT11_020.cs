using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT11
{
    public class BT11_020 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Reveal the top 3 cards of your deck. Add 1 Digimon card with [Gaogamon] in its name and 1 blue Tamer card among them to your hand. Trash the rest.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.ContainsCardName("Gaogamon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    if (cardSource.IsTamer)
                    {
                        if (cardSource.CardColors.Contains(CardColor.Blue))
                        {
                            return true;
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
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message:  "Select 1 Digimon card with [Gaogamon] in its name.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition1,
                            message: "Select 1 blue Tamer card.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        },
                        remainingCardsPlace: RemainingCardsPlace.Trash,
                        activateClass: activateClass
                    ));
                }
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 level 3 Digimon to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Bounce_BT11_020");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking][Once Per Turn] If you have a Tamer in play, return 1 of your opponent's level 3 Digimon to its owner's hand.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.Level == 3)
                        {
                            if (permanent.TopCard.HasLevel)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsTamer))
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
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Bounce,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            return cardEffects;
        }
    }
}