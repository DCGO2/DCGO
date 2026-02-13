using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;


public class BT8_102 : CEntity_Effect
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
                return "[Main] You may suspend 1 of your Digimon to suspend 1 of your opponent's Digimon or Tamers. Your opponent's cards suspended with this effect can't unsuspend until the end of their next turn.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
            }

            bool CanSelectPermanentCondition1(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                {
                    if (permanent.IsDigimon || permanent.IsTamer)
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
                    Permanent selectedPermanent = null;

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to suspend.", "The opponent is selecting 1 Digimon to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        if (selectedPermanent.TopCard != null)
                        {
                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                if (!selectedPermanent.IsSuspended && selectedPermanent.CanSuspend)
                                {
                                    Permanent tapPermanent = selectedPermanent;

                                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { tapPermanent }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                                    if (tapPermanent.TopCard != null)
                                    {
                                        if (tapPermanent.IsSuspended)
                                        {
                                            if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition1) >= 1)
                                            {
                                                maxCount = 1;

                                                if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition1) < maxCount)
                                                {
                                                    maxCount = card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition1);
                                                }

                                                foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaPermanents())
                                                {
                                                    permanent.oldIsTapped_playCard = permanent.IsSuspended;
                                                }

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
                                                    mode: SelectPermanentEffect.Mode.Tap,
                                                    cardEffect: activateClass);

                                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                                IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                                                {
                                                    foreach (Permanent selectedPermanent in permanents)
                                                    {
                                                        if (selectedPermanent != null)
                                                        {
                                                            if (selectedPermanent.IsSuspended)
                                                            {
                                                                if (!selectedPermanent.oldIsTapped_playCard)
                                                                {
                                                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantUnsuspendUntilOpponentTurnEnd(
                                                                        targetPermanent: selectedPermanent,
                                                                        activateClass: activateClass
                                                                    ));
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect($"Suspend 1 Digimon or a Tamer", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] Suspend 1 of your opponent's Digimon or Tamers.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (permanent != null)
                {
                    if (permanent.TopCard != null)
                    {
                        if (permanent.IsDigimon || permanent.IsTamer)
                        {
                            if (permanent.TopCard.Owner == card.Owner.Enemy)
                            {
                                if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                {
                                    return true;
                                }
                            }
                        }
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
                if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                {
                    int maxCount = Math.Min(1, card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

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
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
        }

        return cardEffects;
    }
}
