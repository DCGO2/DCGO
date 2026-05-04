using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Cougarmon
namespace DCGO.CardEffects.BT25
{
    public class BT25_035 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasGlowingDawnTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 3, permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Shared OP / WD
            string SharedEffectName = "- 3K DP to 1 enemy Digimon for turn, by trashing 2 bottom face-down cards from your Tamers, may digivolve into a [Glowing Dawn] Digimon for free";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    onPlay: true,
                    whenDigivolving: true);

            string SharedEffectDescription(string tag)
                => $"[{tag}] 1 of your opponent's Digimon gets -3000 DP for the turn. Then, by trashing 2 bottom face-down cards from under any of your Tamers, this Digimon may digivolve into a [Glowing Dawn] trait Digimon card in the hand without paying the cost.";

            bool IsEnemyDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            bool TamerWithOneFaceDownSource(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.DigivolutionCards.Any(CanSelectTrashSourceCardCondition);
            }

            bool TamerWith2OrMoreFaceDownSources(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.DigivolutionCards.Count(CanSelectTrashSourceCardCondition) > 1;
            }

            bool CanSelectTrashSourceCardCondition(CardSource cardSource)
            {
                return cardSource.IsFaceDown;
            }

            bool CanDigivolveCardCondition(CardSource cardSource) => cardSource.HasGlowingDawnTraits;

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(IsEnemyDigimon))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsEnemyDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get -3000 DP", "The opponent is selecting 1 Digimon that will get -3000 DP");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: -3000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                    }
                }

                if (CardEffectCommons.HasMatchConditionPermanent(TamerWith2OrMoreFaceDownSources) || CardEffectCommons.MatchConditionPermanentCount(TamerWithOneFaceDownSource) > 1)
                {
                    bool trashed = false;

                    SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();
                    int maxCount1 = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(TamerWithOneFaceDownSource));

                    selectPermanentEffect1.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: TamerWithOneFaceDownSource,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: CanEndSelectCondition,
                        maxCount: maxCount1,
                        canNoSelect: true,
                        canEndNotMax: true,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect1.SetUpCustomMessage("Select Tamer(s) to trash bottom face-down cards from", "The opponent is selecting Tamer(s) to trash bottom face-down cards from");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                    bool CanEndSelectCondition(List<Permanent> permanents)
                    {
                        return permanents.Count == 2 || (permanents.Count > 0 && permanents[0].DigivolutionCards.Count(CanSelectTrashSourceCardCondition) >= 2);
                    }

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        if (permanents.Count == 1)
                        {
                            trashed = true;
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanents[0], trashCount: 2, isFromTop: false, activateClass: activateClass, CanSelectTrashSourceCardCondition));
                        }
                        else if (permanents.Count == 2)
                        {
                            trashed = true;
                            foreach (Permanent selectedPermanent in permanents)
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedPermanent, trashCount: 1, isFromTop: false, activateClass: activateClass, CanSelectTrashSourceCardCondition));
                        }
                    }

                    if (trashed)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: CanDigivolveCardCondition,
                        payCost: false,
                        reduceCostTuple: null,
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: true,
                        activateClass: activateClass,
                        successProcess: null));
                    }
                }
            }
            #endregion

            #region Barrier - ESS
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(isInheritedEffect: true, card: card, condition: null));
            #endregion

            return cardEffects;
        }
    }
}
