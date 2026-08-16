using System;
using System.Collections;
using System.Collections.Generic;

// Sangomon
namespace DCGO.CardEffects.BT26
{
    public class BT26_018 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsTraits("DS");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 2));
            }
            #endregion

            #region Shared WM/OP

            string SharedEffectName()
                => "Reveal top 3, add 1 [Aqua]/[Sea Animal]/[DS] card to hand, then trash opponent's bottom digivolution card";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Reveal the top 3 cards of your deck. Add 1 card with [Aqua] or [Sea Animal] in any of its traits or 1 card with the [DS] trait among them to the hand. Return the rest to the bottom of the deck. Then, trash the bottom digivolution card of 1 of your opponent's Digimon.";

            bool CanSelectRevealCardCondition(CardSource cardSource)
                => cardSource.ContainsTraits("Aqua") || cardSource.ContainsTraits("Sea Animal") || cardSource.ContainsTraits("DS");

            bool CanSelectTrashDigivolutionTargetCondition(Permanent permanent, ICardEffect activateClass)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.DigivolutionCards.Count >= 1
                    && !permanent.DigivolutionCards[permanent.DigivolutionCards.Count - 1].CanNotTrashFromDigivolutionCards(activateClass);

            bool SharedCanActivateCondition(Hashtable hashtable, ICardEffect activateClass)
                => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                    revealCount: 3,
                    simplifiedSelectCardConditions: new SimplifiedSelectCardConditionClass[]
                    {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition: CanSelectRevealCardCondition,
                            message: "Select 1 [Aqua]/[Sea Animal]/[DS] card to add to your hand.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                    },
                    remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                    activateClass: activateClass
                ));

                bool CanSelectTrashDigivolutionTargetConditionBound(Permanent permanent) => CanSelectTrashDigivolutionTargetCondition(permanent, activateClass);

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTrashDigivolutionTargetConditionBound))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectTrashDigivolutionTargetConditionBound));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectTrashDigivolutionTargetConditionBound,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will trash its bottom digivolution card.", "The opponent is selecting 1 Digimon that will trash its bottom digivolution card.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanent, trashCount: 1, isFromTop: false, activateClass: activateClass));
                    }
                }
            }

            #endregion

            #region When Moving
            if (timing == EffectTiming.OnMove)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateCondition(hash, activateClass), (hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("When Moving"));
                cardEffects.Add(activateClass);

                bool PermanentCondition(Permanent permanent)
                    => permanent == card.PermanentOfThisCard();

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnMove(hashtable, PermanentCondition);
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateCondition(hash, activateClass), (hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
            }
            #endregion

            #region Inherit - Jamming
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
