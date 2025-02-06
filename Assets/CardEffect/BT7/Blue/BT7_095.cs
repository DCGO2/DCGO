using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT7_095 : CEntity_Effect
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
                return "[Main] For the turn, 1 of your Digimon gets +3000 DP and can also attack your opponent's unsuspended Digimon with no digivolution cards.";
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

                    selectPermanentEffect.SetUpCustomMessage("Selcect 1 Digimon that will get effects.", "The opponent is selecting 1 Digimon that will get effects.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            targetPermanent: permanent,
                            changeValue: 3000,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass));

                        Permanent selectedPermanent = permanent;

                        if (selectedPermanent != null)
                        {
                            CanAttackTargetDefendingPermanentClass canAttackTargetDefendingPermanentClass = new CanAttackTargetDefendingPermanentClass();
                            canAttackTargetDefendingPermanentClass.SetUpICardEffect($"Can attack to unsuspended Digimon with no digivolution cards", CanUseCondition1, card);
                            canAttackTargetDefendingPermanentClass.SetUpCanAttackTargetDefendingPermanentClass(
                                attackerCondition: AttackerCondition,
                                defenderCondition: DefenderCondition,
                                cardEffectCondition: CardEffectCondition);
                            selectedPermanent.UntilEachTurnEndEffects.Add((_timing) => canAttackTargetDefendingPermanentClass);

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                {
                                    return true;
                                }

                                return false;
                            }

                            bool AttackerCondition(Permanent attacker)
                            {
                                return attacker == selectedPermanent;
                            }

                            bool DefenderCondition(Permanent defender)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(defender, card))
                                {
                                    if (!defender.IsSuspended)
                                    {
                                        if (defender.HasNoDigivolutionCards)
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
                            }

                            bool CardEffectCondition(ICardEffect cardEffect)
                            {
                                return true;
                            }
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
