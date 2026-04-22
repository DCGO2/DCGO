using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;


public class BT7_011 : CEntity_Effect
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
                return targetPermanent.TopCard.CardColors.Contains(CardColor.Red) && targetPermanent.IsTamer;
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
            activateClass.SetUpICardEffect("Delete 1 Digimon with 4000 DP or less", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] If a card with [Hybrid] in its traits or [Takuya Kanbara] is in this Digimon's digivolution cards, delete 1 of your opponent's Digimon with 4000 DP or less.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(4000, activateClass))
                    {
                        return true;
                    }
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
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardTraits.Contains("Hybrid") || cardSource.CardNames.Contains("Takuya Kanbara") || cardSource.CardNames.Contains("TakuyaKanbara")) >= 1)
                        {
                            return true;
                        }
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
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
        }

        return cardEffects;
    }
}
