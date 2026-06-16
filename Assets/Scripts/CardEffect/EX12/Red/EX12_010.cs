using System.Collections;
using System.Collections.Generic;

// Greymon
namespace DCGO.CardEffects.EX12
{
    public class EX12_010 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Agumon")
                        || targetPermanent.TopCard.EqualsTraits("ME")
                        || targetPermanent.TopCard.EqualsTraits("VB");
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

            #region Raid
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP/WA
            string SharedEffectName = "May return 1 [Greymon] in name/[ME]/[VB] trait Digimon card from trash to hand";

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

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] You may return 1 Digimon card with [Greymon] in its name or the [ME] or [VB] trait from your trash to the hand.";
            }

            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.IsDigimon
                    && (cardSource.ContainsCardName("Greymon")
                        || cardSource.EqualsTraits("ME")
                        || cardSource.EqualsTraits("VB"));
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                selectCardEffect.SetUp(
                    canTargetCondition: CanSelectCardCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    canNoSelect: () => true,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: null,
                    message: "Select 1 card to add to your hand.",
                    maxCount: 1,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    mode: SelectCardEffect.Mode.AddHand,
                    root: SelectCardEffect.Root.Trash,
                    customRootCardList: null,
                    canLookReverseCard: true,
                    selectPlayer: card.Owner,
                    cardEffect: activateClass);

                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
            }
            #endregion

            #region Inherited Raid
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
