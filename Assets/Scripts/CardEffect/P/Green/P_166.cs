using System.Collections;
using System.Collections.Generic;

// Galemon
namespace DCGO.CardEffects.P
{
    public class P_166 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared OP/WD
            string SharedEffectName = "May suspend 1 Digimon, then if your turn may digivolve into [Avian]/[Bird] in trait in hand for amount of suspended Digimon less";

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                isSkippableFunction: (Hashtable) => CardEffectCommons.IsOwnerTurn(card),
                isSkippable: true,
                onPlay: true,
                whenDigivolving: true);

            string SharedEffectDescription(string tag)
                => $"[{tag}] You may suspend 1 Digimon. Then, if it's your turn, this Digimon may digivolve into a Digimon card with [Avian] or [Bird] in any of its traits in the hand. For every other suspended Digimon, reduce this effect's digivolution cost by 1.";

            bool CanSelectSuspendCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent)
                    && !permanent.IsSuspended
                    && permanent.CanSuspend;
            }

            bool CanSelectDigivolveCardCondition(CardSource cardSource)
            {
                return cardSource.IsDigimon
                    && (cardSource.ContainsTraits("Avian")
                        || cardSource.ContainsTraits("Bird"));
            }

            bool SuspendedDigimonCountCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent)
                    && permanent.IsSuspended
                    && permanent != card.PermanentOfThisCard();
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectSuspendCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

                if (CardEffectCommons.IsOwnerTurn(card))
                {
                    int suspendedCount = CardEffectCommons.MatchConditionPermanentCount(SuspendedDigimonCountCondition);

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: CanSelectDigivolveCardCondition,
                        payCost: true,
                        reduceCostTuple: CardEffectCommons.HasMatchConditionPermanent(SuspendedDigimonCountCondition) ? (reduceCost: suspendedCount, reduceCostCardCondition: null) : null,
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: true,
                        activateClass: activateClass,
                        successProcess: null));
                }
            }
            #endregion

            #region Inherited Effect
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsOwnerTurn(card);
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 2000,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}