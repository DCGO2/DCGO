using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Arukenimon & Mummymon
namespace DCGO.CardEffects.BT16
{
    public class BT16_089 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Your Turn
            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete this Tamer to reduce Play Cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetHashString("DeleteTamer_BT13_103");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] When [Arukenimon] or [Mummymon] would be played from your hand, by deleting this Tamer, reduce the play cost by 3.";
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.Owner == card.Owner
                        && (cardSource.EqualsCardName("Mummymon")
                            || cardSource.EqualsCardName("Arukenimon"))
                        && CardEffectCommons.IsExistOnHand(cardSource);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass,
                        successProcess: permanents => SuccessProcess(),
                        failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        int reduceCost = 3;

                        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect($"Play Cost -{reduceCost}", CanUseCondition1, card);
                        changeCostClass.SetUpChangeCostClass(
                            changeCostFunc: ChangeCost,
                            cardSourceCondition: CardSourceCondition,
                            rootCondition: RootCondition,
                            isUpDown: isUpDown,
                            isCheckAvailability: () => false,
                            isChangePayingCost: () => true);
                        card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            return true;
                        }

                        int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                        {
                            if (CardSourceCondition(cardSource)
                            && RootCondition(root)
                            && PermanentsCondition(targetPermanents))
                            {
                                Cost -= reduceCost;
                            }
                            
                            return Cost;
                        }

                        bool PermanentsCondition(List<Permanent> targetPermanents)
                        {
                            return targetPermanents == null
                                    || targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0;
                        }

                        bool CardSourceCondition(CardSource cardSource)
                        {
                            return cardSource != null;
                        }

                        bool RootCondition(SelectCardEffect.Root root)
                        {
                            return true;
                        }

                        bool isUpDown()
                        {
                            return true;
                        }
                    }
                }
            }
            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Digimon from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Deletion] You may play 1 level 5 or lower Digimon card with [Myotismon] in it's text from your trash without paying the cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel
                        && cardSource.Level <= 5
                        && cardSource.HasText("Myotismon")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                        canTargetCondition: CanSelectCardCondition,
                        SelectCardEffect.Root.Trash,
                        activateClass,
                        payCost: false,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine
                        ));

                    CardSource selectedCard = null;

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count == 1)
                        { 
                            selectedCard = cardSources[0];
                        }

                        yield return null;
                    }

                    if (selectedCard != null)
                    {
                        Permanent selectedPermanent = selectedCard.PermanentOfThisCard();

                        ActivateClass activateClass1 = new ActivateClass();
                        activateClass1.SetUpICardEffect("Delete the Digimon", CanUseCondition1, card);
                        activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, "");
                        card.Owner.UntilOpponentTurnEndEffects.Add(GetCardEffect);

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            return CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent)
                                && CardEffectCommons.IsOpponentTurn(card);
                        }

                        bool CanActivateCondition1(Hashtable hashtable)
                        {
                            return CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent)
                                && selectedPermanent.CanBeDestroyedBySkill(activateClass1)
                                && !selectedPermanent.TopCard.CanNotBeAffected(activateClass1);
                        }

                        IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(new List<Permanent>() { selectedPermanent }, CardEffectCommons.CardEffectHashtable(activateClass1)).Destroy());
                        }

                        ICardEffect GetCardEffect(EffectTiming _timing)
                        {
                            if (_timing == EffectTiming.OnEndTurn)
                            {
                                return activateClass1;
                            }

                            return null;
                        }
                    }
                }
            }
            #endregion

            #region Security Skill
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}