using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX7
{
    public class GrandGalemon_EX7_034 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Vortex

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Vortex", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "<Vortex> [End of Your Turn] This Digimon may attack an opponent's Digimon. With this effect, it can attack the turn it was played.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           card.PermanentOfThisCard().CanAttack(activateClass, isVortex: true) &&
                           CardEffectCommons.HasMatchConditionOpponentsPermanent(card, permanent =>
                               permanent.IsDigimon &&
                               card.PermanentOfThisCard().CanAttackTargetDigimon(permanent, activateClass, isVortex: true));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    if (selectedPermanent.CanAttack(activateClass, isVortex: true))
                    {
                        SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                        selectAttackEffect.SetUp(
                            attacker: selectedPermanent,
                            canAttackPlayerCondition: () => false,
                            defenderCondition: _ => true,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                    }
                }
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
                        "[When Digivolving] Suspend 1 Digimon. If this effect suspends your Digimon, this Digimon isn't affected by your opponent's Digimon's effects until the end of their turn.";
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
                        bool ownDigimon = false;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            ownDigimon = permanents.Some(permanent =>
                                CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card));

                            yield return null;
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
                    return CardEffectCommons.IsExistOnBattleArea(card);
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