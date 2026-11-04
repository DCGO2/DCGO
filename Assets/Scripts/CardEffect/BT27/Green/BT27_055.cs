using System;
using System.Collections;
using System.Collections.Generic;

// Lotosmon
namespace DCGO.CardEffects.BT27
{
    public class BT27_055 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - [Quetzalmon] w/[Bio] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Quetzalmon")
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

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Vortex
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.VortexSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Armor Purge
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.ArmorPurgeEffect(card: card));
            }
            #endregion

            #region Shared When Digivolving / When Attacking - OPT
            string SharedEffectName = "Suspend up to 2 opponent's Digimon/Tamers, then 1 of their Digimon/Tamers can't unsuspend";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] Suspend up to 2 of your opponent's Digimon or Tamers. Then, 1 of their Digimon or Tamers can't unsuspend until their turn ends.";

            bool CanSelectOpponentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Suspend up to 2 of your opponent's Digimon or Tamers
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentCondition))
                {
                    SelectPermanentEffect selectSuspendEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CanSelectOpponentCondition));

                    selectSuspendEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOpponentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: true,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    selectSuspendEffect.SetUpCustomMessage("Select up to 2 Digimon or Tamers to suspend.", "The opponent is selecting up to 2 Digimon or Tamers to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectSuspendEffect.Activate());
                }
                #endregion

                #region 1 of their Digimon or Tamers can't unsuspend
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentCondition))
                {
                    SelectPermanentEffect selectCantUnsuspendEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectCantUnsuspendEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOpponentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectCantUnsuspendCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectCantUnsuspendEffect.SetUpCustomMessage("Select 1 Digimon or Tamer that can't unsuspend.", "The opponent is selecting 1 Digimon or Tamer that can't unsuspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectCantUnsuspendEffect.Activate());

                    IEnumerator SelectCantUnsuspendCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantUnsuspendUntilOpponentTurnEnd(permanent, activateClass));
                    }
                }
                #endregion
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: 1,
                hashValue: "BT27_055_WD_WA",
                whenDigivolving: true,
                whenAttacking: true);
            #endregion

            #region All Turns - OPT
            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon may unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT27_055_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When any Digimon suspend, this Digimon may unsuspend.";

                bool SuspendedDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, SuspendedDigimonCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && card.PermanentOfThisCard().IsSuspended
                        && card.PermanentOfThisCard().CanUnsuspend;

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                        new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass).Unsuspend());
                }
            }
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

            return cardEffects;
        }
    }
}
