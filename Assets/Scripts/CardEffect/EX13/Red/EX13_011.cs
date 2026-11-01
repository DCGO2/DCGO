using System.Collections;
using System.Collections.Generic;

// BaoHuckmon
namespace DCGO.CardEffects.EX13
{
    public class EX13_011 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasText("Huckmon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 3));
            }
            #endregion

            #region Raid
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string PlayMonEffectName = "If 1 or fewer Tamers, may play 1 [Mon] from hand for free";

            string PlayMonEffectDescription(string tag)
                => $"[{tag}] If you have 1 or fewer Tamers, you may play 1 [Mon] from your hand without paying the cost.";

            bool IsPlayableMon(CardSource cardSource, ActivateClass activateClass)
                => cardSource.EqualsCardName("Mon")
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);

            bool PlayMonAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.OwnerHas1OrLessTamers(card)
                    && CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => IsPlayableMon(cardSource, activateClass));

            IEnumerator PlayMonActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                    canTargetCondition: cardSource => IsPlayableMon(cardSource, activateClass),
                    root: SelectCardEffect.Root.Hand,
                    cardEffect: activateClass,
                    payCost: false));
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                PlayMonEffectName,
                PlayMonActivateCoroutine,
                PlayMonEffectDescription,
                optional: false,
                isSkippable: true,
                additionalActivateCondition: PlayMonAdditionalActivateCondition,
                onPlay: true,
                whenDigivolving: true);

            #endregion

            #region Your Turn - ESS
            if (timing == EffectTiming.None)
            {
                bool Condition()
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card);

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
