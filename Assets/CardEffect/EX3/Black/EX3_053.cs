using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX3
{
    public class EX3_053 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("De-Digivolve 1 and delete 1 Digimon with 5 or less Cost or opponent's Digimon can't digivolve", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] <De-Digivolve 1> all of your opponent's Digimon. Then, delete 1 of your opponent's Digimon with a play cost of 5 or less. If no Digimon is deleted by this effect, none of your opponent's unsuspended Digimon can digivolve until the end of your opponent's turn.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.GetCostItself <= 5)
                        {
                            if (permanent.TopCard.HasPlayCost)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
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
                        if (card.Owner.Enemy.GetBattleAreaDigimons().Count >= 1)
                        {
                            foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaDigimons())
                            {
                                if (CanSelectPermanentCondition(permanent))
                                {
                                    if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(new IDegeneration(permanent, 1, activateClass).Degeneration());
                                    }
                                }
                            }
                        }

                        List<Permanent> deleteTargetPermanents = new List<Permanent>();

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                        {
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition1));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition1,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                            {
                                foreach (Permanent permanent in permanents)
                                {
                                    deleteTargetPermanents.Add(permanent);
                                }

                                yield return null;
                            }
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: deleteTargetPermanents, activateClass: activateClass, successProcess: null, failureProcess: FailureProcess));

                        IEnumerator FailureProcess()
                        {
                            CanNotDigivolveClass canNotPutFieldClass = new CanNotDigivolveClass();
                            canNotPutFieldClass.SetUpICardEffect("Can't Digivolve", CanUseCondition1, card);
                            canNotPutFieldClass.SetUpCanNotEvolveClass(permanentCondition: PermanentCondition, cardCondition: CardCondition);
                            card.Owner.Enemy.UntilOwnerTurnEndEffects.Add((_timing) => canNotPutFieldClass);

                            ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().DebuffSE);

                            foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaDigimons())
                            {
                                if (PermanentCondition(permanent))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(permanent));
                                }
                            }

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                return true;
                            }

                            bool PermanentCondition(Permanent permanent)
                            {
                                if (permanent != null)
                                {
                                    if (permanent.TopCard != null)
                                    {
                                        if (permanent.TopCard.Owner == card.Owner.Enemy)
                                        {
                                            if (!permanent.IsSuspended)
                                            {
                                                if (permanent.TopCard.Owner.GetBattleAreaDigimons().Contains(permanent))
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }

                                return false;
                            }

                            bool CardCondition(CardSource cardSource)
                            {
                                if (cardSource.Owner == card.Owner.Enemy)
                                {
                                    return true;
                                }

                                return false;
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsTamer))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: Condition));
            }

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsTamer))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: Condition));
            }

            return cardEffects;
        }
    }
}