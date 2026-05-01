using System.Collections;
using System.Collections.Generic;

// Floramon
namespace DCGO.CardEffects.BT25
{
    public class BT25_047 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasTSTraits;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    level: 2,
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 0,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 3 cards of the deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription() => "[On Play] Reveal the top 3 cards of your deck. Add 1 [Vegetation] or [Shaman] trait card and 1 [TS] trait card among them to the hand. Return the rest to the bottom of the deck.";


                bool FirstCardSelection(CardSource cardSource)
                    => cardSource.EqualsTraits("Vegetation") || cardSource.EqualsTraits("Shaman");


                bool SecondCardSelection(CardSource cardSource)
                    => cardSource.EqualsTraits("TS");

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOnPlay(hashtable, card);


                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                     && card.Owner.LibraryCards.Count >= 1;

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:FirstCardSelection,
                            message: "Select 1 card with [Vegetation] or [Shaman] trait.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:SecondCardSelection,
                            message: "Select 1 card with [TS] trait.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        },
                        mutualConditions: true,
                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                        activateClass: activateClass
                    ));
                }
            }
            #endregion

            #region ESS
            if (timing == EffectTiming.None)
            {
                bool Condition()
                    => CardEffectCommons.IsExistOnBattleArea(card)
                     && CardEffectCommons.IsOwnerTurn(card);

                bool PermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                    permanentCondition: PermanentCondition,
                            changeValue: 1000,
                            isInheritedEffect: true,
                            card: card,
                            condition: Condition,
                            effectName: () => "Your Digimon gain DP +1000"));
            }
            #endregion

            return cardEffects;
        }
    }
}
