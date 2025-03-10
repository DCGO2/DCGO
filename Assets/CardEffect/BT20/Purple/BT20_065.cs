using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DCGO.CardEffects.BT20
{
    public class BT20_065 : CEntity_Effect
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
                                    CardSource _topCard = selectedPermanent.TopCard;

                                    ActivateClass activateClass1 = new ActivateClass();
                                    activateClass1.SetUpICardEffect("Memory -1", CanUseCondition1, selectedPermanent.TopCard);
                                    activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                                    activateClass1.SetEffectSourcePermanent(selectedPermanent);
                                    CardEffectCommons.AddEffectToPermanent(targetPermanent: selectedPermanent, effectDuration: EffectDuration.UntilOpponentTurnEnd, card: card, cardEffect: activateClass1, timing: EffectTiming.OnDestroyedAnyone);

                                    if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                                    }

                                    string EffectDiscription1()
                                    {
                                        return "[On Deletion] Lose 1 memory.";
                                    }

                                    bool CanUseCondition1(Hashtable hashtable1)
                                    {
                                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                        {
                                            if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable1, (permanent) => permanent == selectedPermanent))
                                            {
                                                if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                                {
                                                    return true;
                                                }
                                            }
                                        }

                                        return false;
                                    }

                                    bool CanActivateCondition1(Hashtable hashtable1)
                                    {
                                        if (CardEffectCommons.IsTopCardInTrashOnDeletion(hashtable1))
                                        {
                                            return true;
                                        }

                                        return false;
                                    }

                                    IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(_topCard.Owner.AddMemory(-1, activateClass1));
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