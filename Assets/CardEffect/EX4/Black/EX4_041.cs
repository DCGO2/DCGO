using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX4
{
    public class EX4_041 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 card from hand to Draw 2", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By trashing 1 card with the [Blue Flare] or [Twilight] trait in your hand, <Draw 2>. (Draw 2 cards from your deck.)";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.CardTraits.Contains("Blue Flare"))
                            {
                                return true;
                            }

                            if (cardSource.CardTraits.Contains("BlueFlare"))
                            {
                                return true;
                            }

                            if (cardSource.CardTraits.Contains("Twilight"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.HandCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                            {
                                bool discarded = false;

                                int discardCount = 1;

                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: discardCount,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                    mode: SelectHandEffect.Mode.Discard,
                                    cardEffect: activateClass);

                                yield return StartCoroutine(selectHandEffect.Activate());

                                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                                {
                                    if (cardSources.Count >= 1)
                                    {
                                        discarded = true;

                                        yield return null;
                                    }
                                }

                                if (discarded)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 2, activateClass).Draw());
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top card of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] Reveal the top card of your deck. If that card has the [Blue Flare] or [Twilight] trait, add it to your hand. Trash the rest.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("Blue Flare") || cardSource.CardTraits.Contains("BlueFlare"))
                    {
                        return true;
                    }

                    if (cardSource.CardTraits.Contains("Twilight"))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanActivateOnDeletion(card))
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
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndProcessForAll(
                                        revealCount: 1,
                                        simplifiedSelectCardCondition:
                                        new SimplifiedSelectCardConditionClass(
                                                canTargetCondition: CanSelectCardCondition,
                                                message: "",
                                                mode: SelectCardEffect.Mode.AddHand,
                                                maxCount: -1,
                                                selectCardCoroutine: null),
                                        remainingCardsPlace: RemainingCardsPlace.Trash,
                                        activateClass: activateClass
                                    ));
                }
            }

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 1000, isInheritedEffect: true, card: card, condition: Condition));
            }

            return cardEffects;
        }
    }
}