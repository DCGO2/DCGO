using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT20
{
    public class BT20_059 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Gankoomon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<De-Digivolve 2>, then your Digimon or unaffected by Digimon effects", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] <De-Digivolve 2> 1 of your opponent's Digimon. Then, if [Gankoomon] or [X Antibody] is in this Digimon's digivolution cards, until the end of your opponent's turn, none of your Digimon are affected by your opponent's Digimon effects.";
                }

                bool CanSelectDeDigivolveTarget(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool HasGankooOrXAnti()
                {
                    return card.PermanentOfThisCard().DigivolutionCards.Count(source => source.EqualsCardName("Gankoomon") || source.EqualsCardName("X Antibody")) > 0;
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
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDeDigivolveTarget))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDeDigivolveTarget,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select Digimons to De-Digivolve.", "The opponent is selecting Digimons to De-Digivolve.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            if (permanent != null)
                                yield return ContinuousController.instance.StartCoroutine(new IDegeneration(permanent, 2, activateClass).Degeneration());

                            yield return null;
                        }
                    }

                    if (HasGankooOrXAnti())
                    {
                        CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                        canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's Digimon's effects", CanUseConditionImmunity, card);
                        canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                        card.Owner.UntilOpponentTurnEndEffects.Add(GetCardEffect);

                        bool CanUseConditionImmunity(Hashtable hashtable)
                        {
                            return true;
                        }

                        bool CardCondition(CardSource cardSource)
                        {
                            if (CardEffectCommons.IsExistOnBattleAreaDigimon(cardSource))
                            {
                                if (card.Owner == cardSource.Owner)
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
            #endregion

            #region Opponents Turn
            if (timing == EffectTiming.None)
            {
                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("[Opponent's Turn] All of your Digimon with [Sistermon] or [Huckmon] in their names or the [Royal Knight] trait gain <Reboot> and <Blocker>.", CanUseCondition, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                cardEffects.Add(addSkillClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
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
                        if (permanent.TopCard.ContainsCardName("Sistermon") || permanent.TopCard.ContainsCardName("Huckmon"))
                            return true;

                        if (permanent.TopCard.EqualsTraits("Royal Knight"))
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
                    if (_timing == EffectTiming.None)
                    {
                        bool Condition()
                        {
                            return CardSourceCondition(cardSource);
                        }

                        cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: Condition));
                        cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: Condition));
                    }

                    return cardEffects;
                }
            }
            #endregion

            #region Opponents Turn - ESS
            if (timing == EffectTiming.None)
            {
                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("[Opponent's Turn] While this Digimon is [Jesmon GX], all of your Digimon gain <Reboot> and <Blocker>.", CanUseCondition, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                addSkillClass.SetIsInheritedEffect(true);
                cardEffects.Add(addSkillClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        { 
                            return card.PermanentOfThisCard().TopCard.EqualsCardName("Jesmon GX");
                        }
                    }

                    return false;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(cardSource.PermanentOfThisCard(), card);
                }

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                {
                    if (_timing == EffectTiming.None)
                    {
                        bool Condition()
                        {
                            return true;
                        }

                        cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: Condition));
                        cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: Condition));
                    }

                    return cardEffects;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}