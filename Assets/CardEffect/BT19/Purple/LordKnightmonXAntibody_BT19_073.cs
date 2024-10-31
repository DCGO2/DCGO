using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT19
{
    public class LordKnightmonXAntibody_BT19_073 : CEntity_Effect
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

                    for (int i = 0; i < maxCount; i++)
                    {
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
                    }

                    IEnumerator DeDigivolvePermanent(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDegeneration(permanent, 1, activateClass).Degeneration());
                    }

                    SelectPermanentEffect selectCannotEvoEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectCannotEvoEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOpponenetsDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
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

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantUnsuspendUntilOpponentTurnEnd(
                                    targetPermanent: selectedPermanent,
                                    activateClass: activateClass
                                ));

                    }
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.None)
            {
                string EffectName()
                {
                    return "+3000 DP";
                }
                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                    {
                        if (permanent.IsDigimon)
                        {
                            if (permanent.TopCard.HasText("Knightmon"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool HasKnighmonOrXAntibody(CardSource source)
                {
                    return source.EqualsCardName("X Antibody") || source.EqualsCardName("Knightmon");
                }

                bool CanUseCondition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.PermanentOfThisCard().DigivolutionCards.Count(HasKnighmonOrXAntibody) > 0;
                }

                cardEffects.Add(CardEffectFactory.AllianceStaticEffect(
                    permanentCondition: PermanentCondition,
                    isInheritedEffect: false,
                    card: card, condition: CanUseCondition));

                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                    permanentCondition: PermanentCondition,
                    changeValue: 3000,
                    isInheritedEffect: false,
                    card: card, 
                    condition: CanUseCondition,
                    effectName: EffectName));
            }
            #endregion

            return cardEffects;
        }
    }
}