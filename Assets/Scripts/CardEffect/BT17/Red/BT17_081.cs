using System.Collections;
using System.Collections.Generic;
using System;

namespace DCGO.CardEffects.BT17
{
    public class BT17_081 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region All Turns

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain Memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns] When one of your Digimon is played or digivolves, by suspending this Tamer, if you have a Digimon with [Greymon] in its name, gain 1 memory. If you have a Digimon with [Garurumon] in its name, gain 1 memory.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool IsGreymonPermanentCondition(Permanent permanent)
                {
                    return permanent.IsDigimon && permanent.TopCard.HasGreymonName;
                }

                bool IsGarurumonPermanentCondition(Permanent permanent)
                {
                    return permanent.IsDigimon && permanent.TopCard.HasGarurumonName;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition) ||
                                CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition))
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
                        if (CardEffectCommons.CanActivateSuspendCostEffect(card))
                        {
                           return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() },
                            CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsGreymonPermanentCondition))
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                    }

                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsGarurumonPermanentCondition))
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                    }
                }
            }

            #endregion

            #region End of Your Turn

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 of your [Omnimon] can attack a player", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("Attack_BT17_081");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[End of Your Turn] [Once Per Turn] 1 of your Digimon with [Omnimon] in its name may attack a player.";
                }

                bool IsOmnimonPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.ContainsCardName("Omnimon"))
                        {
                            return permanent.CanAttack(activateClass);
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
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
                        if (CardEffectCommons.HasMatchConditionPermanent(IsOmnimonPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsOmnimonPermanentCondition))
                    {
                        Permanent selectedPermanent = null;

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsOmnimonPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOmnimonPermanentCondition,
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
                            "Select 1 Digimon that will attack.",
                            "The opponent is selecting 1 Digimon that will attack.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            if (selectedPermanent.CanAttack(activateClass))
                            {
                                if (selectedPermanent.CanAttackTargetDigimon(null, activateClass))
                                {
                                    SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                    selectAttackEffect.SetUp(
                                        attacker: selectedPermanent,
                                        canAttackPlayerCondition: () => true,
                                        defenderCondition: _ => false,
                                        cardEffect: activateClass);

                                    yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Security Effect

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}