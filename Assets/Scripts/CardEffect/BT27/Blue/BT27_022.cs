using System.Collections;
using System.Collections.Generic;

// Renamon
namespace DCGO.CardEffects.BT27
{
    public class BT27_022 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Trait Rule
            if (timing == EffectTiming.None)
            {
                ChangeTraitsClass changeTraitsClass = new ChangeTraitsClass();
                changeTraitsClass.SetUpICardEffect("[Rule] Trait: Has [Ice-Snow] Type.", _ => true, card);
                changeTraitsClass.SetUpChangeTraitsClass(changeeTraits: ChangeTraits);
                cardEffects.Add(changeTraitsClass);

                List<string> ChangeTraits(CardSource cardSource, List<string> cardTraits)
                {
                    if (cardSource == card && !cardTraits.Contains("Ice-Snow"))
                    {
                        cardTraits.Add("Ice-Snow");
                    }

                    return cardTraits;
                }
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal top 3, add 1 [Ice-Snow]/[ALYSION] card & 1 blue Tamer to hand, bottom deck the rest", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Play] Reveal the top 3 cards of your deck. Add 1 [Ice-Snow] or [ALYSION] trait card and 1 blue Tamer card among them to the hand. Return the rest to the bottom of the deck.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                bool TraitCardCondition(CardSource cardSource)
                    => cardSource.EqualsTraits("Ice-Snow")
                        || cardSource.EqualsTraits("ALYSION");

                bool BlueTamerCondition(CardSource cardSource)
                    => cardSource.IsTamer
                        && cardSource.HasCardColor(CardColor.Blue);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition: TraitCardCondition,
                                message: "Select 1 [Ice-Snow] or [ALYSION] trait card.",
                                mode: SelectCardEffect.Mode.AddHand,
                                maxCount: 1,
                                selectCardCoroutine: null),
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition: BlueTamerCondition,
                                message: "Select 1 blue Tamer card.",
                                mode: SelectCardEffect.Mode.AddHand,
                                maxCount: 1,
                                selectCardCoroutine: null),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                        mutualConditions: true,
                        activateClass: activateClass));
                }
            }
            #endregion

            #region When Attacking - Inherited
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Change the attack target to 1 opponent's unsuspended Digimon with the fewest digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] If this Digimon has the [Ice-Snow] trait, you may change the attack target to 1 of your opponent's unsuspended Digimon with the fewest digivolution cards.";

                bool NewTargetCondition(Permanent permanent)
                    => CardEffectCommons.IsMinDigivolutionCards(permanent, card.Owner.Enemy, target => !target.IsSuspended)
                        && permanent != GManager.instance.attackProcess.DefendingPermanent;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && card.PermanentOfThisCard().TopCard.EqualsTraits("Ice-Snow")
                        && card.PermanentOfThisCard().CanSwitchAttackTarget
                        && CardEffectCommons.HasMatchConditionPermanent(NewTargetCondition);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: NewTargetCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to become the new attack target.", "The opponent is selecting 1 Digimon to become the new attack target.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SwitchDefender(activateClass, false, permanent));
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
