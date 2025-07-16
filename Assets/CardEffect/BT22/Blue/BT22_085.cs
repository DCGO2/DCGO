using System;
using System.Collections;
using System.Collections.Generic;

// Rina Shinomiya
namespace DCGO.CardEffects.BT22
{
    public class BT22_085 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region 3 Memory Setter

            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 digimon with [Veedramon] in name gains +3k DP ", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] 1 of your Digimon with [Veedramon] in its name gets +3000 DP until your opponent's turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsVeedramonInName);
                }

                bool IsVeedramonInName(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.ContainsCardName("Veedramon");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsVeedramonInName))
                    {
                        Permanent selectedPermanent = null;

                        #region Select Permanent

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, IsVeedramonInName));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsVeedramonInName,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }
                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to gain +3K DP.", "The opponent is selecting 1 Digimon to gain +3K DP.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        #endregion

                        if (selectedPermanent != null) 
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: selectedPermanent, changeValue: 3000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                    }
                }
            }

            #endregion

            #region Your Turn

            if (timing == EffectTiming.OnAllyAttack)
            {
                Permanent attackingVeedraInName = null;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Bounce this tamer to hand, give attacking digimon <Jamming>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When one of your Digimon with [Veedramon] in its name attacks, by returning this Tamer to the hand, that Digimon gains <Jamming> for the turn. (This Digimon can't be deleted in battles against Security Digimon.)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) && 
                           CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, PermanentAttackCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                bool PermanentAttackCondition(Permanent permanent)
                {
                    if (IsVeedramonInName(permanent))
                    {
                        attackingVeedraInName = permanent;
                        return true;
                    }
                    return false;
                }

                bool IsVeedramonInName(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.ContainsCardName("Veedramon");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent bounceTargetPermanent = card.PermanentOfThisCard();

                    if (bounceTargetPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.BouncePeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent>() { bounceTargetPermanent },
                            activateClass: activateClass,
                            successProcess: SuccessProcess(),
                            failureProcess: null));

                        IEnumerator SuccessProcess()
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainJamming(attackingVeedraInName, EffectDuration.UntilEachTurnEnd, activateClass));
                        }
                    }
                }
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}