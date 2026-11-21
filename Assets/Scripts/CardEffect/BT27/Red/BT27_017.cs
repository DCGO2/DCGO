using System.Collections;
using System.Collections.Generic;

// Spinomon
namespace DCGO.CardEffects.BT27
{
    public class BT27_017 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - [Stegomon] w/[Bio] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Stegomon")
                        && targetPermanent.TopCard.EqualsTraits("Bio");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 5, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Alternate Digivolution Requirement - Tamer w/[Kurata's Army] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.IsTamer
                        && targetPermanent.TopCard.EqualsTraits("Kurata's Army");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 7, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Security A. +1
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Armor Purge
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.ArmorPurgeEffect(card: card));
            }
            #endregion

            #region Engage
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.EngageSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName = "Delete 1 opponent's Digimon with as much DP as this Digimon or less";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Delete 1 of your opponent's Digimon with as much DP as this Digimon or less.";

            bool CanSelectDeleteCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.HasDP
                    && card.PermanentOfThisCard() != null
                    && card.PermanentOfThisCard().HasDP
                    && permanent.DP <= card.PermanentOfThisCard().DP;

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteCondition)) yield break;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectDeleteCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: null,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Destroy,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
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

            #region All Turns - Can't Digivolve
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                    => permanent == card.PermanentOfThisCard();

                cardEffects.Add(CardEffectFactory.CanNotDigivolveStaticEffect(
                    permanentCondition: PermanentCondition,
                    cardCondition: cardSource => true,
                    isInheritedEffect: false,
                    card: card,
                    condition: () => CardEffectCommons.IsExistOnBattleAreaDigimon(card),
                    effectName: "This Digimon can't digivolve"));
            }
            #endregion

            #region Opponent's Turn - Can't Attack Players
            if (timing == EffectTiming.None)
            {
                bool Condition()
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOpponentTurn(card)
                        && card.PermanentOfThisCard().IsSuspended;

                bool AttackerCondition(Permanent attacker)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(attacker, card);

                bool DefenderCondition(Permanent defender)
                    => defender == null;

                cardEffects.Add(CardEffectFactory.CanNotAttackStaticEffect(
                    attackerCondition: AttackerCondition,
                    defenderCondition: DefenderCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition,
                    effectName: "While suspended, your opponent's Digimon can't attack players"));
            }
            #endregion

            return cardEffects;
        }
    }
}
