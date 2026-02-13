using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX3
{
    public class EX3_024 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("Wingdramon") || targetPermanent.TopCard.CardNames.Contains("Groundramon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("Unsuspen_EX3_024");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns][Once Per Turn] When this Digimon becomes suspended, unsuspend it.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, (permanent) => permanent == card.PermanentOfThisCard()))
                        {
                            return true;
                        }
                    }

                    return false;
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
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());
                }
            }

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend your 1 Digimon to force opponent to attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Opponent's Main Phase] By suspending 1 of your Digimon with [Dramon] or [Examon] in its name, your opponent attacks with 1 of their Digimon.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (!permanent.IsSuspended && permanent.CanSuspend)
                        {
                            if (permanent.TopCard.HasDramonName)
                            {
                                return true;
                            }

                            if (permanent.TopCard.CardNames.Contains("Examon"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.CanSelectBySkill(activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
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
                                    Hashtable hashtable = new Hashtable();
                                    hashtable.Add("CardEffect", activateClass);

                                    Permanent tapPermanent = selectedPermanent;

                                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { tapPermanent }, hashtable).Tap());

                                    if (tapPermanent.TopCard != null)
                                    {
                                        if (tapPermanent.IsSuspended)
                                        {
                                            if (!GManager.instance.attackProcess.IsAttacking && card.Owner.Enemy.GetBattleAreaDigimons().Count(CanSelectPermanentCondition1) >= 1)
                                            {
                                                Permanent attackPermanent = null;

                                                selectPermanentEffect.SetUp(
                                                selectPlayer: card.Owner.Enemy,
                                                canTargetCondition: CanSelectPermanentCondition1,
                                                canTargetCondition_ByPreSelecetedList: null,
                                                canEndSelectCondition: null,
                                                maxCount: maxCount,
                                                canNoSelect: false,
                                                canEndNotMax: false,
                                                selectPermanentCoroutine: SelectPermanentCoroutine1,
                                                afterSelectPermanentCoroutine: null,
                                                mode: SelectPermanentEffect.Mode.Custom,
                                                cardEffect: activateClass);

                                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will attack.", "The opponent is selecting 1 Digimon that will attack.");
                                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                                IEnumerator SelectPermanentCoroutine1(Permanent permanent)
                                                {
                                                    attackPermanent = permanent;

                                                    yield return null;
                                                }

                                                if (attackPermanent != null)
                                                {
                                                    if (!attackPermanent.TopCard.CanNotBeAffected(activateClass))
                                                    {
                                                        if (attackPermanent.CanAttack(activateClass))
                                                        {
                                                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                                            selectAttackEffect.SetUp(
                                                                attacker: attackPermanent,
                                                                canAttackPlayerCondition: () => true,
                                                                defenderCondition: (permanent) => true,
                                                                cardEffect: activateClass);

                                                            selectAttackEffect.SetCanNotSelectNotAttack();

                                                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
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

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend your 1 Digimon to force opponent to attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Opponent's Main Phase] By suspending 1 of your Digimon with [Dramon] or [Examon] in its name, your opponent attacks with 1 of their Digimon.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (!permanent.IsSuspended && permanent.CanSuspend)
                        {
                            if (permanent.TopCard.HasDramonName)
                            {
                                return true;
                            }

                            if (permanent.TopCard.ContainsCardName("Examon"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.CanSelectBySkill(activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
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
                                    Hashtable hashtable = new Hashtable();
                                    hashtable.Add("CardEffect", activateClass);

                                    Permanent tapPermanent = selectedPermanent;

                                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { tapPermanent }, hashtable).Tap());

                                    if (tapPermanent.TopCard != null)
                                    {
                                        if (tapPermanent.IsSuspended)
                                        {
                                            if (!GManager.instance.attackProcess.IsAttacking && card.Owner.Enemy.GetBattleAreaDigimons().Count(CanSelectPermanentCondition1) >= 1)
                                            {
                                                Permanent attackPermanent = null;

                                                selectPermanentEffect.SetUp(
                                                selectPlayer: card.Owner.Enemy,
                                                canTargetCondition: CanSelectPermanentCondition1,
                                                canTargetCondition_ByPreSelecetedList: null,
                                                canEndSelectCondition: null,
                                                maxCount: maxCount,
                                                canNoSelect: false,
                                                canEndNotMax: false,
                                                selectPermanentCoroutine: SelectPermanentCoroutine1,
                                                afterSelectPermanentCoroutine: null,
                                                mode: SelectPermanentEffect.Mode.Custom,
                                                cardEffect: activateClass);

                                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will attack.", "The opponent is selecting 1 Digimon that will attack.");

                                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                                IEnumerator SelectPermanentCoroutine1(Permanent permanent)
                                                {
                                                    attackPermanent = permanent;

                                                    yield return null;
                                                }

                                                if (attackPermanent != null)
                                                {
                                                    if (attackPermanent.CanAttack(activateClass))
                                                    {
                                                        SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                                        selectAttackEffect.SetUp(
                                                            attacker: attackPermanent,
                                                            canAttackPlayerCondition: () => true,
                                                            defenderCondition: (permanent) => true,
                                                            cardEffect: activateClass);

                                                        selectAttackEffect.SetCanNotSelectNotAttack();

                                                        yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
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

            return cardEffects;
        }
    }
}