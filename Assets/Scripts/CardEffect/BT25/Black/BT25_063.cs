using System.Collections;
using System.Collections.Generic;

// Commandramon
namespace DCGO.CardEffects.BT25
{
    public class BT25_063 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region WM/OP Shared
            string SharedEffectName = "Reveal 3, add [Chaosmon] in name or [D-Brigade]/[ACCEL] trait to hand, return rest to top or bot";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: -1,
                whenMoving: true,
                onPlay: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] Reveal the top 3 cards of your deck. Add 1 card with [Chaosmon] in its name or the [D-Brigade] or [ACCEL] trait among them to the hand. Return the rest to the top or bottom of the deck.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.ContainsCardName("Chaosmon")            
                    || cardSource.EqualsTraits("D-Brigade")
                    || cardSource.EqualsTraits("ACCEL");
            }

            IEnumerator SharedActivateCoroutine(Hashtable _hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                    revealCount: 3,
                    simplifiedSelectCardConditions:
                    new SimplifiedSelectCardConditionClass[]
                    {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 [Chaosmon] in name or [D-Brigade]/[ACCEL] trait card to add to hand.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                    },
                    remainingCardsPlace: RemainingCardsPlace.DeckTopOrBottom,
                    activateClass: activateClass
                ));
            }
            #endregion

            #region All Turns - Inherited Effect
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return true;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 1000,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}
