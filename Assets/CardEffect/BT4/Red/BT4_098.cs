using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT4_098 : CEntity_Effect
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
                return "[Main] 1 of your Digimon with [Hybrid] in its form gets +3000 DP, <Security Attack +1> (This Digimon checks 1 additional security card), and \"[Your Turn] When this Digimon is blocked, gain + 3 memory\" for the turn.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    if (permanent.TopCard.CardTraits.Contains("Hybrid"))
                    {
                        return true;
                    }
                }

                return false;
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

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 Digimon that will get effects.",
                        "The opponent is selecting 1 Digimon that will get effects.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            targetPermanent: permanent,
                            changeValue: 3000,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass));

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(
                            targetPermanent: permanent,
                            changeValue: 1,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass));

                        Permanent selectedPermanent = permanent;

                        if (selectedPermanent != null)
                        {
                            ActivateClass activateClass1 = new ActivateClass();
                            activateClass1.SetUpICardEffect("Memory +3", CanUseCondition2, selectedPermanent.TopCard);
                            activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, true, EffectDiscription1());
                            activateClass1.SetEffectSourcePermanent(selectedPermanent);
                            CardEffectCommons.AddEffectToPermanent(
                                targetPermanent: selectedPermanent,
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                card: card,
                                cardEffect: activateClass1,
                                timing: EffectTiming.OnBlockAnyone
                            );

                            string EffectDiscription1()
                            {
                                return "[Your Turn] When this Digimon is blocked, gain + 3 memory";
                            }

                            bool CanUseCondition2(Hashtable hashtable1)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                {
                                    if (CardEffectCommons.IsOwnerTurn(selectedPermanent.TopCard))
                                    {
                                        if (CardEffectCommons.CanTriggerOnAttack(hashtable1, selectedPermanent.TopCard))
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
                                    if (selectedPermanent.TopCard.Owner.CanAddMemory(activateClass1))
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(selectedPermanent.TopCard.Owner.AddMemory(3, activateClass1));
                            }
                        }
                    }
                }
            }
        }


        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect($"Security Attack +1", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] All of your Digimon gain <Security Attack +1>  (This Digimon checks 1 additional security card) until the end of your next turn.";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttackPlayerEffect(
                    permanentCondition: PermanentCondition,
                    changeValue: 1,
                    effectDuration: EffectDuration.UntilOwnerTurnEnd,
                    activateClass: activateClass));
            }
        }

        return cardEffects;
    }
}
