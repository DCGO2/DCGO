using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT20
{
    public class BT20_019 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {

                    return targetPermanent.TopCard.EqualsCardName("Jesmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Alliance

            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Become unaffected by opponents effects, then can attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If [Jesmon] or [X Antibody] is in this Digimon's digivolution cards, for the turn, 1 of your Digimon isn't affected by your opponent's effects. Then, 1 of your Digimon may attack.";
                }

                bool SourceCondition(CardSource source)
                {
                    return source.EqualsCardName("Jesmon") || source.EqualsCardName("X Antibody");
                }

                bool CanSelectCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent this_card = card.PermanentOfThisCard();

                    if (this_card != null)
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count(SourceCondition) >= 1)
                        {
                            #region select immunity recipient
                            {
                                Permanent immunityPermanent = null;

                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectCondition));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will be immune from your opponent's effects.", "Opponent is selecting 1 Digimon that will be immune from your effects.");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    immunityPermanent = permanent;
                                    yield return null;
                                }

                                if(immunityPermanent != null)
                                {
                                    CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                                    canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's effects.", CanUseConditionImmunity, card);
                                    canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                                    immunityPermanent.UntilEachTurnEndEffects.Add((_timing) => canNotAffectedClass);

                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(immunityPermanent));

                                    bool CanUseConditionImmunity(Hashtable hashtable)
                                    {
                                        return true;
                                    }

                                    bool CardCondition(CardSource cardSource)
                                    {
                                        if (CardEffectCommons.IsExistOnBattleAreaDigimon(cardSource))
                                        {
                                            if (cardSource == immunityPermanent.TopCard)
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
                                            return true;
                                        }

                                        return false;
                                    }
                                }
                            }
                            #endregion
                        }

                        #region Select attacker
                        {
                            bool CanSelectAttackPermanentCondition(Permanent permanent)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                                {
                                    if (permanent.CanAttack(activateClass))
                                    {
                                        return true;
                                    }
                                }
                                return false;
                            }

                            Permanent selectedAttacker = null;

                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectAttackPermanentCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectAttackPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will attack.", "Opponent is selecting 1 Digimon that will attack.");
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedAttacker = permanent;

                                yield return null;
                            }

                            if (selectedAttacker != null)
                            {
                                SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                selectAttackEffect.SetUp(
                                    attacker: selectedAttacker,
                                    canAttackPlayerCondition: () => true,
                                    defenderCondition: (permanent) => true,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                            }
                        }
                        #endregion
                    }
                }
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.None)
            {
                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("Your Digimon with [Sistermon] in their names or the [Royal Knight] trait gain Pierce", CanUseCondition, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                cardEffects.Add(addSkillClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.ContainsCardName("Sistermon") || permanent.TopCard.HasRoyalKnightTraits)
                            return true;
                    }

                    return false;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    if (PermanentCondition(cardSource.PermanentOfThisCard()))
                    {
                        if (cardSource == cardSource.PermanentOfThisCard().TopCard)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                {
                    if (_timing == EffectTiming.OnDetermineDoSecurityCheck)
                    {
                        bool Condition()
                        {
                            return CardSourceCondition(cardSource);
                        }

                        cardEffects.Add(CardEffectFactory.PierceSelfEffect(
                            isInheritedEffect: false,
                            card: cardSource,
                            condition: Condition));
                    }

                    return cardEffects;
                }
            }

            if (timing == EffectTiming.None)
            {

                CanAttackTargetDefendingPermanentClass canAttackTargetDefendingPermanentClass = new CanAttackTargetDefendingPermanentClass();
                canAttackTargetDefendingPermanentClass.SetUpICardEffect($"Can attack to unsuspended Digimon", CanUseCondition, card);
                canAttackTargetDefendingPermanentClass.SetUpCanAttackTargetDefendingPermanentClass(attackerCondition: PermanentCondition, defenderCondition: DefenderCondition, cardEffectCondition: CardEffectCondition);
                cardEffects.Add(canAttackTargetDefendingPermanentClass);

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.ContainsCardName("Sistermon") || permanent.TopCard.HasRoyalKnightTraits)
                            return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool DefenderCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (!permanent.IsSuspended)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CardEffectCondition(ICardEffect cardEffect)
                {
                    return true;
                }
            }
            #endregion

            #region Your Turn - ESS
            if (timing == EffectTiming.None)
            {
                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("[Your Turn] While this Digimon is [Jesmon GX], all of your Digimon gain <Piercing> and can also attack your opponent's unsuspended Digimon.", CanUseCondition, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                addSkillClass.SetIsInheritedEffect(true);
                cardEffects.Add(addSkillClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return card.PermanentOfThisCard().TopCard.EqualsCardName("Jesmon GX") &&
                           CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(cardSource) &&
                           CardEffectCommons.IsOwnerPermanent(cardSource.PermanentOfThisCard(), card);
                }

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                {
                    if (_timing == EffectTiming.OnDetermineDoSecurityCheck)
                    {
                        bool Condition()
                        {
                            return card.PermanentOfThisCard().TopCard.EqualsCardName("Jesmon GX") &&
                                   CardSourceCondition(cardSource);
                        }

                        cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: Condition));
                    }

                    return cardEffects;
                }

                CanAttackTargetDefendingPermanentClass canAttackTargetDefendingPermanentClass = new CanAttackTargetDefendingPermanentClass();
                canAttackTargetDefendingPermanentClass.SetUpICardEffect($"Can attack to unsuspended Digimon", CanUseCondition1, card);
                canAttackTargetDefendingPermanentClass.SetUpCanAttackTargetDefendingPermanentClass(attackerCondition: PermanentCondition, defenderCondition: DefenderCondition, cardEffectCondition: CardEffectCondition);
                canAttackTargetDefendingPermanentClass.SetIsInheritedEffect(true);
                cardEffects.Add(canAttackTargetDefendingPermanentClass);

                bool CanUseCondition1(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool DefenderCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (!permanent.IsSuspended)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CardEffectCondition(ICardEffect cardEffect)
                {
                    return true;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}