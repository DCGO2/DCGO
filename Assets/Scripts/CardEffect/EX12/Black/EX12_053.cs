using System.Collections;
using System.Collections.Generic;

// Hagurumon
namespace DCGO.CardEffects.EX12
{
    public class EX12_053 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.EqualsTraits("ME");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 0, false, card, null, level: 2));
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal top 3, add 1 [Machine]/[Cyborg]/[Mutant] & 1 [ME] to hand, bottom deck the rest", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] Reveal the top 3 cards of your deck. Add 1 card with the [Machine], [Cyborg] or [Mutant] trait and 1 card with the [ME] trait among them to the hand. Return the rest to the bottom of the deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("Machine")
                        || cardSource.EqualsTraits("Cyborg")
                        || cardSource.EqualsTraits("Mutant");
                }

                bool CanSelectCardCondition2(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("ME");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition:CanSelectCardCondition1,
                                message: "Select 1 [Machine]/[Cyborg]/[Mutant] trait card.",
                                mode: SelectCardEffect.Mode.AddHand,
                                maxCount: 1,
                                selectCardCoroutine: null),
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition:CanSelectCardCondition2,
                                message: "Select 1 [ME] trait card.",
                                mode: SelectCardEffect.Mode.AddHand,
                                maxCount: 1,
                                selectCardCoroutine: null),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                        mutualConditions: true,
                        activateClass: activateClass
                    ));
                }
            }
            #endregion

            #region Inherited Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
