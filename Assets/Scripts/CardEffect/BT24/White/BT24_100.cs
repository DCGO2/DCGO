using System.Collections;
using System.Collections.Generic;

// In-Between Theater
namespace DCGO.CardEffects.BT24
{
    public class BT24_100 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region ignoring colours

            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);

                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.HasMatchConditionPermanent((permanent) => permanent.TopCard.Owner == card.Owner && (permanent.IsTamer || permanent.IsDigimon) && permanent.TopCard.HasTSTraits, true);

                bool CardCondition(CardSource cardSource)
                    => cardSource == card;
            }

            #endregion

            #region Main

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal top 3, add 1 [TS] card to hand, bottom deck the rest", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] Reveal the top 3 cards of your deck. Add 1 [TS] trait card among them to the hand. Return the rest to the bottom of the deck. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.HasTSTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 [TS] card.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                        activateClass: activateClass
                    ));

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }

            #endregion

            #region Delay

            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.Gain2MemoryOptionDelayEffect(card));
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaceSelfDelayOptionSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}
