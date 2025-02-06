using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT13
{
    public class BT13_108 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] Until the end of your opponent's turn, 1 of your Digimon gains \"[Opponent's Turn] When this Digimon becomes suspended, delete all of your opponent's Digimon with a play cost less than or equal to this Digimon's\" and \"[Opponent's Turn] This Digimon isn't affected by your opponent's Option cards.\" ";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
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
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get effects.", "The opponent is selecting 1 Digimon that will get effects.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(selectedPermanent));
                            }

                            if (selectedPermanent != null)
                            {
                                ActivateClass activateClass1 = new ActivateClass();
                                activateClass1.SetUpICardEffect("Delete opponent's Digimon", CanUseCondition2, selectedPermanent.TopCard);
                                activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                                activateClass1.SetEffectSourcePermanent(selectedPermanent);
                                selectedPermanent.UntilOpponentTurnEndEffects.Add(GetCardEffect);

                                string EffectDiscription1()
                                {
                                    return "[Opponent's Turn] When this Digimon becomes suspended, delete all of your opponent's Digimon with a play cost less than or equal to this Digimon's";
                                }

                                bool CanUseCondition2(Hashtable hashtable1)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                    {
                                        if (CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable1, (permanent) => permanent == selectedPermanent))
                                        {
                                            if (CardEffectCommons.IsOpponentTurn(selectedPermanent.TopCard))
                                            {
                                                return true;
                                            }
                                        }
                                    }

                                    return false;
                                }

                                bool CanActivateCondition1(Hashtable hashtable1)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                    {
                                        if (selectedPermanent.TopCard.HasPlayCost)
                                        {
                                            int maxCost = selectedPermanent.TopCard.GetCostItself;

                                            bool PermanentCondition(Permanent permanent)
                                            {
                                                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, selectedPermanent.TopCard))
                                                {
                                                    if (permanent.TopCard.GetCostItself <= maxCost)
                                                    {
                                                        if (permanent.TopCard.HasPlayCost)
                                                        {
                                                            if (permanent.CanBeDestroyedBySkill(activateClass1))
                                                            {
                                                                if (!permanent.TopCard.CanNotBeAffected(activateClass1))
                                                                {
                                                                    return true;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                return false;
                                            }

                                            if (selectedPermanent.TopCard.Owner.Enemy.GetBattleAreaDigimons().Count(PermanentCondition) >= 1)
                                            {
                                                return true;
                                            }
                                        }
                                    }

                                    return false;
                                }

                                IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                    {
                                        if (selectedPermanent.TopCard.HasPlayCost)
                                        {
                                            int maxCost = selectedPermanent.TopCard.GetCostItself;

                                            bool PermanentCondition(Permanent permanent)
                                            {
                                                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, selectedPermanent.TopCard))
                                                {
                                                    if (permanent.TopCard.GetCostItself <= maxCost)
                                                    {
                                                        if (permanent.TopCard.HasPlayCost)
                                                        {
                                                            if (permanent.CanBeDestroyedBySkill(activateClass1))
                                                            {
                                                                if (!permanent.TopCard.CanNotBeAffected(activateClass1))
                                                                {
                                                                    return true;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                return false;
                                            }

                                            List<Permanent> destroyTargetPermanents = selectedPermanent.TopCard.Owner.Enemy.GetBattleAreaDigimons().Filter(PermanentCondition);
                                            yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(destroyTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass1)).Destroy());
                                        }
                                    }
                                }

                                ICardEffect GetCardEffect(EffectTiming _timing)
                                {
                                    if (_timing == EffectTiming.OnTappedAnyone)
                                    {
                                        return activateClass1;
                                    }

                                    return null;
                                }
                            }

                            if (selectedPermanent != null)
                            {
                                CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                                canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's option", CanUseCondition1, card);
                                canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                                selectedPermanent.UntilOpponentTurnEndEffects.Add((_timing) => canNotAffectedClass);

                                bool CanUseCondition1(Hashtable hashtable)
                                {
                                    return CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent);
                                }

                                bool CardCondition(CardSource cardSource)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                    {
                                        if (cardSource == selectedPermanent.TopCard)
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                bool SkillCondition(ICardEffect cardEffect)
                                {
                                    if (cardEffect != null)
                                    {
                                        if (cardEffect.EffectSourceCard != null)
                                        {
                                            if (cardEffect.EffectSourceCard.Owner == card.Owner.Enemy)
                                            {
                                                if (cardEffect.EffectSourceCard.IsOption)
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }

                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Delete 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] Delete 1 of your opponent's Digimon with the lowest play cost.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (CardEffectCommons.IsMinCost(permanent, card.Owner.Enemy, true))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
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

            return cardEffects;
        }
    }
}