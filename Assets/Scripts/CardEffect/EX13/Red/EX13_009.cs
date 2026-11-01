using System.Collections;
using System.Collections.Generic;

// Huckmon
namespace DCGO.CardEffects.EX13
{
    public class EX13_009 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal 3, add 1 [Huckmon]/[Sistermon] text Digimon and 1 such Tamer or Option", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Play] Reveal the top 3 cards of your deck. Add 1 Digimon card with [Huckmon] or [Sistermon] in its text and 1 such Tamer card or Option card among them to the hand. Return the rest to the bottom of the deck.";

                bool HasHuckmonOrSistermonText(CardSource cardSource)
                    => cardSource.HasText("Huckmon") || cardSource.HasText("Sistermon");

                bool CanSelectDigimonCondition(CardSource cardSource)
                    => cardSource.IsDigimon && HasHuckmonOrSistermonText(cardSource);

                bool CanSelectTamerOrOptionCondition(CardSource cardSource)
                    => (cardSource.IsTamer || cardSource.IsOption) && HasHuckmonOrSistermonText(cardSource);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition: CanSelectDigimonCondition,
                                message: "Select 1 Digimon card with [Huckmon] or [Sistermon] in its text.",
                                mode: SelectCardEffect.Mode.AddHand,
                                maxCount: 1,
                                selectCardCoroutine: null),
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition: CanSelectTamerOrOptionCondition,
                                message: "Select 1 Tamer or Option card with [Huckmon] or [Sistermon] in its text.",
                                mode: SelectCardEffect.Mode.AddHand,
                                maxCount: 1,
                                selectCardCoroutine: null),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                        activateClass: activateClass));
                }
            }
            #endregion

            #region Your Turn - ESS
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("EX13_009_ESS_YT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Your Turn] [Once Per Turn] When any of your white Digimon are played, gain 1 memory.";

                bool IsOwnerWhiteDigimon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.CardColors.Contains(CardColor.White);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, IsOwnerWhiteDigimon);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
