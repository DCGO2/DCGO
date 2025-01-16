using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX5
{
    public class EX5_025 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 5)
                    {
                        if (targetPermanent.TopCard.CardTraits.Contains("Light Fang"))
                        {
                            return true;
                        }

                        if (targetPermanent.TopCard.CardTraits.Contains("LightFung"))
                        {
                            return true;
                        }

                        if (targetPermanent.TopCard.CardTraits.Contains("Night Claw"))
                        {
                            return true;
                        }

                        if (targetPermanent.TopCard.CardTraits.Contains("NightClaw"))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash digivolution cards and opponent's Digimon can't suspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("TrashDigivolutionCards_EX5_025");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] [Once Per Turn] For each of this Digimon's digivolution cards, trash any 1 digivolution card from 1 of your opponent's Digimon. Then, until the end of your opponent's turn, all of their Digimon with no digivolution cards can't suspend.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
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
                    if (!cardSource.CanNotTrashFromDigivolutionCards(activateClass))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        int maxCount = card.PermanentOfThisCard().DigivolutionCards.Count;

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                            permanentCondition: CanSelectPermanentCondition,
                            cardCondition: CanSelectCardCondition,
                            maxCount: maxCount,
                            canNoTrash: false,
                            isFromOnly1Permanent: true,
                            activateClass: activateClass
                        ));
                    }

                    CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                    canNotSuspendClass.SetUpICardEffect("Opponent's Digimon without digivolution cards can't Suspend", CanUseCondition1, card);
                    canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCondition);
                    card.Owner.UntilOpponentTurnEndEffects.Add(_timing => canNotSuspendClass);

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return true;
                    }

                    bool PermanentCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                        {
                            if (permanent.HasNoDigivolutionCards)
                            {
                                if (!permanent.TopCard.CanNotBeAffected(canNotSuspendClass))
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }

                    foreach (Permanent permanent in GManager.instance.turnStateMachine.gameContext.PermanentsForTurnPlayer)
                    {
                        if (PermanentCondition(permanent))
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(permanent));
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash digivolution cards and opponent's Digimon can't suspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("TrashDigivolutionCards_EX5_025");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] For each of this Digimon's digivolution cards, trash any 1 digivolution card from 1 of your opponent's Digimon. Then, until the end of your opponent's turn, all of their Digimon with no digivolution cards can't suspend.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
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
                    if (!cardSource.CanNotTrashFromDigivolutionCards(activateClass))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        int maxCount = card.PermanentOfThisCard().DigivolutionCards.Count;

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                            permanentCondition: CanSelectPermanentCondition,
                            cardCondition: CanSelectCardCondition,
                            maxCount: maxCount,
                            canNoTrash: false,
                            isFromOnly1Permanent: true,
                            activateClass: activateClass
                        ));
                    }

                    CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                    canNotSuspendClass.SetUpICardEffect("Opponent's Digimon without digivolution cards can't Suspend", CanUseCondition1, card);
                    canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCondition);
                    card.Owner.UntilOpponentTurnEndEffects.Add(_timing => canNotSuspendClass);

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return true;
                    }

                    bool PermanentCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                        {
                            if (permanent.HasNoDigivolutionCards)
                            {
                                if (!permanent.TopCard.CanNotBeAffected(canNotSuspendClass))
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }

                    foreach (Permanent permanent in GManager.instance.turnStateMachine.gameContext.PermanentsForTurnPlayer)
                    {
                        if (PermanentCondition(permanent))
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(permanent));
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnDigivolutionCardDiscarded)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("Unsuspend_EX5_025");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When an opponent's Digimon's digivolution card is trashed, unsuspend this Digimon.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerOnTrashDigivolutionCard(hashtable, PermanentCondition, cardEffect => true, cardSource => true))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                        new List<Permanent>() { selectedPermanent },
                        activateClass).Unsuspend());
                }
            }

            return cardEffects;
        }
    }
}