using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT19
{
    public class BT19_055 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 3 cards of your deck, add 1, place 1 under a tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] Reveal the top 3 cards of your deck. Among them, add 1 card with [Knightmon] in its text or the [Twilight] trait to the hand and place 1 such card under any of your Tamers. Return the rest to the bottom of the deck.";
                }

                bool SelectKnightmonOrTwilight(CardSource source)
                {
                    return source.HasText("Knightmon") || source.EqualsTraits("Twilight");
                }

                bool MyTamer(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card) &&
                           permanent.IsTamer;
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
                    int tamerCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, MyTamer));
                    CardSource tuckedCard = null;

                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, MyTamer))
                    {
                       yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                       revealCount: 3,
                       simplifiedSelectCardConditions:
                       new SimplifiedSelectCardConditionClass[]
                       {
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition:SelectKnightmonOrTwilight,
                                message: "Select 1 card with [Knightmon] in its text or the [Twilight] trait to add to hand.",
                                mode: SelectCardEffect.Mode.AddHand,
                                maxCount: 1,
                                selectCardCoroutine: null),
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition:SelectKnightmonOrTwilight,
                                message: "Select 1 card with [Knightmon] in its text or the [Twilight] trait to place under a tamer.",
                                mode: SelectCardEffect.Mode.Custom,
                                maxCount: 1,
                                selectCardCoroutine: PlaceUnderTamer),
                       },
                       remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                       activateClass: activateClass));
                    }
                    else
                    {
                       yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                       revealCount: 3,
                       simplifiedSelectCardConditions:
                       new SimplifiedSelectCardConditionClass[]
                       {
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition:SelectKnightmonOrTwilight,
                                message: "Select 1 card with [Knightmon] in its text or the [Twilight] trait to add to hand.",
                                mode: SelectCardEffect.Mode.AddHand,
                                maxCount: 1,
                                selectCardCoroutine: null),
                       },
                       remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                       activateClass: activateClass));
                    }

                        

                    IEnumerator PlaceUnderTamer(CardSource source)
                    {
                        tuckedCard = source;
                        yield return null;
                    }

                    if(tuckedCard != null)
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, MyTamer))
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: MyTamer,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectTamerCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer that will add card to sources.", "The opponent is selecting 1 Tamer that will add card to sources.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectTamerCoroutine(Permanent permanent)
                            {
                                Permanent selectedPermanent = permanent;

                                if (selectedPermanent != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource> { tuckedCard }, activateClass));
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}