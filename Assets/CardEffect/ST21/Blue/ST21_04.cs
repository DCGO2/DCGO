using System.Collections;
using System.Collections.Generic;
using System.Linq;


//ST21 Zudomon
namespace DCGO.CardEffects.ST21
{
    public class ST21_04 : CEntity_Effect
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

            bool CanSelectPermanentBounceCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) && permanent.DigivolutionCards.Count() <= 1;
            }

            bool CanSelectPermanentStripCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash sources and bounce", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] From 1 of your opponent's Digimon, trash any 1 digivolution card for every 2 colors your Tamers have. Then, return 1 of their Digimon with 1 or fewer digivolution cards to the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return !cardSource.CanNotTrashFromDigivolutionCards(activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentStripCondition) && TamerTwoColourCount() > 0) 
                    { 
                    
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                            permanentCondition: CanSelectPermanentStripCondition,
                            cardCondition: CanSelectCardCondition,
                            maxCount: TamerTwoColourCount(),
                            canNoTrash: false,
                            isFromOnly1Permanent: true,
                            activateClass: activateClass
                        )); 
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentBounceCondition))
                    {

                        SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect1.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentBounceCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Bounce,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());
                    }
                }
            }
            #endregion

            #region When Digivolgin
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash sources and bounce", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] From 1 of your opponent's Digimon, trash any 1 digivolution card for every 2 colors your Tamers have. Then, return 1 of their Digimon with 1 or fewer digivolution cards to the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return !cardSource.CanNotTrashFromDigivolutionCards(activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentStripCondition) && TamerTwoColourCount() > 0) 
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                        permanentCondition: CanSelectPermanentStripCondition,
                        cardCondition: CanSelectCardCondition,
                        maxCount: TamerTwoColourCount(),
                        canNoTrash: false,
                        isFromOnly1Permanent: true,
                        activateClass: activateClass
                    ));
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentBounceCondition))
                    {

                        SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect1.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentBounceCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Bounce,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());
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
                activateClass.SetHashString("alliance-attack-ST21_04");
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