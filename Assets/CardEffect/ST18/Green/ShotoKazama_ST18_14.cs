using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.ST18
{
    public class ShotoKazama_ST18_14 : CEntity_Effect
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

            if (timing == EffectTiming.OnCounterTiming)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Switch attack target of own Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetHashString("AttackSwitch_ST18_14");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Your Turn] When one of your Digimon attacks your opponent's Digimon, by suspending this Tamer, you may change the attack target to another of your opponent's Digimon or the player.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.IsOwnerTurn(card) &&
                           CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, AttackingPermanent);
                }

                bool AttackingPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsOwnerPermanent(permanent, card);
                }

                bool DefendingPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) ||
                           card.Owner.Enemy;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.CanActivateSuspendCostEffect(card) &&
                           GManager.instance.attackProcess.AttackingPermanent != null &&
                           GManager.instance.attackProcess.AttackingPermanent.CanSwitchAttackTarget;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: DefendingPermanent,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to attack or the Opponent.",
                        "The opponent is selecting 1 Digimon or you to attack.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SwitchDefender(
                            activateClass,
                            false,
                            permanent));
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