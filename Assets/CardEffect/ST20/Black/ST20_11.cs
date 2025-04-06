using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//ST20 Wargreymon
namespace DCGO.CardEffects.ST20
{
    public class ST20_11 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt digivolution condition
            if(timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return (permanent.TopCard.HasAdventureTraits || permanent.TopCard.EqualsTraits("Hero"))
                        && permanent.Level == 5;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, false, card, null));
            }
            #endregion

            #region Ace - Blast Digivolve

            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }

            #endregion

            #region On Play/When Digivolving shared

            bool CanActivateConditionShared(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            int TamerTwoColourCount()
            {
                List<CardSource> tamerCards = new List<CardSource>();

                foreach (Permanent permanent in card.Owner.GetBattleAreaPermanents())
                {
                    if (permanent.IsTamer)
                    {
                        tamerCards.Add(permanent.TopCard);
                    }
                }
                return Mathf.FloorToInt(Combinations.GetDifferenetColorCardCount(tamerCards) / 2);
            }

            bool CanSelectProtectCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
            }
            #endregion

            #region On Play

            if(timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Make digimon immune", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] Until your opponent's turn ends, for every 2 colors your Tamers have, their Digimon's effects don't affect 1 of your Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> selectedPermanents = new List<Permanent>();
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectProtectCondition) && TamerTwoColourCount() > 0)
                    {
                        int maxCount = Math.Min(Math.Max(1, TamerTwoColourCount()), CardEffectCommons.MatchConditionPermanentCount(CanSelectProtectCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectProtectCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon for every 2 tamer colours you have that will not be affected by the effects of your opponent's Digimon.",
                            "The opponent is selecting 1 Digimon for every 2 tamer colours they have that will not be affected by the effects of your Digimon.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanents.Add(permanent);
                            yield return null;
                        }
                    }

                    if (selectedPermanents.Count() > 0)
                    {
                        foreach (Permanent selectedPermanent in selectedPermanents)
                        {
                            CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                            canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's Digimon's effects", CanUseConditionImmunity, card);
                            canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                            selectedPermanent.UntilOpponentTurnEndEffects.Add(GetCardEffect);

                            bool CanUseConditionImmunity(Hashtable hashtable)
                            {
                                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(selectedPermanent, card);
                            }

                            bool CardCondition(CardSource cardSource)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                {
                                    if (cardSource == selectedPermanent.TopCard)
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            bool SkillCondition(ICardEffect cardEffect)
                            {
                                if (CardEffectCommons.IsOpponentEffect(cardEffect, card))
                                {
                                    if (cardEffect.IsDigimonEffect)
                                    {
                                        return true;
                                    }

                                    if (cardEffect.IsDigimonEffect && cardEffect.IsSecurityEffect)
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            ICardEffect GetCardEffect(EffectTiming _timing)
                            {
                                if (_timing == EffectTiming.None)
                                {
                                    return canNotAffectedClass;
                                }

                                return null;
                            }
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving (immunity)

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Make digimon immune", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] Until your opponent's turn ends, for every 2 colors your Tamers have, their Digimon's effects don't affect 1 of your Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }


                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> selectedPermanents = new List<Permanent>();
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectProtectCondition) && TamerTwoColourCount() > 0)
                    {
                        int maxCount = Math.Min(Math.Max(1, TamerTwoColourCount()), CardEffectCommons.MatchConditionPermanentCount(CanSelectProtectCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectProtectCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon for every 2 tamer colours you have that will not be affected by the effects of your opponent's Digimon.",
                            "The opponent is selecting 1 Digimon for every 2 tamer colours they have that will not be affected by the effects of your Digimon.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanents.Add(permanent);
                            yield return null;
                        }
                    }

                    if (selectedPermanents.Count() > 0)
                    {
                        foreach (Permanent selectedPermanent in selectedPermanents)
                        {
                            CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                            canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's Digimon's effects", CanUseConditionImmunity, card);
                            canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                            selectedPermanent.UntilOpponentTurnEndEffects.Add(GetCardEffect);

                            bool CanUseConditionImmunity(Hashtable hashtable)
                            {
                                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(selectedPermanent, card);
                            }

                            bool CardCondition(CardSource cardSource)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                {
                                    if (cardSource == selectedPermanent.TopCard)
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            bool SkillCondition(ICardEffect cardEffect)
                            {
                                if (CardEffectCommons.IsOpponentEffect(cardEffect, card))
                                {
                                    if (cardEffect.IsDigimonEffect)
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            ICardEffect GetCardEffect(EffectTiming _timing)
                            {
                                if (_timing == EffectTiming.None)
                                {
                                    return canNotAffectedClass;
                                }

                                return null;
                            }
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving-When Attacking shared
            bool CanSelectTargetCondition(Permanent permanent)
            {
                return CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy);
            }

            bool CanActivateAtkDigivolveCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTargetCondition))
                    {
                        return true;
                    }
                }

                return false;
            }
            #endregion

            #region When Digivolving (removal)
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete lowest DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateAtkDigivolveCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] Delete 1 of your opponent's lowest DP Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTargetCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectTargetCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete lowest DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateAtkDigivolveCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] Delete 1 of your opponent's lowest DP Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTargetCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectTargetCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion
            return cardEffects;
        }
    }
}