using System.Collections;
using System.Collections.Generic;

//ST21 Magnaangemon
namespace DCGO.CardEffects.ST21
{
    public class ST21_06 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("ADVENTURE") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region On Play/When Digivolving shared
            int TamerTwoColourCount()
            {
                List<CardSource> tamerCards = new List<CardSource>();

                foreach (Permanent permanent in card.Owner.GetBattleAreaPermanents())
                {
                    if (permanent.IsTamer && permanent.TopCard.EqualsTraits("ADVENTURE"))
                    {
                        tamerCards.Add(permanent.TopCard);
                    }
                }
                return Combinations.GetDifferenetColorCardCount(tamerCards) / 2;
            }
            #endregion

            #region On Play
            if(timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place into security", canUseCondition, card);
                activateClass.SetUpActivateClass(canActivateCondition, activateCoroutine, -1, false, effectDescription());

                string effectDescription()
                {
                    return "[On Play] Place 1 of your opponent's 6000 DP or lower Digimon as the top security card. For every 2 colors your Tamers have, add 2000 to this effect's DP maximum.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) 
                        && permanent.TopCard.Owner.CanAddSecurity(activateClass)
                        && permanent.HasDP && permanent.DP <= 6000 + 2000 * TamerTwoColourCount();
                }

                bool canActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                bool canUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator activateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        maxCount: 1,
                        canEndNotMax: false,
                        mode: SelectPermanentEffect.Mode.Custom,
                        selectPlayer: card.Owner,
                        cardEffect: null);

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 of the opponent's Digimon to place on top of security",
                        "The opponent is selecting 1 of your Digimon to place on top of Security.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        if (!permanent.IsToken)
                        {
                            Player selectedOwner = permanent.TopCard.Owner;
                            yield return ContinuousController.instance.StartCoroutine(new IPutSecurityPermanent(permanent, CardEffectCommons.CardEffectHashtable(activateClass), toTop: true).PutSecurity());
                        }

                        yield return null;
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place into security", canUseCondition, card);
                activateClass.SetUpActivateClass(canActivateCondition, activateCoroutine, -1, false, effectDescription());

                string effectDescription()
                {
                    return "[On Play] Place 1 of your opponent's 6000 DP or lower Digimon as the top security card. For every 2 colors your Tamers have, add 2000 to this effect's DP maximum.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.Owner.CanAddSecurity(activateClass)
                        && permanent.HasDP && permanent.DP <= 6000 + 2000 * TamerTwoColourCount();
                }

                bool canActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                bool canUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator activateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        maxCount: 1,
                        canEndNotMax: false,
                        mode: SelectPermanentEffect.Mode.Custom,
                        selectPlayer: card.Owner,
                        cardEffect: null);

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 of the opponent's Digimon to place on top of security",
                        "The opponent is selecting 1 of your Digimon to place on top of Security.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        if (!permanent.IsToken)
                        {
                            Player selectedOwner = permanent.TopCard.Owner;
                            yield return ContinuousController.instance.StartCoroutine(new IPutSecurityPermanent(permanent, CardEffectCommons.CardEffectHashtable(activateClass), toTop: true).PutSecurity());
                        }

                        yield return null;
                    }
                }
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain adventure then attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("alliance-attack-ST20-004");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn][Once Per Turn] When your other Digimon are played or digivolve, if any of them have the [ADVENTURE] trait, 1 of your Digimon gains <Alliance> for the turn. Then, 1 of your Digimon may attack.";
                }

                bool MyDigimonPlayedDigid(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) && permanent != card.PermanentOfThisCard())
                    {
                        return permanent.TopCard.EqualsTraits("ADVENTURE");
                    }
                    return false;
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
                            canNoSelect: false,
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