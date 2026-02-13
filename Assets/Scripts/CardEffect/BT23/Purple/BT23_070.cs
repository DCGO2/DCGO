using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Belphemon (X Antibody)
namespace DCGO.CardEffects.BT23
{
    public class BT23_070 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Alt Digivovle Cost

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel6
                        && targetPermanent.TopCard.ContainsCardName("Belphemon")
                        && !targetPermanent.TopCard.HasXAntibodyTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card, condition: null)
                );
            }

            #endregion

            #region Rush

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RushSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region Piercing

            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete all highest level digimon, then if with [Belphemon] in name is in sources, atttack without suspending", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Delete all of your opponent's Digimon with the highest level. Then, if a card with [Belphemon] in its name is in this Digimon's digivolution cards, this Digimon attacks without suspending.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool HasBelphemonInName(CardSource cardSource)
                {
                    return cardSource.ContainsCardName("Belphemon");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    var digimonToDelete = card.Owner.Enemy.GetBattleAreaDigimons().Filter(x => CardEffectCommons.IsMaxLevel(x, card.Owner.Enemy));
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                        targetPermanents: digimonToDelete,
                        activateClass: activateClass,
                        successProcess: null,
                        failureProcess: null));

                    var thisPermament = card.PermanentOfThisCard();

                    if (thisPermament.DigivolutionCards.Any(HasBelphemonInName) && thisPermament.CanAttack(activateClass, withoutTap: true))
                    {
                        SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                        selectAttackEffect.SetUp(
                            attacker: thisPermament,
                            canAttackPlayerCondition: () => true,
                            defenderCondition: (permanent) => true,
                            cardEffect: activateClass);

                        selectAttackEffect.SetWithoutTap();
                        selectAttackEffect.SetCanNotSelectNotAttack();

                        yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                    }
                }
            }

            #endregion

            #region End of Attack

            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [Belphemon: Sleep Mode] in trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Attack] This Digimon may digivolve into [Belphemon: Sleep Mode] in the trash, ignoring digivolution requirements and without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnEndAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsSleepMode);
                }

                bool IsSleepMode(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.EqualsCardName("Belphemon: Sleep Mode");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsSleepMode)) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: IsSleepMode,
                        payCost: false,
                        reduceCostTuple: null,
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: 2,
                        isHand: false,
                        activateClass: activateClass,
                        successProcess: null)
                    );
                }
            }

            #endregion

            return cardEffects;
        }
    }
}