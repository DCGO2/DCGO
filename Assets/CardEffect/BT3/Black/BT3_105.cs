using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT3_105 : CEntity_Effect
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
                return "[Main] 1 of your Digimon gains <Reboot> (Unsuspend this Digimon during your opponent's unsuspend phase) and \"This Digimon can't have its DP reduced or be returned to its owner's hand or deck\" until the end of your opponent's next turn.";
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
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainReboot(
                            targetPermanent: permanent,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainImmuneFromDPMinus(
                            targetPermanent: permanent,
                            cardEffectCondition: null,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass,
                            effectName: "Can't have DP reduced"));

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotReturnToHand(
                            targetPermanent: permanent,
                            cardEffectCondition: null,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass,
                            effectName: "Can't return to hand"));

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotReturnToDeck(
                            targetPermanent: permanent,
                            cardEffectCondition: null,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass,
                            effectName: "Can't return to deck"));
                    }
                }
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect($"All opponent's Digimon can't attack to player", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] Your opponent's Digimon can't attack players for the turn.";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                bool AttackerCondition(Permanent Attacker)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(Attacker, card))
                    {
                        return true;
                    }

                    return false;
                }

                bool DefenderCondition(Permanent Defender)
                {
                    return Defender == null;
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotAttackPlayerEffect(
                    attackerCondition: AttackerCondition,
                    defenderCondition: DefenderCondition,
                    effectDuration: EffectDuration.UntilEachTurnEnd,
                    activateClass: activateClass,
                    effectName: "Can't Attack to player"));
            }
        }

        return cardEffects;
    }
}
