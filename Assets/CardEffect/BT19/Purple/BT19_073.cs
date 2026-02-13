using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DCGO.CardEffects.BT19
{
    public class BT19_073 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("LordKnightmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Collision/Piercing
            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.CollisionSelfStaticEffect(false, card, null));
            }

            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<De-Digivolve 1> 1 of your opponent's Digimon for each of your Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] <De-Digivolve 1> 1 of your opponent's Digimon for each of your Digimon. Then, 1 of your opponent's Digimon can't digivolve until the end of their turn.";
                }

                bool IsOpponenetsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        return card.Owner.GetBattleAreaDigimons().Count > 0;

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int maxCount = card.Owner.GetBattleAreaDigimons().Count;

                    SelectPermanentEffect selectDeDigivolvePermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectDeDigivolvePermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOpponenetsDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: DeDigivolvePermanent,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectDeDigivolvePermanentEffect.Activate());

                    IEnumerator DeDigivolvePermanent(Permanent permanent)
                    {
                        for(int i = 0; i < maxCount; i++)
                            yield return ContinuousController.instance.StartCoroutine(new IDegeneration(permanent, 1, activateClass).Degeneration());
                    }

                    SelectPermanentEffect selectCannotEvoEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectCannotEvoEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOpponenetsDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectCannotEvoEffect.SetUpCustomMessage("Select 1 Digimon can not digivolve.", "The opponent is selecting 1 Digimon can not digivolve.");

                    yield return ContinuousController.instance.StartCoroutine(selectCannotEvoEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        Permanent selectedPermanent = permanent;

                        CanNotDigivolveClass canNotPutFieldClass = new CanNotDigivolveClass();
                        canNotPutFieldClass.SetUpICardEffect("Can't Digivolve", CanUseCondition1, card);
                        canNotPutFieldClass.SetUpCanNotEvolveClass(permanentCondition: PermanentCondition, cardCondition: CardCondition);
                        selectedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().DebuffSE);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            return true;
                        }

                        bool PermanentCondition(Permanent permanent)
                        {
                            if (permanent == selectedPermanent)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.IsDigimon || permanent.IsTamer)
                                    {
                                        if (!permanent.TopCard.CanNotBeAffected(canNotPutFieldClass))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        bool CardCondition(CardSource cardSource)
                        {

                            if (cardSource.Owner == card.Owner.Enemy)
                            {
                                if (cardSource.IsDigimon || cardSource.IsTamer)
                                {
                                    if (!cardSource.CanNotBeAffected(canNotPutFieldClass))
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;

                        }

                        ICardEffect GetCardEffect(EffectTiming _timing)
                        {
                            if (_timing == EffectTiming.None)
                            {
                                return canNotPutFieldClass;
                            }

                            return null;
                        }
                    }
                }
            }
            #endregion

            #region All Turns Shared

            string EffectName()
            {
                return "+3000 DP";
            }
            bool AllTurnPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    if (permanent.TopCard.HasText("Knightmon"))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool HasKnighmonOrXAntibody(CardSource source)
            {
                return source.EqualsCardName("X Antibody") || source.EqualsCardName("LordKnightmon");
            }

            bool CanUseAllTurnsCondition()
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                       card.PermanentOfThisCard().DigivolutionCards.Count(HasKnighmonOrXAntibody) > 0;
            }

            #endregion

            #region All Turns
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                    permanentCondition: AllTurnPermanentCondition,
                    changeValue: 3000,
                    isInheritedEffect: false,
                    card: card, 
                    condition: CanUseAllTurnsCondition,
                    effectName: EffectName));

                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("Your Digimons gain Alliance", CanUseCondition, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                cardEffects.Add(addSkillClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CanUseAllTurnsCondition();
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(cardSource))
                    {
                        if (cardSource == cardSource.PermanentOfThisCard().TopCard)
                        {
                            if (AllTurnPermanentCondition(cardSource.PermanentOfThisCard()))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                {
                    if (_timing == EffectTiming.OnAllyAttack)
                    {
                        bool Condition()
                        {
                            return CardSourceCondition(cardSource);
                        }

                        cardEffects.Add(CardEffectFactory.AllianceSelfEffect(false, cardSource, Condition));
                    }

                    return cardEffects;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}