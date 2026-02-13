using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Trash] [Your Turn] When your Digimon digivolves into [Lilithmon (X Antibody)], by returning this card to the bottom of the deck, activate this card's [Main] effect.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.CardNames.Contains("Lilithmon (X Antibody)"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardNames.Contains("Lilithmon(XAntibody)"))
                        {
                            return true;
                        }
                    }

                    return false;
                }              

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnTrash(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnTrash(card))
                    {
                        List<CardSource> cardSources = new List<CardSource>() { card };

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(cardSources));

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(cardSources, "Deck Bottom Card", true, true));

                        ActivateClass mainActivateClass = CardEffectCommons.OptionMainEffect(card);

                        if (mainActivateClass != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(mainActivateClass.Activate(CardEffectCommons.OptionMainCheckHashtable(card)));
                        }
                    }
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("All Opponents Digimon gain \"Delete 1 of your Digimon\"", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] All your opponent's Digimon gain \" [End of Your Turn] Delete 1 of your Digimon.\" until the end of their turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool PermanentCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                        {
                            if (!permanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaPermanents())
                    {
                        if (PermanentCondition(permanent))
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(permanent));
                        }
                    }

                    AddSkillClass addSkillClass = new AddSkillClass();
                    addSkillClass.SetUpICardEffect("Delete 1 of your Digimon", CanUseCondition1, card);
                    addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);

                    card.Owner.UntilOpponentTurnEndEffects.Add((_timing) => addSkillClass);

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return true;
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
                        if (_timing == EffectTiming.OnEndTurn)
                        {
                            ActivateClass activateClass1 = new ActivateClass();
                            activateClass1.SetUpICardEffect("Delete 1 of your Digimon", CanUseCondition2, cardSource);
                            activateClass1.SetUpActivateClass(CanActivateCondition2, ActivateCoroutine1, -1, false, EffectDiscription1());
                            activateClass1.SetEffectSourceCard(cardSource);
                            cardEffects.Add(activateClass1);

                            if (cardSource.PermanentOfThisCard() != null)
                            {
                                activateClass1.SetEffectSourcePermanent(cardSource.PermanentOfThisCard());
                            }

                            string EffectDiscription1()
                            {
                                return "[End of Your Turn] Delete 1 of your Digimon.";
                            }

                            bool CanSelectPermanentCondition(Permanent permanent)
                            {
                                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, cardSource);
                            }

                            bool CanUseCondition2(Hashtable hashtable)
                            {
                                if (CardEffectCommons.IsOwnerTurn(cardSource))
                                {
                                    if(CardEffectCommons.IsExistOnBattleAreaDigimon(cardSource))
                                        return CardSourceCondition(cardSource);                                    
                                }

                                return false;
                            }

                            bool CanActivateCondition2(Hashtable hashtable)
                            {
                                return CardEffectCommons.IsExistOnBattleAreaDigimon(cardSource);
                            }

                            IEnumerator ActivateCoroutine1(Hashtable _hashtable)
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                                {
                                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: cardSource.Owner,
                                        canTargetCondition: CanSelectPermanentCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
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

                        return cardEffects;
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 Opponents unsuspended Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] Delete 1 of your opponent's unsuspended Digimon.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
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

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
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