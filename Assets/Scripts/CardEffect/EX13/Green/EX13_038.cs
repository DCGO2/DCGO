using System.Collections;
using System.Collections.Generic;

// Salamon
namespace DCGO.CardEffects.EX13
{
    public class EX13_038 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                cardEffects.Add(CardEffectFactory.OnPlayClass(
                    card: card,
                    effectName: "Reveal the top 3 cards of deck, add 1 [Leopardmon] text card and 1 [Beast]/[Animal]/[Sovereign] Digimon to hand",
                    activateCoroutine: ActivateCoroutine,
                    effectDescription: EffectDescription(),
                    optional: false));

                string EffectDescription()
                    => "[On Play] Reveal the top 3 cards of your deck. Add 1 card with [Leopardmon] in its text and 1 Digimon card with [Beast], [Animal] or [Sovereign], other than [Sea Animal], in any of its traits among them to the hand. Return the rest to the bottom of the deck.";

                bool IsLeopardmonTextCard(CardSource cardSource)
                    => cardSource.HasText("Leopardmon");

                bool IsBeastDigimonCard(CardSource cardSource)
                    => cardSource.IsDigimon
                        && cardSource.HasBeastTraits;

                IEnumerator ActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition: IsLeopardmonTextCard,
                                message: "Select 1 card with [Leopardmon] in its text.",
                                mode: SelectCardEffect.Mode.AddHand,
                                maxCount: 1,
                                selectCardCoroutine: null),
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition: IsBeastDigimonCard,
                                message: "Select 1 Digimon card with [Beast], [Animal] or [Sovereign] in any of its traits.",
                                mode: SelectCardEffect.Mode.AddHand,
                                maxCount: 1,
                                selectCardCoroutine: null),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                        activateClass: activateClass
                    ));
                }
            }
            #endregion

            #region All Turns - ESS
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.IsSuspended;

                bool Condition()
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                    permanentCondition: PermanentCondition,
                    changeValue: 1000,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition,
                    effectName: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
