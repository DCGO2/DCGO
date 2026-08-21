using System;
using System.Collections;
using System.Collections.Generic;

// Musyamon
namespace DCGO.CardEffects.BT26
{
    public class BT26_013 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Shambala") || targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 3));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared On Play / On Deletion

            string SharedEffectName()
                => "By trashing 1 hand card, delete 1 opponent's Digimon with 6000 DP or less";

            string SharedEffectDescription(string tag)
                => $"[{tag}] By trashing 1 card in your hand, delete 1 of your opponent's Digimon with 6000 DP or less.";

            bool CanSelectPermanentCondition(Permanent permanent, ICardEffect activateClass)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.DP <= card.Owner.MaxDP_DeleteEffect(6000, activateClass);

            bool SharedTargetCostCheck(Hashtable hashtable, ActivateClass activateClass)
                => card.Owner.HandCards.Count >= 1
                    && CardEffectCommons.HasMatchConditionPermanent(permanent => CanSelectPermanentCondition(permanent, activateClass));

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (card.Owner.HandCards.Count >= 1)
                {
                    CardSource selectedCardToTrash = null;

                    #region Select Hand Card to Trash

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: _ => true,
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

                    selectHandEffect.SetUpCustomMessage("Select 1 card to trash.", "The opponent is selecting 1 card to trash.");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    #endregion

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
                            bool CanSelectPermanentConditionBound(Permanent permanent) => CanSelectPermanentCondition(permanent, activateClass);

                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentConditionBound))
                            {
                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentConditionBound));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentConditionBound,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Destroy,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon with 6000 DP or less to delete.", "The opponent is selecting 1 Digimon with 6000 DP or less to delete.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                            }
                        }
                    }
                }
            }

            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                additionalActivateCondition: SharedTargetCostCheck,
                onPlay: true,
                onDeletion: true);

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
