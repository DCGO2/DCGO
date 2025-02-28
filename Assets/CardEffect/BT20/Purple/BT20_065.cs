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
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        return CardEffectCommons.CanTriggerOnPlay(hashtable, card);

                    return false;
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
                                    if (permanent != null)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(permanent));

                                        ActivateClass activateClass1 = new ActivateClass();
                                        activateClass1.SetUpICardEffect("Lose 1 Memory.", GivenEffectCanUseCondition,permanent.TopCard);
                                        activateClass1.SetUpActivateClass(GivenEffectCanActivateCondition,GivenEffectActivateCoroutine,-1,false, GivenEffectDescription());
                                        activateClass1.SetEffectSourcePermanent(permanent);
                                        permanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(permanent));
                                        }

                                        string GivenEffectDescription()
                                        {
                                            return "[On Deletion] Lose 1 Memory.";
                                        }

                                        bool GivenEffectCanUseCondition(Hashtable hashtable)
                                        {
                                            return CardEffectCommons.CanTriggerOnDeletion(hashtable, permanent.TopCard);
                                        }

                                        bool GivenEffectCanActivateCondition(Hashtable hashtable1)
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                                            {
                                                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                                {
                                                    return true;
                                                }
                                            }

                                            return false;
                                        }
                                        
                                        IEnumerator GivenEffectActivateCoroutine(Hashtable _hashtable)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(permanent.TopCard.Owner.AddMemory(-1, activateClass));

                                            
                                        }

                                        ICardEffect GetCardEffect(EffectTiming _timing)
                                        {
                                            if (_timing == EffectTiming.OnDestroyedAnyone)
                                            {
                                                return activateClass1;
                                            }

                                            return null;
                                        }

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