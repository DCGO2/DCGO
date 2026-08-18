using System;
using System.Collections;
using System.Collections.Generic;

// Ghostmon
namespace DCGO.CardEffects.BT26
{
    public class BT26_062 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("DemiMeramon") || (targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.IsLevel2 && targetPermanent.TopCard.ContainsTraits("NSo"));
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                cardEffects.Add(CardEffectFactory.StartOfYourMainPhaseClass(
                    card,
                    "By trashing 1 [Ghost]/[NSo] hand card, Draw 1 and gain 1 memory",
                    ActivateCoroutine,
                    EffectDescription(),
                    additionalActivateCondition: AdditionalActivateCondition,
                    optional: false,
                    isSkippable: true
                ));

                string EffectDescription()
                    => "[Start of Your Main Phase] By trashing 1 card with the [Ghost] or [NSo] trait from your hand, <Draw 1> and gain 1 memory.";

                bool CanSelectHandCardCondition(CardSource cardSource)
                    => cardSource.ContainsTraits("Ghost") || cardSource.ContainsTraits("NSo");

                bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                    => CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectHandCardCondition);

                IEnumerator ActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectHandCardCondition))
                    {
                        CardSource selectedCardToTrash = null;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectHandCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCardToTrash = cardSource;
                            yield return null;
                        }

                        selectHandEffect.SetUpCustomMessage("Select 1 [Ghost]/[NSo] card to trash.", "The opponent is selecting 1 [Ghost]/[NSo] card to trash.");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                        if (selectedCardToTrash != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashHandAndProcessAccordingToResult(
                                player: card.Owner,
                                hashtable: hashtable,
                                cardToTrash: selectedCardToTrash,
                                activateClass: activateClass,
                                successProcess: SuccessProcess,
                                failureProcess: null));

                            IEnumerator SuccessProcess(CardSource cardSource)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                            }
                        }
                    }
                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 2000,
                    isInheritedEffect: true,
                    card: card,
                    condition: () => CardEffectCommons.IsOwnerTurn(card)));
            }
            #endregion

            return cardEffects;
        }
    }
}
