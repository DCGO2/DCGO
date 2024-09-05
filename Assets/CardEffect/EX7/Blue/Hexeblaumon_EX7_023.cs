using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX7
{
    public class Hexeblaumon_EX7_023 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
                cardEffects.Add(CardEffectFactory.IcecladSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            
            #region When Digivolving
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 4 then return Tamer to bottom of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Trash any 4 digivolution cards of your opponent's Digimon. Then, if your opponent has no Digimon with digivolution cards, return 1 of your opponent's Tamers to the bottom of the deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                    }

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
                

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.Owner.Enemy && cardSource.IsDigimon)
                    {
                        return true;
                    }

                    return false;
                }
                
                bool CanBounceTamerCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        int count = CardEffectCommons.MatchConditionOpponentsPermanentCount(card, (permanent) => permanent.IsDigimon && permanent.HasNoDigivolutionCards);

                        if (count >= 1)
                            return false;
                    }
                    return true;
                }

                bool CanSelectTamerCondition(Permanent permanent)
                {
                    return permanent.IsTamer;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                            permanentCondition: CanSelectPermanentCondition,
                            cardCondition: CanSelectCardCondition,
                            maxCount: 4,
                            canNoTrash: false,
                            isFromOnly1Permanent: false,
                            activateClass: activateClass
                        ));
                    }

                    if (CanBounceTamerCondition(hashtable))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTamerCondition))
                        {
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectTamerCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectTamerCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }
                }
            }
            #endregion 
            
            #region Opponent's Turn Effect

            if (timing == EffectTiming.None)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent's digimon can't suspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Opponent's Turn] None of your opponent's Digimon with as many or fewer digivolution cards as this Digimon can suspend.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return CardEffectCommons.IsOpponentTurn(card);
                    }

                    return false;
                }
                
                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        return true;

                    return false;
                }
                
                bool CanSelectPermanentToNoSuspend(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        if (permanent.IsDigimon && permanent.DigivolutionCards.Count <= card.PermanentOfThisCard().DigivolutionCards.Count)
                            return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentToNoSuspend) >= 1)
                    {
                        foreach (Permanent affected in card.Owner.Enemy.GetBattleAreaPermanents())
                        {
                            if (!affected.IsDigimon)
                                continue;

                            if (affected.TopCard.CanNotBeAffected(activateClass))
                                continue;

                            if (affected.DigivolutionCards.Count > card.PermanentOfThisCard().DigivolutionCards.Count)
                            {
                                StartCoroutine(SelectPermanentCoroutine(affected));
                            }
                                
                        }
                        
                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                            canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCondition1, card);
                            canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCondition);
                            permanent.UntilOwnerTurnEndEffects.Add((_timing) => canNotSuspendClass);

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(permanent));

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                return true;
                            }

                            bool PermanentCondition(Permanent permanent)
                            {
                                return true;
                            }
                        }
                    }

                    yield return null;
                }
            }

            #endregion

            return cardEffects;
        }
    }
}