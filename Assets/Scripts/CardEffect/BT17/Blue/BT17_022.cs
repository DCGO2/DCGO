using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT17
{
    public class BT17_022 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            // Koji Minamoto
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return card.Owner.HandCards.Contains(card);
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Koji Minamoto");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false,
                    card: card, condition: Condition));
            }

            // KendoGarurumon
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("KendoGarurumon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false,
                    card: card, condition: null));
            }

            // Any yellow tamer
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return card.Owner.HandCards.Contains(card);
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.IsTamer && targetPermanent.TopCard.CardColors.Contains(CardColor.Yellow);
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false,
                    card: card, condition: Condition));
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon digivolves into [AncientGarurumon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] If [KendoGarurumon] is in this Digimon's digivolution cards or you have a black or purple Digimon or Tamer, this Digimon may digivolve into [KendoGarurumon] in the hand for a digivolution cost of 3, ignoring its digivolution requirements. If digivolved by this effect, delete this Digimon at the end of the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count(cardSource =>
                                cardSource.EqualsCardName("KendoGarurumon")) >= 1)
                        {
                            return true;
                        }

                        if (card.Owner.HandCards.Count >= 1)
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersPermanent(card,
                                    permanent =>
                                        permanent.TopCard.CardColors.Contains(CardColor.Black) ||
                                        permanent.TopCard.CardColors.Contains(CardColor.Purple)))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        Permanent thisPermanent = card.PermanentOfThisCard();

                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: thisPermanent,
                                cardCondition: cardSource =>
                                    cardSource.IsDigimon && cardSource.EqualsCardName("AncientGarurumon"),
                                payCost: true,
                                reduceCostTuple: null,
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: 3,
                                isHand: true,
                                activateClass: activateClass,
                                successProcess: SuccessProcess()));

                        IEnumerator SuccessProcess()
                        {
                            ActivateClass activateClass1 = new ActivateClass();
                            activateClass1.SetUpICardEffect("Delete this Digimon", CanUseSuccessCondition, card);
                            activateClass1.SetUpActivateClass(CanActivateSuccessCondition, ActivateSuccessCoroutine, -1,
                                false, "");
                            activateClass1.SetEffectSourcePermanent(thisPermanent);
                            CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilEachTurnEnd,
                                card: card, cardEffect: activateClass1, timing: EffectTiming.OnEndTurn);

                            bool CanUseSuccessCondition(Hashtable successHashtable)
                            {
                                return true;
                            }

                            bool CanActivateSuccessCondition(Hashtable successHashtable)
                            {
                                if (thisPermanent.TopCard != null)
                                {
                                    if (thisPermanent.TopCard.IsDigimon)
                                    {
                                        if (thisPermanent.CanBeDestroyedBySkill(activateClass1))
                                        {
                                            if (!thisPermanent.TopCard.CanNotBeAffected(activateClass1))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }

                                return false;
                            }

                            IEnumerator ActivateSuccessCoroutine(Hashtable successHashtable)
                            {
                                yield return ContinuousController.instance.StartCoroutine(
                                    new DestroyPermanentsClass(new List<Permanent>() { thisPermanent },
                                        CardEffectCommons.CardEffectHashtable(activateClass1)).Destroy());
                            }

                            yield return null;
                        }
                    }
                }
            }

            #endregion

            #region When Attacking - ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Attacking] If you have 7 or fewer cards in your hand, [Draw 1].";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (card.Owner.HandCards.Count <= 7)
                        {
                            if (card.Owner.LibraryCards.Count >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}