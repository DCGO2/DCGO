using System.Collections;
using System.Collections.Generic;

// Veemon
namespace DCGO.CardEffects.P
{
    public class P_248 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("DemiVeemon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                cardEffects.Add(CardEffectFactory.StartOfYourMainPhaseClass(
                    card,
                    "Trash 1 [Veedramon] text/[Armor Form]/[Free] traits to <Draw 1> and gain 1 memory",
                    ActivateCoroutine,
                    EffectDescription(),
                    additionalActivateCondition: AdditionalActivateCondition,
                    optional: false,
                    isSkippable: true
                ));

                string EffectDescription()
                    => "[Start of Your Main Phase] By trashing 1 card with [Veedramon] in its text or the [Armor Form] or [Free] trait from your hand, <Draw 1> and gain 1 memory.";

                bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                    => CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.HasText("Veedramon")
                        || cardSource.EqualsTraits("Armor Form")
                        || cardSource.EqualsTraits("Free");

                IEnumerator ActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
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

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                            yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                        }
                    }
                }
            }
            #endregion

            #region Your Turn - ESS
            if (timing == EffectTiming.None)
            {
                bool Condition()
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card);

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 2000, isInheritedEffect: true, card: card, condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}
