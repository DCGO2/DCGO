using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT7_036 : CEntity_Effect
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
                return targetPermanent.TopCard.CardColors.Contains(CardColor.Yellow) && targetPermanent.IsTamer;
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
                        return targetPermanent.TopCard.CardColors.Contains(CardColor.Yellow) && targetPermanent.IsTamer;
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
            activateClass.SetUpICardEffect("DP +3000 for your Security Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] If a card with [Hybrid] in its traits or [Zoe Orimoto] is in this Digimon's digivolution cards, all of your Security Digimon get +3000 DP until the end of your opponent's next turn.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardTraits.Contains("Hybrid") || cardSource.CardNames.Contains("Zoe Orimoto") || cardSource.CardNames.Contains("ZoeOrimoto")) >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeSecurityDigimonCardDPPlayerEffect(
                    cardCondition: cardSource => cardSource.Owner == card.Owner,
                    changeValue: 3000,
                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                    activateClass: activateClass));
            }
        }

        return cardEffects;
    }
}
