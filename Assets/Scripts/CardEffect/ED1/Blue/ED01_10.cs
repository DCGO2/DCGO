using System.Collections;
using System.Collections.Generic;

//Bond of Snowflakes
namespace DCGO.CardEffects.ED1
{
    public class ED02_10 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 blue [Renamon]/[Valner Dragnogh] from hand for free, then place in battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] You may play 1 blue [Renamon] or [Valner Dragnogh] from your hand without paying the cost. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.CardColors.Contains(CardColor.Blue)
                        && (cardSource.EqualsCardName("Renamon")
                            || cardSource.EqualsCardName("Valner Dragnogh"))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                    { 
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                            canTargetCondition: CanSelectCardCondition,
                            SelectCardEffect.Root.Hand,
                            activateClass,
                            payCost: false
                        ));
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }
            #endregion

            #region Your turn - Delay
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<Delay> to attack with 1 of your blue [ALYSION] Digimon targeting 1 enemy Digimon with fewer sources than it", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[End of Your Turn] <Delay>. (By trashing this card after the placing turn, activate the effect.)\r\n• Choose 1 of your blue [ALYSION] trait Digimon and it attacks 1 of your opponent's Digimon with fewer digivolution cards than it. It can also attack your opponent's unsuspended Digimon with this effect.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanDeclareOptionDelayEffect(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool CanSelectOwnerPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.CardColors.Contains(CardColor.Blue)
                        && permanent.TopCard.HasAlysianTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnerPermanentCondition))
                        {
                            Permanent selectedPermanent = null;
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectOwnerPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will attack.",
                                "The opponent is selecting 1 Digimon that will attack.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;

                                yield return null;
                            }

                            if (selectedPermanent != null
                            && selectedPermanent.CanAttack(activateClass))
                            {
                                #region Attack unsuspended Digimon
                                CanAttackTargetDefendingPermanentClass canAttackTargetDefendingPermanentClass = new CanAttackTargetDefendingPermanentClass();
                                canAttackTargetDefendingPermanentClass.SetUpICardEffect("Can attack unsuspended Digimon", CanUseCondition, selectedPermanent.TopCard);
                                canAttackTargetDefendingPermanentClass.SetUpCanAttackTargetDefendingPermanentClass(
                                    attackerCondition: AttackerCondition,
                                    defenderCondition: DefenderCondition,
                                    cardEffectCondition: CardEffectCondition);
                                selectedPermanent.UntilEndAttackEffects.Add(_ => canAttackTargetDefendingPermanentClass);

                                bool CanUseCondition(Hashtable hashtable)
                                {
                                    return true;
                                }

                                bool AttackerCondition(Permanent attacker)
                                {
                                    return attacker == selectedPermanent;
                                }

                                bool DefenderCondition(Permanent defender)
                                {
                                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(defender, card)
                                        && !defender.IsSuspended
                                        && defender.DigivolutionCards.Count < selectedPermanent.DigivolutionCards.Count;
                                }

                                bool CardEffectCondition(ICardEffect cardEffect)
                                {
                                    return true;
                                }
                                #endregion

                                bool CanSelectEnemyPermanentCondition (Permanent permanent)
                                {
                                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                                        && permanent.DigivolutionCards.Count < selectedPermanent.DigivolutionCards.Count;
                                }

                                SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                selectAttackEffect.SetUp(
                                    attacker: selectedPermanent,
                                    canAttackPlayerCondition: () => false,
                                    defenderCondition: CanSelectEnemyPermanentCondition,
                                    cardEffect: activateClass);

                                selectAttackEffect.SetCanNotSelectNotAttack();

                                yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                            }
                        }
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                string EffectDescription() => "[Security] Activate this card's [Main] effect.";

                CardEffectCommons.AddActivateMainOptionSecurityEffect(card, ref cardEffects, "Play 1 blue [Renamon]/[Valner Dragnogh] from hand for free, then place in battle area", EffectDescription());
            }
            #endregion

            return cardEffects;
        }
    }
}
