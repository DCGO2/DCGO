using System.Collections;
using System.Collections.Generic;

// Liollmon
namespace DCGO.CardEffects.BT27
{
    public class BT27_045 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - [Frimon]/[Kyaromon]
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Frimon")
                        || targetPermanent.TopCard.EqualsCardName("Kyaromon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Alternate Digivolution Requirement - Lv.2 w/[ACCEL] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsTraits("ACCEL");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 2));
            }
            #endregion

            #region Rule - Name: Treated as including [Leomon]
            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Treated as including [Leomon] in its name", CanUseCondition, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: ChangeCardNames);
                cardEffects.Add(changeCardNamesClass);

                bool CanUseCondition(Hashtable hashtable)
                    => true;

                List<string> ChangeCardNames(CardSource cardSource, List<string> cardNames)
                {
                    if (cardSource == card)
                    {
                        cardNames.Add("Leomon");
                    }

                    return cardNames;
                }
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 3 cards of deck, add 1 [Leomon] in name and 1 [Unique]/[Boss]/[ACCEL] trait card to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Play] Reveal the top 3 cards of your deck. Add 1 card with [Leomon] in its name and 1 [Unique], [Boss] or [ACCEL] trait card among them to the hand. Return the rest to the bottom of the deck.";

                bool LeomonNameCondition(CardSource cardSource)
                    => cardSource.ContainsCardName("Leomon");

                bool TraitCondition(CardSource cardSource)
                    => cardSource.EqualsTraits("Unique")
                        || cardSource.EqualsTraits("Boss")
                        || cardSource.EqualsTraits("ACCEL");

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition: LeomonNameCondition,
                                message: "Select 1 card with [Leomon] in its name.",
                                mode: SelectCardEffect.Mode.AddHand,
                                maxCount: 1,
                                selectCardCoroutine: null),
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition: TraitCondition,
                                message: "Select 1 card with the [Unique], [Boss] or [ACCEL] trait.",
                                mode: SelectCardEffect.Mode.AddHand,
                                maxCount: 1,
                                selectCardCoroutine: null),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                        activateClass: activateClass,
                        mutualConditions: true
                    ));
                }
            }
            #endregion

            #region Your Turn - OPT - ESS
            if (timing == EffectTiming.OnEndBattle)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT27_045_Inherit");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Your Turn] [Once Per Turn] When this Digimon wins a battle, gain 1 memory.";

                bool WinnerCondition(Permanent permanent) => permanent.cardSources.Contains(card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerWhenWinBattle(hashtable, WinnerCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.CanAddMemory(activateClass))
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
