using System.Collections;
using System.Collections.Generic;

// Seventh Fascination
namespace DCGO.CardEffects.EX7
{
    public class EX7_072 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Trash Your Turn
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return this to bottom of deck, Activate Main", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Trash] [Your Turn] When your Digimon digivolves into [Lilithmon (X Antibody)], by returning this card to the bottom of the deck, activate this card's [Main] effect.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && (permanent.TopCard.CardNames.Contains("Lilithmon (X Antibody)")
                            || permanent.TopCard.CardNames.Contains("Lilithmon(XAntibody)"));
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrashTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrashActivate(card, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> cardSources = new List<CardSource>() { card };

                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(cardSources, cardEffect: activateClass));

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(cardSources, "Deck Bottom Card", true, true));

                    ActivateClass mainActivateClass = CardEffectCommons.OptionMainEffect(card);

                    if (mainActivateClass != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(mainActivateClass.Activate(CardEffectCommons.OptionMainCheckHashtable(card)));
                    }
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("All Opponents Digimon gain \"Delete 1 of your Digimon\"", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] All your opponent's Digimon gain \" [End of Your Turn] Delete 1 of your Digimon.\" until the end of their turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool PermanentCondition(Permanent permanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                            && !permanent.TopCard.CanNotBeAffected(activateClass);
                    }

                    AddSkillClass addSkillClass = new AddSkillClass();
                    addSkillClass.SetUpICardEffect("Your opponent's Digimon gain [End of your Turn] Delete 1 of your Digimon", CanUseCondition, card);
                    addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                    card.Owner.UntilOpponentTurnEndEffects.Add((_timing) => addSkillClass);

                    bool CanUseCondition(Hashtable hashtable)
                    {
                        return true;
                    }

                    bool CardSourceCondition(CardSource cardSource)
                    {
                        return PermanentCondition(cardSource.PermanentOfThisCard())
                            && cardSource == cardSource.PermanentOfThisCard().TopCard;
                    }

                    List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                    {
                        if (_timing == EffectTiming.OnEndTurn)
                        {
                            ActivateClass activateClass1 = new ActivateClass();
                            activateClass1.SetUpICardEffect("Delete 1 of your Digimon", CanUseCondition1, cardSource);
                            activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDescription1());
                            cardEffects.Add(activateClass1);

                            string EffectDescription1()
                            {
                                return "[End of Your Turn] Delete 1 of your Digimon.";
                            }

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                return CardEffectCommons.IsExistOnBattleAreaTrigger(cardSource, activateClass1)
                                    && CardEffectCommons.IsOwnerTurn(cardSource)
                                    && !cardSource.CanNotBeAffected(activateClass);
                            }

                            bool CanActivateCondition1(Hashtable hashtable)
                            {
                                return CardEffectCommons.IsExistOnBattleAreaActivate(cardSource, activateClass1)
                                    && !cardSource.CanNotBeAffected(activateClass);
                            }

                            bool CanSelectPermanentCondition(Permanent permanent)
                            {
                                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, cardSource);
                            }

                            IEnumerator ActivateCoroutine1(Hashtable hashtable)
                            {
                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: cardSource.Owner,
                                    canTargetCondition: CanSelectPermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Destroy,
                                    cardEffect: activateClass1);

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                            }
                        }

                        if (_timing == EffectTiming.None)
                        {
                            cardEffects.Add(PermanentEffectFactory.AddDetailClass(cardSource.PermanentOfThisCard(), "[End of Your Turn] Delete 1 of your Digimon.", true, activateClass));
                        }

                        return cardEffects;
                    }

                    foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaDigimons())
                    {
                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(permanent));
                        }
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 Opponents unsuspended Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] Delete 1 of your opponent's unsuspended Digimon.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && !permanent.IsSuspended;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
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
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
