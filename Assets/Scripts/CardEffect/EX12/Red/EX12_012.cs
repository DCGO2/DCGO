using System.Collections;
using System.Collections.Generic;

// Apemon
namespace DCGO.CardEffects.EX12
{
    public class EX12_012 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Shambala");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 3));
            }
            #endregion

            #region Raid
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Start of Your Main Phase
            string SharedEffectName = "Trash 1 [SW] card to <Draw 2>";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    additionalActivateCondition: AdditionalActivateCondition,
                    optional: false,
                    onPlay: true,
                    whenDigivolving: true);

            string SharedEffectDescription(string tag) => $"[{tag}] By trashing 1 card with the [SW] trait from your hand, <Draw 2>.";

            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.EqualsTraits("Shambala");
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

                yield return StartCoroutine(selectHandEffect.Activate());

                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                {
                    if (cardSources.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 2, activateClass).Draw());
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
