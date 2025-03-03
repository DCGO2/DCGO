using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT20
{
    public class WormmonBT20_065 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Give effects to your opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By trashing 1 card in your hand, give 1 of your opponent's Digimon '[On Deletion] Lose 1 memory.' until the end of their turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
             }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Count >= 1){
                        bool discarded = false;

                        int discardCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: cardSource => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: discardCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return StartCoroutine(selectHandEffect.Activate());
                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count == 1)
                            {
                                discarded = true;

                                yield return null;
                            }
                        }
                        if (discarded)
                        {
                            Permanent selectedPermanent = null;

                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                int maxCount = Math.Min(1,
                                    CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                                SelectPermanentEffect selectPermanentEffect =
                                    GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage(
                                    "Select 1 Digimon that will gain effect.",
                                    "The opponent is selecting 1 Digimon that will gain effect.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    selectedPermanent = permanent;
                                    yield return null;
                                }

                                if(selectedPermanent != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));

                                    bool PermanentCondition(Permanent permanent)
                                    {
                                        return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                                    }

                                    AddSkillClass addSkillClass = new AddSkillClass();
                                    addSkillClass.SetUpICardEffect("Memory -1", CanUseCondition1, card);
                                    addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                                    selectedPermanent.UntilOpponentTurnEndEffects.Add((_timing) => addSkillClass);

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
                                        if (_timing == EffectTiming.OnDestroyedAnyone)
                                        {
                                            ActivateClass activateClass1 = new ActivateClass();
                                            activateClass1.SetUpICardEffect("Memory -1", CanUseCondition2, cardSource);
                                            activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                                            cardEffects.Add(activateClass1);

                                            if (cardSource.PermanentOfThisCard() != null)
                                            {
                                                activateClass1.SetEffectSourcePermanent(cardSource.PermanentOfThisCard());
                                            }

                                            string EffectDiscription1()
                                            {
                                                return "[On Deletion] Lose 1 memory.";
                                            }

                                            bool CanUseCondition2(Hashtable hashtable)
                                            {
                                                if (CardSourceCondition(cardSource))
                                                {
                                                    if (CardEffectCommons.CanTriggerOnDeletion(hashtable, cardSource))
                                                    {
                                                        return true;
                                                    }
                                                }

                                                return false;
                                            }

                                            bool CanActivateCondition1(Hashtable hashtable)
                                            {
                                                if (CardEffectCommons.CanActivateOnDeletion(cardSource))
                                                {
                                                    return true;
                                                }

                                                return false;
                                            }

                                            IEnumerator ActivateCoroutine1(Hashtable _hashtable)
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(cardSource.Owner.AddMemory(-1, activateClass1));
                                            }
                                        }

                                        return cardEffects;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.RetaliationSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }

            return cardEffects;
        }
    }
}