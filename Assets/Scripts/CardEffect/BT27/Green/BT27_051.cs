using System.Collections;
using System.Collections.Generic;

// LoaderLeomon — BT27-051
namespace DCGO.CardEffects.BT27
{
    public class BT27_051 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    CardSource top = targetPermanent.TopCard;
                    return top != null && top.IsLevel4 &&
                           (top.ContainsCardName("Leomon") ||
                            top.EqualsTraits("Holy Beast") ||
                            top.EqualsTraits("ACCEL"));
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region Fortitude
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.FortitudeSelfEffect(
                    isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region On Play / When Digivolving - Battle
            string BattleName() => "This Digimon may battle 1 of your opponent's Digimon";
            string BattleDescription(string tag)
                => $"[{tag}] This Digimon may battle 1 of your opponent's Digimon.";

            bool EligibleOpponent(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                   && permanent.HasDP;

            bool CanBattle(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionPermanent(EligibleOpponent);

            IEnumerator BattleCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent attacker = card.PermanentOfThisCard();
                if (attacker == null) yield break;

                SelectPermanentEffect selector = GManager.instance.GetComponent<SelectPermanentEffect>();
                selector.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: EligibleOpponent,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectedTarget,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);
                selector.SetUpCustomMessage("Select 1 Digimon to battle (or skip).",
                    "The opponent is choosing a Digimon to battle.");
                yield return ContinuousController.instance.StartCoroutine(selector.Activate());

                IEnumerator SelectedTarget(Permanent target)
                {
                    if (target != null && card.PermanentOfThisCard() != null)
                        yield return ContinuousController.instance.StartCoroutine(
                            new IBattle(card.PermanentOfThisCard(), target, null, true).Battle());
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                BattleName(), BattleCoroutine, BattleDescription,
                optional: false,
                isSkippable: true,
                additionalActivateCondition: CanBattle,
                onPlay: true,
                whenDigivolving: true);
            #endregion

            #region Main On Deletion - Recovery +1
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass recovery = new ActivateClass();
                recovery.SetUpICardEffect("Recovery +1 (On Deletion)",
                    hashtable => CardEffectCommons.CanTriggerOnDeletion(hashtable, card, recovery), card);
                recovery.SetUpActivateClass(
                    hashtable => CardEffectCommons.IsExistOnTrash(card)
                                 && card.Owner.LibraryCards.Count > 0
                                 && card.Owner.CanAddSecurity(recovery),
                    hashtable => Recover(recovery),
                    -1, false,
                    "[On Deletion] <Recovery +1>.");
                cardEffects.Add(recovery);
            }
            #endregion

            #region Inherited On Deletion - Recovery +1
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass inheritedRecovery = new ActivateClass();
                inheritedRecovery.SetUpICardEffect("Recovery +1 (Inherited On Deletion)",
                    hashtable => CardEffectCommons.CanTriggerOnDeletion(hashtable, card, inheritedRecovery), card);
                inheritedRecovery.SetIsInheritedEffect(true);
                inheritedRecovery.SetUpActivateClass(
                    hashtable => card.Owner.LibraryCards.Count > 0
                                 && card.Owner.CanAddSecurity(inheritedRecovery),
                    hashtable => Recover(inheritedRecovery),
                    -1, false,
                    "[On Deletion] <Recovery +1>.");
                cardEffects.Add(inheritedRecovery);
            }
            #endregion

            return cardEffects;

            IEnumerator Recover(ActivateClass effect)
            {
                yield return ContinuousController.instance.StartCoroutine(
                    new IRecovery(card.Owner, 1, effect).Recovery());
            }
        }
    }
}
