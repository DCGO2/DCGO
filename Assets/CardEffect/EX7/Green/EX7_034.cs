using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX7
{
    public class EX7_034 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Vortex

            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.VortexSelfEffect(isInheritedEffect: false, card: card,
                    condition: null));
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] You may suspend 1 Digimon. If this effect suspended your Digimon, this Digimon isn't affected by the effect of your opponent's Digimon's until the end of their turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleArea(permanent) &&
                           permanent.IsDigimon;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        Permanent selectedPermanent = null;
                        bool ownDigimon = false;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null &&
                            selectedPermanent.TopCard &&
                            !selectedPermanent.TopCard.CanNotBeAffected(activateClass) &&
                            !selectedPermanent.IsSuspended && selectedPermanent.CanSuspend)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                new SuspendPermanentsClass(new List<Permanent>() { selectedPermanent },
                                    CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                            ownDigimon = selectedPermanent.IsSuspended &&
                                         CardEffectCommons.IsOwnerPermanent(selectedPermanent, card);
                        }

                        if (ownDigimon)
                        {
                            Permanent permanentOfThisCard = card.PermanentOfThisCard();

                            CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                            canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's Digimon's effects",
                                CanUseConditionImmunity, card);
                            canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition,
                                SkillCondition: SkillCondition);
                            permanentOfThisCard.UntilOpponentTurnEndEffects.Add(GetCardEffect);

                            bool CanUseConditionImmunity(Hashtable hashtableImmunity)
                            {
                                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanentOfThisCard, card);
                            }

                            bool CardCondition(CardSource cardSource)
                            {
                                return CardEffectCommons.IsPermanentExistsOnBattleArea(permanentOfThisCard) &&
                                       cardSource == permanentOfThisCard.TopCard;
                            }

                            bool SkillCondition(ICardEffect cardEffect)
                            {
                                return CardEffectCommons.IsOpponentEffect(cardEffect, card) &&
                                       cardEffect.IsDigimonEffect;
                            }

                            ICardEffect GetCardEffect(EffectTiming timingImmunity)
                            {
                                return timingImmunity == EffectTiming.None ? canNotAffectedClass : null;
                            }
                        }
                    }
                }
            }

            #endregion

            #region Your Turn - ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Unsuspend_EX7_034");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] (Once Per Turn) When this Digimon attacks your opponent's Digimon, you may unsuspend this Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card) &&
                           CardEffectCommons.IsOwnerTurn(card) &&
                           CardEffectCommons.IsPermanentExistsOnBattleArea(GManager.instance.attackProcess.DefendingPermanent);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}