using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Piximon
namespace DCGO.CardEffects.BT26
{
    public class BT26_043 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsTraits("DM");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 4));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName()
                => "Suspend 1 opponent's Digimon/Tamer, then place deck top card as bottom digivolution card and stack up can't-unsuspend";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Suspend 1 of your opponent's Digimon or Tamers. Then, by placing your deck's top card face down as this Digimon's bottom digivolution card, for each of this Digimon's face-down digivolution cards, 1 of your opponent's Digimon or Tamers can't unsuspend until their turn ends.";

            bool CanSelectSuspendTargetCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer)
                    && !permanent.IsSuspended && permanent.CanSuspend;

            bool CanSelectCantUnsuspendCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer);

            bool SharedCanActivateCondition(Hashtable hashtable, ICardEffect activateClass)
                => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                    && CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendTargetCondition);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendTargetCondition))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectSuspendTargetCondition));

                    SelectPermanentEffect selectSuspendEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectSuspendEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectSuspendTargetCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    selectSuspendEffect.SetUpCustomMessage("Select 1 Digimon or Tamer to suspend.", "The opponent is selecting 1 Digimon or Tamer to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectSuspendEffect.Activate());
                }

                if (card.Owner.LibraryCards.Count >= 1 && card.PermanentOfThisCard() != null)
                {
                    CardSource topDeckCard = card.Owner.LibraryCards[0];

                    yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { topDeckCard }, activateClass, isFacedown: true));

                    int faceDownCount = card.PermanentOfThisCard().DigivolutionCards.Count(cs => cs.IsFaceDown);

                    for (int i = 0; i < faceDownCount; i++)
                    {
                        if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectCantUnsuspendCondition)) break;

                        SelectPermanentEffect selectCantUnsuspendEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectCantUnsuspendEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCantUnsuspendCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectCantUnsuspendEffect.SetUpCustomMessage("Select 1 Digimon or Tamer that can't unsuspend.", "The opponent is selecting 1 Digimon or Tamer that can't unsuspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectCantUnsuspendEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotUnsuspend(permanent, EffectDuration.UntilOpponentTurnEnd, activateClass, null, "Can't unsuspend"));
                        }
                    }
                }
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

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateCondition(hash, activateClass), (hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May suspend 1 opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] When any of your Digimon are played, you may suspend 1 of your opponent's Digimon.";

                bool PlayedDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool CanSelectSuspendTargetConditionInherit(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && !permanent.IsSuspended && permanent.CanSuspend;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PlayedDigimonCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendTargetConditionInherit);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectSuspendTargetConditionInherit));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectSuspendTargetConditionInherit,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to suspend.", "The opponent is selecting 1 Digimon to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
