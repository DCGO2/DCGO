using System;
using System.Collections;
using System.Collections.Generic;

// Coronamon
namespace DCGO.CardEffects.BT25
{
    public class BT25_008 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolve Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 0, false, card, null, level: 2));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "May trash up to 2 cards to <Draw 1> same amount";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    isSkippable: true,
                    onPlay: true,
                    whenDigivolving: true);

            string SharedEffectDescription(string tag) => $"[{tag}] By trashing up to 2 [Iliad] or [TS] trait cards from your hand, <Draw 1> for each card trashed.";

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.HasIliadTraits
                        || cardSource.HasTSTraits;
            }

            IEnumerator SharedActivateCoroutine(Hashtable _hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                {
                    int maxCount = Math.Min(2, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, CanSelectCardCondition));

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: true,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        int drawCount = cardSources.Count;

                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(
                            player: card.Owner,
                            drawCount: drawCount,
                            cardEffect: activateClass).Draw()
                        );
                    }
                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card);
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
