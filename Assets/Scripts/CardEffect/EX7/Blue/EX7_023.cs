using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX7
{
    public class EX7_023 : CEntity_Effect
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
                    return true;
                }

                bool CanSelectTamerCondition(Permanent permanent)
                {
                    if(!CardEffectCommons.IsOwnerPermanent(permanent, card))
                        return permanent.IsTamer;

                    return false;
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

                    if (!CardEffectCommons.HasMatchConditionOpponentsPermanent(card, (permanent) => permanent.IsDigimon && !permanent.HasNoDigivolutionCards))
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
                CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCondition, card);
                canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: CantSuspendCondition);
                cardEffects.Add(canNotSuspendClass);

                bool CantSuspendCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        if (permanent.IsDigimon)
                        {
                            if(permanent.DigivolutionCards.Count <= card.PermanentOfThisCard().DigivolutionCards.Count)
                            {
                                if (!permanent.TopCard.CanNotBeAffected(canNotSuspendClass))
                                    return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return CardEffectCommons.IsOpponentTurn(card);
                    }

                    return false;
                }
            }

            #endregion

            return cardEffects;
        }
    }
}