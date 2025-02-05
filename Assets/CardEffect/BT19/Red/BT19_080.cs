using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT19
{
    public class BT19_080 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Turn
            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Give a Digimon Raid and it attacks", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When any of your Digimon digivolve into a Digimon with [Growlmon]/[Gallantmon] in its name, by suspending this Tamer, that Digimon gains <Raid> for the turn. Then, that Digimon attacks a player.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.IsDigimon && permanent.TopCard.EqualsCardName("Growlmon") || permanent.IsDigimon && permanent.TopCard.EqualsCardName("Gallantmon"))
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
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, CanSelectPermanentCondition))
                            {
                                return true;

                            }
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
                    List<Permanent> digivolvedPermanent = CardEffectCommons.GetPlayedPermanentsFromEnterFieldHashtable(hashtable, null);
                    List<CardSource> selectedCards = new List<CardSource>();

                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));
                        Permanent selectedPermanent = null;

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

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get Raid and attack a Player.", "The opponent is selecting 1 Digimon.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRaid(
                            targetPermanent: permanent,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass));
                        }

                        if (selectedPermanent != null)
                        {
                            if (selectedPermanent.CanAttack(activateClass))
                            {
                                SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                selectAttackEffect.SetUp(
                                    attacker: selectedPermanent,
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