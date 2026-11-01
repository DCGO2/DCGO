using System.Collections;
using System.Collections.Generic;

// Coredramon
namespace DCGO.CardEffects.EX13
{
    public class EX13_018 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.ContainsCardName("Dracomon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 3));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName = "Trash 1 [Dracomon]/[Examon] text card from hand, <Draw 2>";

            string SharedEffectDescription(string tag)
                => $"[{tag}] By trashing 1 card with [Dracomon] or [Examon] in its text from your hand, <Draw 2>.";

            bool CanSelectHandCardCondition(CardSource cardSource)
                => cardSource.HasText("Dracomon") || cardSource.HasText("Examon");

            bool SharedAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectHandCardCondition);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    canTargetCondition: CanSelectHandCardCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    canNoSelect: true,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: AfterSelectCardCoroutine,
                    maxCount: 1,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    mode: SelectHandEffect.Mode.Discard,
                    selectPlayer: card.Owner,
                    cardEffect: activateClass);

                selectHandEffect.SetUpCustomMessage("Select 1 card with [Dracomon] or [Examon] in its text to discard.",
                    "The opponent is selecting 1 card with [Dracomon] or [Examon] in its text to discard.");

                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                {
                    if (cardSources.Count > 0)
                        yield return ContinuousController.instance.StartCoroutine(
                            new DrawClass(card.Owner, 2, activateClass).Draw());
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                isSkippable: true,
                additionalActivateCondition: SharedAdditionalActivateCondition,
                onPlay: true,
                whenDigivolving: true);

            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [Examon] text Digimon for reduced cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Your Turn] When any of your other Digimon with [Dracomon] or [Examon] in their texts are played, this Digimon may digivolve into a Digimon card with [Examon] in its text in the hand with the cost reduced by 2.";

                bool PlayedPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent != card.PermanentOfThisCard()
                        && (permanent.TopCard.HasText("Dracomon") || permanent.TopCard.HasText("Examon"));

                bool DigivolveCardCondition(CardSource cardSource)
                    => cardSource.IsDigimon
                        && cardSource.HasText("Examon")
                        && cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, true, activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PlayedPermanentCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, DigivolveCardCondition);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: DigivolveCardCondition,
                        payCost: true,
                        reduceCostTuple: (reduceCost: 2, reduceCostCardCondition: null),
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: true,
                        activateClass: activateClass,
                        successProcess: null));
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
