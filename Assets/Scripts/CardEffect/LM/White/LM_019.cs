using System.Collections;
using System.Collections.Generic;
using System;

namespace DCGO.CardEffects.LM
{
    public class LM_019 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region OnPlay
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 4 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Reveal the top 4 cards of your deck. Add 1 card with [Gammamon] in its text among them to the hand. Return the rest to the bottom of the deck.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.HasText("Gammamon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 4,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 card with [Gammamon] in it's text.",
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

            #region All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete this Digimon to prevent 1 other Digimon from leaving Battle Area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Substitute_LM01_019");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When one of your Digimon with [Gammamon] in its text, other than [Bokomon], would leave the battle area other than by one of your effects, by deleting this Digimon, prevent it from leaving.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.HasText("Gammamon") && !permanent.TopCard.CardNames.Contains("Bokomon"))
                        {
                            if (permanent.willBeRemoveField)
                                return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, PermanentCondition))
                        {
                            if (!CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOwnerEffect(cardEffect, card)))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        Permanent thisCardPermanent = card.PermanentOfThisCard();

                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                        {
                            List<Permanent> permanents = new List<Permanent>();

                            if (_hashtable.ContainsKey("Permanents"))
                            {
                                if (_hashtable["Permanents"] is List<Permanent>)
                                {
                                    permanents = (List<Permanent>)_hashtable["Permanents"];

                                    permanents = permanents.Filter(PermanentCondition);

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                        targetPermanents: new List<Permanent>() { thisCardPermanent },
                                        activateClass: activateClass,
                                        successProcess: permanents => SuccessProcess(),
                                        failureProcess: null));
                                }
                            }

                            IEnumerator SuccessProcess()
                            {
                                foreach (Permanent permanent in permanents)
                                {
                                    permanent.willBeRemoveField = false;

                                    permanent.HideDeleteEffect();
                                    permanent.HideHandBounceEffect();
                                    permanent.HideDeckBounceEffect();
                                    permanent.HideWillRemoveFieldEffect();
                                }

                                yield return null;
                            }
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}