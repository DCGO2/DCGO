using System;
using System.Collections;
using System.Collections.Generic;

//Gaiomon
namespace DCGO.CardEffects.P
{
    public class P_183 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Card Name

            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Also treated as having [Greymon]", hashtable => true, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: changeCardNames);
                cardEffects.Add(changeCardNamesClass);

                List<string> changeCardNames(CardSource cardSource, List<string> CardNames)
                {
                    if (cardSource == card)
                    {
                        CardNames.Add("Greymon_P_183");
                    }

                    return CardNames;
                }
            }

            #endregion

            #region Blocker

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #region Reboot

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 digimon gains attack on Start of your main phase", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Until your opponent's turn ends, 1 of your opponent's Digimon gains '[Start of Your Main Phase] This Digimon attacks.' Then, this Digimon may attack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                        {
                            return true;
                        }
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

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        Permanent selectedPermanent = null;
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
                            activateClass1.SetUpICardEffect("[Start of Your Main Phase] This Digimon attacks.", CanUseCondition1, selectedPermanent.TopCard);
                            activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                            activateClass1.SetEffectSourcePermanent(selectedPermanent);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                            }

                            string EffectDiscription1()
                            {
                                return "[Start of Your Main Phase] This Digimon attacks.";
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

                            ICardEffect GetCardEffect(EffectTiming _timing)
                            {
                                if (_timing == EffectTiming.OnStartMainPhase)
                                {
                                    return activateClass1;
                                }

                                return null;
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
                        }
                    }

                    var permanent = card.PermanentOfThisCard();
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) && permanent.CanAttack(activateClass))
                    {
                        SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();
                        selectAttackEffect.SetUp(
                            attacker: permanent,
                            canAttackPlayerCondition: () => true,
                            defenderCondition: (permanent) => true,
                            cardEffect: activateClass);
                        yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                    }
                }
            }

            #endregion

            #region All Turns OPT

            if (timing == EffectTiming.OnAttackTargetChanged)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash top secuity card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("trashsec_P_183");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When attack targets change, trash your opponent's top security card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPermanentAttackTargetSwitch(hashtable, permanent => true))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                    player: card.Owner.Enemy,
                    destroySecurityCount: 1,
                    cardEffect: activateClass,
                    fromTop: true).DestroySecurity());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}