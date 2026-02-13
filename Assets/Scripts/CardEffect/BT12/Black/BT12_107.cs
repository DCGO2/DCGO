using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT12
{
    public class BT12_107 : CEntity_Effect
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
                    return "[Main] Until the end of your opponent's turn, 1 of your opponent's Digimon gains \"[Start of Your Main Phase] Attack with this Digimon.\"";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = null;

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

                        selectPermanentEffect.SetUpCustomMessage("Selcect 1 Digimon that will get effects.", "The opponent is selecting 1 Digimon that will get effects.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            ActivateClass activateClass1 = new ActivateClass();
                            activateClass1.SetUpICardEffect("Attack with this Digimon", CanUseCondition1, selectedPermanent.TopCard);
                            activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                            activateClass1.SetEffectSourcePermanent(selectedPermanent);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                            }

                            string EffectDiscription1()
                            {
                                return "[Start of Your Main Phase] Attack with this Digimon.";
                            }

                            bool CanUseCondition1(Hashtable hashtable1)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                {
                                    if (selectedPermanent.TopCard.Owner.GetBattleAreaDigimons().Contains(selectedPermanent))
                                    {
                                        if (GManager.instance.turnStateMachine.gameContext.TurnPlayer == selectedPermanent.TopCard.Owner)
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
                                    if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                    {
                                        if (selectedPermanent.CanAttack(activateClass1))
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
                                    if (selectedPermanent.CanAttack(activateClass1))
                                    {
                                        SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                        selectAttackEffect.SetUp(
                                            attacker: selectedPermanent,
                                            canAttackPlayerCondition: () => true,
                                            defenderCondition: (permanent) => true,
                                            cardEffect: activateClass1);

                                        selectAttackEffect.SetCanNotSelectNotAttack();

                                        yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                                    }
                                }
                            }

                            ICardEffect GetCardEffect(EffectTiming _timing)
                            {
                                if (_timing == EffectTiming.OnStartMainPhase)
                                {
                                    return activateClass1;
                                }

                                return null;
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Add this card to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] Add this card to its owner's hand.";
                }
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
                }
            }

            return cardEffects;
        }
    }
}