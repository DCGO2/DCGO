using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Shinegreymon: Burst Mode // Final Shining Burst
namespace DCGO.CardEffects.BT25
{
    public class BT25_104 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Effects

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.EqualsTraits("DATA SQUAD");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 5, true, card, null, level: 6));
            }
            #endregion

            #region Burst Digivolution
            if (timing == EffectTiming.None)
            {
                AddBurstDigivolutionConditionClass addBurstDigivolutionConditionClass = new AddBurstDigivolutionConditionClass();
                addBurstDigivolutionConditionClass.SetUpICardEffect($"Burst Digivolution", CanUseCondition, card);
                addBurstDigivolutionConditionClass.SetUpAddBurstDigivolutionConditionClass(getBurstDigivolutionCondition: GetBurstDigivolution);
                addBurstDigivolutionConditionClass.SetNotShowUI(true);
                cardEffects.Add(addBurstDigivolutionConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                BurstDigivolutionCondition GetBurstDigivolution(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        bool tamerCondition(Permanent permanent)
                        {
                            return permanent != null
                                && permanent.TopCard != null
                                && permanent.TopCard.Owner == card.Owner
                                && permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent)
                                && !permanent.CannotReturnToHand(null)
                                && permanent.TopCard.EqualsCardName("Marcus Damon");
                        }

                        bool digimonCondition(Permanent permanent)
                        {
                            return permanent != null
                                && permanent.TopCard != null
                                && permanent.TopCard.Owner == card.Owner
                                && permanent.TopCard.Owner.GetFieldPermanents().Contains(permanent)
                                && !card.CanNotEvolve(permanent)
                                && permanent.TopCard.EqualsCardName("ShineGreymon");
                        }

                        BurstDigivolutionCondition burstDigivolutionCondition = new BurstDigivolutionCondition(
                            tamerCondition: tamerCondition,
                            selectTamerMessage: "1 [Marcus Damon]",
                            digimonCondition: digimonCondition,
                            selectDigimonMessage: "1 [ShineGreymon]",
                            cost: 0);

                        return burstDigivolutionCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Raid
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Security Attack +1
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Barrier
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(false, card, null));
            }
            #endregion

            #region Shared WD / WA

            string SharedHashString = "BT25_104_WD_WA";
            
            string SharedEffectName = "1 Enemy Digimon gets -15k DP, play 1 tamer.";

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] Activate 1 [Main] effect on this card's Option side.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ActivateMainOfOptionSide(card, activateClass));
            }

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    maxCountPerTurn: 1,
                    hashValue: SharedHashString,
                    whenDigivolving: true,
                    whenAttacking: true);

            #endregion

            #region Your Turn
            if (timing == EffectTiming.None)
            {
                //All of your [Marcus Damon]s
                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && permanent.TopCard.EqualsCardName("Marcus Damon");
                }

                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                #region are also treated as Digimon
                TreatAsDigimonClass treatAsDigimonClass = CardEffectFactory.TreatAsDigimonStaticEffect(
                    permanentCondition: PermanentCondition, 
                    isInheritedEffect: false, 
                    card: card, 
                    condition: Condition);

                cardEffects.Add(treatAsDigimonClass);
                #endregion

                #region with 12000 DP
                ChangeBaseDPClass changeBaseDPClass = CardEffectFactory.ChangeBaseDPGlobalEffect(
                    permanentCondition: PermanentCondition, 
                    changeValue: 12000, 
                    isInheritedEffect: false, 
                    card: card, 
                    condition: Condition);

                cardEffects.Add(changeBaseDPClass);
                #endregion

                #region and gain Rush
                RushClass rushClass = CardEffectFactory.RushStaticEffect(
                    permanentCondition: PermanentCondition, 
                    isInheritedEffect: false, 
                    card: card, 
                    condition: Condition);

                cardEffects.Add(rushClass);
                #endregion
            }
            #endregion

            #endregion
            
            #region Option Effects

            #region Ignore Colour Requirement
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("DATA SQUAD");
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[Main] 1 of your opponent's Digimon gets -15000 DP for the turn. Then, you may play 1 Tamer card from your hand without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectDPMinusPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool CanSelectTamerCondition(CardSource cardSource)
                {
                    return cardSource.IsTamer
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectDPMinusPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDPMinusPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -15000.", "The opponent is selecting 1 Digimon that will get DP -15000.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: -15000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                        }
                    }

                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectTamerCondition))
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectTamerCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.PlayForFree,
                                cardEffect: activateClass);

                            yield return StartCoroutine(selectHandEffect.Activate());
                    }
                }
            }
            #endregion

            #region Arts Digivolution
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }
            #endregion

            #endregion

            return cardEffects;
        }
    }
}
