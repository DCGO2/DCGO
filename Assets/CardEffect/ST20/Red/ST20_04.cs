using System.Collections;
using System.Collections.Generic;
using System.Linq;

//ST20-04 garudamon
namespace DCGO.CardEffects.ST20
{
    public class ST20_04 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasAdventureTraits && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
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
                return Combinations.GetDifferenetColorCardCount(tamerCards) / 2;
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
            }
            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("For the turn, 1 of your Digimon gains <Security A. +1>, and for every 2 colors your Tamers have, gets +2000 DP. (This Digimon checks 1 additional security card.)", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] For the turn, 1 of your Digimon gains <Security A. +1>, and for every 2 colors your Tamers have, gets +2000 DP. (This Digimon checks 1 additional security card.)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage(
                            "Select 1 Digimon that will gain <Security A. +1>, and +2000DP per 2 colours of tamers you have.",
                            "The opponent is selecting 1 Digimon that will gain <Security A. +1>, and +2000DP per 2 colours of tamers they have.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(
                                targetPermanent: permanent,
                                changeValue: 1,
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass));
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: permanent,
                                changeValue: 2000 * TamerTwoColourCount(),
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass));
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("For the turn, 1 of your Digimon gains<Security A. +1>, and for every 2 colors your Tamers have, gets +2000 DP. (This Digimon checks 1 additional security card.)", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving]For the turn, 1 of your Digimon gains <Security A. +1>, and for every 2 colors your Tamers have, gets +2000 DP. (This Digimon checks 1 additional security card.)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage(
                            "Select 1 Digimon that will gain <Security A. +1>, and +2000DP per 2 colours of tamers you have.",
                            "The opponent is selecting 1 Digimon that will gain <Security A. +1>, and +2000DP per 2 colours of tamers they have.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(
                                targetPermanent: permanent,
                                changeValue: 1,
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass));
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: permanent,
                                changeValue: 2000 * TamerTwoColourCount(),
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass));
                        }
                    }
                }
            }

            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain alliance then attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("alliance-attack-ST20_04");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn][Once Per Turn] When your other Digimon are played or digivolve, if any of them have the [ADVENTURE] trait, 1 of your Digimon gains <Alliance> for the turn. Then, 1 of your Digimon may attack.";
                }

                bool MyDigimonAdventurePlayedDigid(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent != card.PermanentOfThisCard() &&
                           permanent.TopCard.EqualsTraits("ADVENTURE");
                }

                bool MyDigimonPlayedDigid(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent != card.PermanentOfThisCard();
                }

                bool MyDigimonAlliance(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool MyDigimonAttacking(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.CanAttack(activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, MyDigimonPlayedDigid))
                                return true;

                            if (CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, MyDigimonPlayedDigid))
                                return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> etbPermanents = new List<Permanent>();
                    List<Hashtable> hashtables = CardEffectCommons.GetHashtablesFromHashtable(hashtable);

                    if (hashtables != null)
                    {
                        foreach (Hashtable hashtable1 in hashtables)
                        {
                            Permanent permanent = CardEffectCommons.GetPermanentFromHashtable(hashtable1);

                            if (permanent != null)
                                etbPermanents.Add(permanent);
                        }

                        etbPermanents = etbPermanents.Filter(MyDigimonAdventurePlayedDigid);
                    }

                    if (etbPermanents.Count > 0)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(MyDigimonAlliance))
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: MyDigimonAlliance,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectBoostedDigimon,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to gain <Alliance>", "The opponent is selecting 1 Digimon to gain <Alliance>");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectBoostedDigimon(Permanent target)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainAlliance(targetPermanent: target, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass)); ;
                            }
                        }
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(MyDigimonAttacking))
                    {
                        Permanent selectedPermanent = null;
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: MyDigimonAttacking,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to attack.", "The opponent is selecting 1 Digimon to attack.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                            selectAttackEffect.SetUp(
                                attacker: selectedPermanent,
                                canAttackPlayerCondition: () => true,
                                defenderCondition: _ => true,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                        }
                    }
                }
            }
            #endregion

            #region Alliance Inherit

            if (timing == EffectTiming.OnAllyAttack)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: true, card: card, condition: Condition));
            }

            #endregion

            return cardEffects;
        }
    }
}