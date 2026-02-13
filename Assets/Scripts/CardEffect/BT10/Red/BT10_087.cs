using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.BT10
{
    public class BT10_087 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 4 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Reveal the top 4 cards of your deck. Add 1 card with [Xros Heart] in its traits among them to your hand, and place 1 Digimon card with [Xros Heart] in its traits among them under this Tamer. Place the rest at the bottom of your deck in any order.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.CardTraits.Contains("Xros Heart") || cardSource.CardTraits.Contains("XrosHeart");
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("Xros Heart") || cardSource.CardTraits.Contains("XrosHeart"))
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (CardEffectCommons.IsExistOnBattleArea(card))
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
                        revealCount: 4,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 card with [Xros Heart] in its traits.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition1,
                            message: "Select 1 Digimon card with [Xros Heart] in its traits.",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: 1,
                            selectCardCoroutine: SelectCardCoroutine),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                        activateClass: activateClass
                    ));

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { cardSource }, activateClass));
                        }
                    }
                }
            }

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Can select DigiXros cards from Tamer's digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("CanSelectDigiXrosFromTamer_BT10_088");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When you play 1 Digimon with DigiXros requirements, by suspending this Tamer, you may place cards from under one of your Tamers as digivolution cards for a DigiXros.";
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource.Owner == card.Owner)
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (cardSource.HasDigiXros)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition))
                            {
                                if (CardEffectCommons.IsOnly1CardPlayed(hashtable))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanActivateSuspendCostEffect(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(
                        new List<Permanent>() { card.PermanentOfThisCard() },
                        CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                    AddMaxUnderTamerCountDigiXrosClass addMaxTamerCountDigiXrosClass = new AddMaxUnderTamerCountDigiXrosClass();
                    addMaxTamerCountDigiXrosClass.SetUpICardEffect("Can select DigiXros cards from Tamer's digivolution cards", CanUseCondition1, card);
                    addMaxTamerCountDigiXrosClass.SetUpAddMaxUnderTamerCountDigiXrosClass(getMaxUnderTamerCount: GetCount);
                    card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => addMaxTamerCountDigiXrosClass);

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return true;
                    }

                    int GetCount(CardSource cardSource)
                    {
                        if (CardSourceCondition(cardSource))
                        {
                            return 100;
                        }

                        return 0;
                    }

                    bool CardSourceCondition(CardSource cardSource)
                    {
                        if (cardSource != null)
                        {
                            if (cardSource.Owner == card.Owner)
                            {
                                if (cardSource.HasDigiXros)
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
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
}