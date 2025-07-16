using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// MetalTyrannomon
namespace DCGO.CardEffects.BT22
{
    public class BT22_062 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel5
                        && targetPermanent.TopCard.ContainsCardName("Tyrannomon")
                        && !targetPermanent.TopCard.HasXAntibodyTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 1,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region Collision

            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.CollisionSelfStaticEffect(false, card, null));
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain 4K DP, then if [MetalTyrannomon]/[X Antibody] in sources, 1 digimon cant digivolve", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] This Digimon gets +4000 DP until your opponent's turn ends. Then, if [MetalTyrannomon] or [X Antibody] is in this Digimon's digivolution cards, 1 of your opponent's Digimon can't digivolve until their turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool OpponentDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.ChangeDigimonDP(card.PermanentOfThisCard(), 4000, EffectDuration.UntilOpponentTurnEnd, activateClass));

                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.EqualsCardName("MetalTyrannomon") || cardSource.EqualsCardName("X Antibody")) >= 1)
                    {
                        Permanent selectedPermanent = null;

                        #region Select Permanent

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, OpponentDigimon));
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 opponent's Digimon that can't digivolve", "The opponent is selecting 1 Digimon that can't digivolve");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        #endregion

                        if (selectedPermanent != null)
                        {
                            #region Setup CanNotDigivolveClass

                            CanNotDigivolveClass canNotPutFieldClass = new CanNotDigivolveClass();
                            canNotPutFieldClass.SetUpICardEffect("Can't Digivolve", _ => true, card);
                            canNotPutFieldClass.SetUpCanNotEvolveClass(permanentCondition: PermanentCondition, cardCondition: CardCondition);

                            bool PermanentCondition(Permanent permanent)
                            {
                                if (permanent == selectedPermanent)
                                {
                                    if (permanent.TopCard != null)
                                    {
                                        if (permanent.IsDigimon)
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
                                    if (cardSource.IsDigimon)
                                    {
                                        if (!cardSource.CanNotBeAffected(canNotPutFieldClass))
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
                            }

                            ICardEffect CanNotDigivolve(EffectTiming _timing)
                            {
                                if (_timing == EffectTiming.None)
                                {
                                    return canNotPutFieldClass;
                                }

                                return null;
                            }

                            #endregion

                            selectedPermanent.UntilOwnerTurnEndEffects.Add(CanNotDigivolve);
                            ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().DebuffSE);
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                        }
                    }
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 of your opponent's digimon attacks", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT22_062_EndOfOpponentTurn");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Opponent's Turn] [Once Per Turn] You may choose 1 of your opponent's Digimon. Your opponent attacks with the chosen Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOpponentTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionOpponentsPermanent(card, OpponentDigimon);
                }

                bool OpponentDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.CanAttack(activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, OpponentDigimon))
                    {
                        Permanent selectedPermanent = null;

                        #region Select Permanent

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, OpponentDigimon));
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }
                        selectPermanentEffect.SetUpCustomMessage("Select 1 opponent's Digimon that will attack", "The opponent is selecting 1 Digimon that will attack");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        #endregion

                        if (selectedPermanent != null)
                        {
                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();
                            selectAttackEffect.SetUp(
                                attacker: selectedPermanent,
                                canAttackPlayerCondition: () => true,
                                defenderCondition: (permanent) => true,
                                cardEffect: activateClass);
                            selectAttackEffect.SetCanNotSelectNotAttack();
                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}