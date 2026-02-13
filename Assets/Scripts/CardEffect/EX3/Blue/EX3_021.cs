using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX3
{
    public class EX3_021 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash digivolution cards and opponent's 1 Digimon can't attack and block", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Trash any 2 digivolution cards under 1 of your opponent's Digimon. Then, 1 of your opponent's Digimon with no digivolution cards can't attack or block until the end of your opponent's turn.";
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
                    return !cardSource.CanNotTrashFromDigivolutionCards(activateClass);
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.HasNoDigivolutionCards)
                        {
                            return true;
                        }
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
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                            permanentCondition: CanSelectPermanentCondition,
                            cardCondition: CanSelectCardCondition,
                            maxCount: 2,
                            canNoTrash: false,
                            isFromOnly1Permanent: true,
                            activateClass: activateClass
                        ));

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
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get effects.", "The opponent is selecting 1 Digimon that will get effects.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotAttack(
                                targetPermanent: selectedPermanent,
                                defenderCondition: null,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass,
                                effectName: "Can't Attack"));

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotBlock(
                                targetPermanent: selectedPermanent,
                                attackerCondition: null,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass,
                                effectName: "Can't Block"));
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}