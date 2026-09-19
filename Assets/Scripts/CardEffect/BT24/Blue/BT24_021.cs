using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

// SnowGoblimon
namespace DCGO.CardEffects.BT24
{
    public class BT24_021 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Condition

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Tsunomon") ||
                        (targetPermanent.TopCard.IsLevel2 && targetPermanent.TopCard.HasTSTraits);
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal 3 from deck. Add 1 [Demon] or [Shaman] and 1 [Titan]. Trash 1 card if you added any.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] Reveal the top 3 cards of your deck. Add 1 Digimon card with the [Demon] or [Shaman] trait and 1 card with the [Titan] trait among them to the hand. Return the rest to the bottom of the deck. If this effect added, trash 1 card in your hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool CanSelectCardCondition(CardSource card)
                {
                    return card.IsDigimon && 
                        (card.EqualsTraits("Demon") || card.EqualsTraits("Shaman"));
                }

                bool CanSelectCardCondition1(CardSource card)
                {
                    return card.EqualsTraits("Titan");
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool addedCard = false;
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new (
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 Digimon with [Demon] or [Shaman] in its traits.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new (
                            canTargetCondition:CanSelectCardCondition1,
                            message: "Select 1 card with [Titan] in its traits.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                        activateClass: activateClass,
                        revealedCardsCoroutine: RevealedCardsCoroutine
                    ));

                    IEnumerator RevealedCardsCoroutine(List<CardSource> revealedCards)
                    {
                        addedCard = revealedCards.Count(cardSource => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource)) >= 1;

                        yield return null;
                    }

                    if (addedCard && card.Owner.HandCards.Count >= 1)
                    {
                        int discardCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: (card) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: discardCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 Card to trash.", "The opponent is selecting 1 card to trash from their hand.");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    }
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.OnDiscardHand)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("When your hand is trashed from, digivolve", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT24_021_YT_ESS");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] [Once Per Turn] When your hand is trashed from, this [Demon] or [Titan] trait Digimon may digivolve into [Titamon] or a [Titan] trait Digimon card in the trash with the digivolution cost reduced by 1.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && 
                        (cardSource.EqualsCardName("Titamon") || cardSource.EqualsTraits("Titan")) && 
                        cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, 
                                                            false, 
                                                            activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnTrashHand(hashtable, null, cardSource => cardSource.Owner == card.Owner)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) 
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition)
                        && (card.PermanentOfThisCard().TopCard.EqualsTraits("Demon") 
                            || card.PermanentOfThisCard().TopCard.EqualsTraits("Titan"));
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: card.PermanentOfThisCard(),
                            cardCondition: CanSelectCardCondition,
                            payCost: true,
                            reduceCostTuple: (reduceCost: 1, reduceCostCardCondition: null),
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: false,
                            activateClass: activateClass,
                            successProcess: null));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
