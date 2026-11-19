using System.Collections;
using System.Collections.Generic;

// Reptilimon
namespace DCGO.CardEffects.ST25
{
    public class ST25_04 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - Lv.4 w/[Gemmon] in text
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasText("Gemmon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 4));
            }
            #endregion

            #region Engage
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.EngageSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "1 of your Digimon gets +3000 DP, then if it has 12000 DP or more, it gains <Security A. +1>";

            string SharedEffectDescription(string tag)
                => $"[{tag}] 1 of your Digimon gets +3000 DP for the turn. Then, if that Digimon has 12000 DP or more, it also gains <Security A. +1>.";

            bool OwnDigimonCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (!CardEffectCommons.HasMatchConditionPermanent(OwnDigimonCondition)) yield break;

                Permanent selectedPermanent = null;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: OwnDigimonCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP +3000.", "The opponent is selecting 1 Digimon that will get DP +3000.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    selectedPermanent = permanent;
                    yield return null;
                }

                if (selectedPermanent == null) yield break;

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                    targetPermanent: selectedPermanent, changeValue: 3000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));

                if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent)
                    && selectedPermanent.HasDP
                    && selectedPermanent.DP >= 12000)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(
                        targetPermanent: selectedPermanent, changeValue: 1, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);
            #endregion

            #region Your Turn - Inherited - [Gemmon] in text gains Engage
            if (timing == EffectTiming.OnEndTurn)
            {
                bool Condition()
                    => CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.PermanentOfThisCard().TopCard.HasText("Gemmon");

                cardEffects.Add(CardEffectFactory.EngageSelfStaticEffect(isInheritedEffect: true, card: card, condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}
