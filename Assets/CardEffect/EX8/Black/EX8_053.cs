using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DCGO.CardEffects.EX8
{
    public class EX8_053 : CEntity_Effect
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

            if (timing != EffectTiming.None)
            {
                bool OpponentsDigimon(Permanent permanent)
                {
                    return permanent.IsDigimon &&
                           permanent.DP >= 13000;
                }

                if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();

                    if (Condition())
                        thisPermanent.AddBoost(new Permanent.DPBoost("EX8_053", 5000, Condition));
                    else
                        thisPermanent.RemoveBoost("EX8_053");
                }

                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.PermanentOfThisCard().TopCard == card &&
                           CardEffectCommons.HasMatchConditionOpponentsPermanent(card, OpponentsDigimon);
                }
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

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        selectCardConditions:
                        new SelectCardConditionClass[]
                        {
                            new SelectCardConditionClass(
                                canTargetCondition: PlayableMineralorRock,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: true,
                                selectCardCoroutine: CardToPlay,
                                message: "Select 1 [Mineral] or [Rock] trait digimon with 8 cost or less to play",
                                maxCount: 1,
                                canEndNotMax: false,
                                mode: SelectCardEffect.Mode.Custom
                                )
                        },                        
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