using System.Collections;
using System.Collections.Generic;

// Kotemon
namespace DCGO.CardEffects.BT26
{
    public class BT26_008 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsTraits("Shambala") || targetPermanent.TopCard.EqualsTraits("TS");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 2));
            }
            #endregion

            #region Shared When Moving / On Play

            string SharedEffectName = "1 [Shambala]/[TS] Digimon gains Piercing and +3000 DP for the turn";

            string SharedEffectDescription(string tag)
                => $"[{tag}] 1 of your [Shambala] or [TS] trait Digimon gains <Piercing> and +3000 DP for the turn.";

            bool CanSelectPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && (permanent.TopCard.EqualsTraits("Shambala") || permanent.TopCard.EqualsTraits("TS"));

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.GainPierce(targetPermanent: permanent, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));

                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: 3000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                whenMoving: true,
                onPlay: true);

            #endregion

            #region Inherited Effect
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 2000, isInheritedEffect: true, card: card, condition: () => CardEffectCommons.IsOwnerTurn(card)));
            }
            #endregion

            return cardEffects;
        }
    }
}
