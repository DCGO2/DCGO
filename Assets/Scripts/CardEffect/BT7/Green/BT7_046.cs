using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT7_046 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            bool Condition()
            {
                return card.Owner.HandCards.Contains(card);
            }

            bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent.TopCard.CardColors.Contains(CardColor.Green) && targetPermanent.IsTamer;
            }

            cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: Condition));
        }

        if(timing == EffectTiming.BeforePayCost)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("As if it were a level 3 digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, "");
            activateClass.SetIsBackgroundProcess(true);
            cardEffects.Add(activateClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnHand(card))
                {
                    bool PermanentCondition(Permanent targetPermanent)
                    {
                        return targetPermanent.TopCard.CardColors.Contains(CardColor.Green) && targetPermanent.IsTamer;
                    }

                    bool CardCondition(CardSource cardSource)
                    {
                        return cardSource == card;
                    }

                    if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(hashtable, PermanentCondition, CardCondition))
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                Permanent selectedPermanent = CardEffectCommons.GetPermanentsFromHashtable(_hashtable)[0];

                bool CanUseChangeCondition(Hashtable ccHashtable)
                {
                    if (selectedPermanent.TopCard != null)
                    {
                        if (card.Owner.GetBattleAreaPermanents().Contains(selectedPermanent))
                        {
                            if (card == selectedPermanent.TopCard)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }


                ChangePermanentLevelClass changePermanentLevelClass = new ChangePermanentLevelClass();
                changePermanentLevelClass.SetUpICardEffect($"Treated as level 3", CanUseChangeCondition, card);
                changePermanentLevelClass.SetUpChangePermanentLevelClass(GetLevel: GetLevel);
                changePermanentLevelClass.SetNotShowUI(true);

                int GetLevel(Permanent permanent, int level)
                {
                    if (selectedPermanent.TopCard != null)
                    {
                        if (permanent == selectedPermanent)
                        {
                            level = 3;
                        }
                    }

                    return level;
                }


                TreatAsDigimonClass treatAsDigimonClass = new TreatAsDigimonClass();
                treatAsDigimonClass.SetUpICardEffect($"Treated as Digimon", CanUseChangeCondition, card);
                treatAsDigimonClass.SetUpTreatAsDigimonClass(
                    permanentCondition: PermanentCondition);
                treatAsDigimonClass.SetNotShowUI(true);

                bool PermanentCondition(Permanent permanent)
                {
                    if (selectedPermanent.TopCard != null)
                    {
                        if (permanent == selectedPermanent)
                        {
                            return true;
                        }
                    }

                    return false;
                }


                DontHaveDPClass dontHaveDPClass = new DontHaveDPClass();
                dontHaveDPClass.SetUpICardEffect("Don't have DP", CanUseChangeCondition, card);
                dontHaveDPClass.SetUpDontHaveDPClass(PermanentCondition: PermanentCondition);
                dontHaveDPClass.SetNotShowUI(true);

                List<Func<EffectTiming, ICardEffect>> getCardEffects =
                    new List<Func<EffectTiming, ICardEffect>>()
                    {
                                                _ => changePermanentLevelClass,
                                                _ => treatAsDigimonClass,
                                                _ => dontHaveDPClass,
                    };

                foreach (Func<EffectTiming, ICardEffect> getCardEffect in getCardEffects)
                {
                   card.Owner.UntilAfterPlayEffect.Add(getCardEffect);
                }

                yield return null;
            }
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Reveal the top 5 cards of deck", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Reveal the top 5 cards of your deck. Add 1 card with [Hybrid] in its traits and 1 [J.P. Shibayama] among them to your hand. Place the remaining cards at the bottom of your deck in any order.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.CardTraits.Contains("Hybrid");
            }

            bool CanSelectCardCondition1(CardSource cardSource)
            {
                if (cardSource.CardNames.Contains("J.P. Shibayama"))
                {
                    return true;
                }

                if (cardSource.CardNames.Contains("J.P.Shibayama"))
                {
                    return true;
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
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
                    revealCount: 5,
                    simplifiedSelectCardConditions:
                    new SimplifiedSelectCardConditionClass[]
                    {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 card with [Hybrid] in its traits.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition1,
                            message: "Select 1 [J.P. Shibayama].",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                    },
                    remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                    activateClass: activateClass,
                    mutualConditions: true
                ));
            }
        }

        return cardEffects;
    }
}
