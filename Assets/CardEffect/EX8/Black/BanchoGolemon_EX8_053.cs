using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX8
{
    public class BanchoGolemon_EX8_053 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.None)
            {
                string EffectDiscription()
                {
                    return "[All Turns] While your opponent has a Digimon with 13000 DP or more, this Digimon gets +5000 DP.";
                }

                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) && 
                           CardEffectCommons.HasMatchConditionOpponentsPermanent(card, permanent => permanent.IsDigimon && permanent.DP >= 13000);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           card == card.PermanentOfThisCard().TopCard &&
                           permanent == card.PermanentOfThisCard();
                }

                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                permanentCondition: PermanentCondition,
                changeValue: 5000,
                isInheritedEffect: false,
                card: card,
                condition: Condition,
                effectName: EffectDiscription));
            }
            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal top 3 from deck, Play 1 8 cost or less, [Mineral] or [Rock] trait Digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] Reveal the top 3 cards of your deck. You may play 1 [Mineral] or [Rock] trait Digimon card with a play cost of 8 or less among them without paying the cost. Trash the rest.";
                }

                bool PlayableMineralorRock(CardSource source)
                {
                    return CardEffectCommons.CanPlayAsNewPermanent(cardSource: source, payCost: false, cardEffect: activateClass) &&
                           source.IsDigimon &&
                           source.HasPlayCost && source.GetCostItself <= 8 &&
                           source.ContainsTraits("Mineral") || source.ContainsTraits("Rock");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndProcessForAll(
                        revealCount: 1,
                        simplifiedSelectCardCondition:
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition: PlayableMineralorRock,
                            message: "Select 1 [Mineral] or [Rock] trait digimon with 8 cost or less to play",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: -1,
                            selectCardCoroutine: CardToPlay),
                        remainingCardsPlace: RemainingCardsPlace.Trash,
                        activateClass: activateClass
                    ));

                    IEnumerator CardToPlay(CardSource source)
                    {
                        selectedCards.Add(source);

                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Library, activateETB: true));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}