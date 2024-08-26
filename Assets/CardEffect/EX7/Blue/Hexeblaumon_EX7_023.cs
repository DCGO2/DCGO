using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects
{
    public class Hexeblaumon_EX7_023 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Change Traits, Static Effects

            if (timing == EffectTiming.None)
            {
                ChangeTraitsClass changeTraitsClass = new ChangeTraitsClass();
                changeTraitsClass.SetUpICardEffect("Trait: Has [Ice-Snow] type", CanUseCondition, card);
                changeTraitsClass.SetUpChangeTraitsClass(changeeTraits: ChangeTraits);
                cardEffects.Add(changeTraitsClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                List<string> ChangeTraits(CardSource cardSource, List<string> cardTraits)
                {
                    if (cardSource == card)
                    {
                        cardTraits.Add("Ice-Snow");
                    }

                    return cardTraits;
                }
            }
            
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
                cardEffects.Add(CardEffectFactory.IcecladSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region When Digivolving
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 4 then return Tamer to bottom of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Trash any 4 digivolution cards of your opponent's Digimon. Then, if your opponent has no Digimon with digivolution cards, return 1 of your opponent's Tamers to the bottom of the deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }
                
                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        return true;
                    }

                    return false;
                }
                
                bool CanSelectTrashSourceCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return !cardSource.CanNotTrashFromDigivolutionCards(activateClass);
                }
                
                bool CanBounceTamerCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
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
                            permanentCondition: CanSelectTrashSourceCondition,
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
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Opponent's Turn] None of your opponent's Digimon with as many or fewer digivolution cards as this Digimon can suspend.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOpponentTurn(card);
                }
                
                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
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
                            if (CanSelectPermanentToNoSuspend(affected))
                                StartCoroutine(SelectPermanentCoroutine(affected));
                        }
                        
                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            if (selectedPermanent != null)
                            {
                                CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                                canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCondition1, card);
                                canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCondition);
                                selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => canNotSuspendClass);

                                if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                                }

                                bool CanUseCondition1(Hashtable hashtable)
                                {
                                    if (selectedPermanent.TopCard != null)
                                    {
                                        if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                bool PermanentCondition(Permanent permanent)
                                {
                                    if (permanent == selectedPermanent)
                                    {
                                        return true;
                                    }

                                    return false;
                                }
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