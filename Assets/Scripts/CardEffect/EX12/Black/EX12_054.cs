using System.Collections;
using System.Collections.Generic;

// Guardromon
namespace DCGO.CardEffects.EX12
{
    public class EX12_054 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("ME");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 3)
                );
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP / WD
            string SharedEffectName = "Trash 1 [Machine]/[Cyborg]/[ME] card to <Draw 2>";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    additionalActivateCondition: AdditionalActivateCondition,
                    optional: false,
                    isSkippable: true,
                    onPlay: true,
                    whenDigivolving: true);

            string SharedEffectDescription(string tag) => $"[{tag}] By trashing 1 card with the [Machine], [Cyborg] or [ME] trait from your hand, <Draw 2>.";
            
            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            { 
                return CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.EqualsTraits("Machine")
                    || cardSource.EqualsTraits("Cyborg")
                    || cardSource.EqualsTraits("ME");
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectCardCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: AfterSelectCardCoroutine,
                    mode: SelectHandEffect.Mode.Discard,
                    cardEffect: activateClass);

                selectHandEffect.SetUpCustomMessage("Select 1 card to trash.", "The opponent is selecting 1 card to trash from their hand.");

                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                {
                    if (cardSources.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 2, activateClass).Draw());
                    }
                }
            }
            #endregion

            #region Inherited Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
