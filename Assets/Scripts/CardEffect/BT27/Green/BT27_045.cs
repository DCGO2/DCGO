using System.Collections;
using System.Collections.Generic;

// Liollmon - BT27-045
namespace DCGO.CardEffects.BT27
{
    public class BT27_045 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    CardSource source = permanent.TopCard;
                    return source.EqualsCardName("Frimon")
                        || source.EqualsCardName("Kyaromon")
                        || (source.HasLevel && source.IsLevel2 && source.EqualsTraits("ACCEL"));
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 0,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region On Play - Reveal Top 3
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal 3, add 1 [Leomon] and 1 [Unique]/[Boss]/[ACCEL]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Play] Reveal the top 3 cards of your deck. Add 1 card with [Leomon] in its name and 1 [Unique], [Boss] or [ACCEL] trait card among them to the hand. Return the rest to the bottom of the deck.";

                bool LeomonCondition(CardSource source)
                    => source.ContainsCardName("Leomon");

                bool TraitCondition(CardSource source)
                    => source.EqualsTraits("Unique") || source.EqualsTraits("Boss") || source.EqualsTraits("ACCEL");

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                            revealCount: 3,
                            simplifiedSelectCardConditions: new SimplifiedSelectCardConditionClass[]
                            {
                                new SimplifiedSelectCardConditionClass(
                                    canTargetCondition: LeomonCondition,
                                    message: "Select 1 card with [Leomon] in its name.",
                                    mode: SelectCardEffect.Mode.AddHand,
                                    maxCount: 1,
                                    selectCardCoroutine: null),
                                new SimplifiedSelectCardConditionClass(
                                    canTargetCondition: TraitCondition,
                                    message: "Select 1 [Unique], [Boss] or [ACCEL] trait card.",
                                    mode: SelectCardEffect.Mode.AddHand,
                                    maxCount: 1,
                                    selectCardCoroutine: null)
                            },
                            remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                            activateClass: activateClass,
                            mutualConditions: true));
                }
            }
            #endregion

            #region Inherited - Win Battle
            if (timing == EffectTiming.OnEndBattle)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false,
                    "[Your Turn] [Once Per Turn] When this Digimon wins a battle, gain 1 memory.");
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT27_045_Inherit");
                cardEffects.Add(activateClass);

                bool WinnerCondition(Permanent permanent)
                    => permanent.cardSources.Contains(card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerWhenWinBattle(hashtable, WinnerCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.CanAddMemory(activateClass))
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
