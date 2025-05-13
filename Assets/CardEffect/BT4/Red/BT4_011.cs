using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;


public class BT4_011 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        List<Func<EffectTiming, ICardEffect>> getCardEffects = new List<Func<EffectTiming, ICardEffect>>();

        if (timing == EffectTiming.None)
        {
            bool Condition()
            {
                return card.Owner.HandCards.Contains(card);
            }

            bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent.TopCard.CardColors.Contains(CardColor.Red) && targetPermanent.IsTamer;
            }

            cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: Condition));
        }

        if(timing == EffectTiming.BeforePayCost)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Trash your Security to reduce digivolution cost", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, "");
            activateClass.SetIsBackgroundProcess(true);
            cardEffects.Add(activateClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnHand(card))
                {
                    bool PermanentCondition(Permanent targetPermanent)
                    {
                        return targetPermanent.TopCard.CardColors.Contains(CardColor.Red) && targetPermanent.IsTamer;
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

               getCardEffects =
                    new List<Func<EffectTiming, ICardEffect>>()
                    {
                                                _ => changePermanentLevelClass,
                                                _ => treatAsDigimonClass,
                                                _ => dontHaveDPClass,
                    };

                foreach (Func<EffectTiming, ICardEffect> getCardEffect in getCardEffects)
                {
                   card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);
                }

                yield return null;
            }
        }

        return cardEffects;
    }
}
