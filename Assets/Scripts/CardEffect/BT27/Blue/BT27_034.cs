using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Examon (X Antibody) // Ultimate Battle Dragonlance Ambrose
namespace DCGO.CardEffects.BT27
{
    public class BT27_034 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Side
            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Examon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Raid
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Security A. +1
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Evade
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.EvadeSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Decode <Green Lv.5>
            if (timing == EffectTiming.WhenRemoveField)
            {
                bool SourceCondition(CardSource source)
                    => source.CardColors.Contains(CardColor.Green)
                        && source.IsLevel5;

                string[] decodeStrings = { "(Green Lv.5)", "Green Lv.5" };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(card: card, isInheritedEffect: false, decodeStrings: decodeStrings, sourceCondition: SourceCondition, condition: null));
            }
            #endregion

            #region Decode <Blue Lv.5>
            if (timing == EffectTiming.WhenRemoveField)
            {
                bool SourceCondition(CardSource source)
                    => source.CardColors.Contains(CardColor.Blue)
                        && source.IsLevel5;

                string[] decodeStrings = { "(Blue Lv.5)", "Blue Lv.5" };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(card: card, isInheritedEffect: false, decodeStrings: decodeStrings, sourceCondition: SourceCondition, condition: null));
            }
            #endregion

            #region Shared When Digivolving / When Attacking / Counter - OPT

            string SharedEffectName = "May suspend up to 3 Digimon/Tamers, then by trashing 1 [Examon]/[X Antibody] source, activate 1 [Main] effect on the Option side";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] You may suspend up to 3 Digimon or Tamers. Then, by trashing 1 [Examon] or [X Antibody] from this Digimon's digivolution cards, activate 1 [Main] effect on this card's Option side.";

            bool CanSelectSuspendCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnBattleArea(permanent)
                    && (permanent.IsDigimon || permanent.IsTamer);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool isUsed = false;

                bool CanSelectSource(CardSource cardSource)
                    => (cardSource.EqualsCardName("Examon") || cardSource.EqualsCardName("X Antibody"))
                        && !cardSource.CanNotTrashFromDigivolutionCards(activateClass);

                #region May suspend up to 3 Digimon or Tamers
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendCondition))
                {
                    SelectPermanentEffect selectSuspendEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    int maxCount = Math.Min(3, CardEffectCommons.MatchConditionPermanentCount(CanSelectSuspendCondition));

                    selectSuspendEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectSuspendCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: true,
                        selectPermanentCoroutine: SelectSuspendCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    selectSuspendEffect.SetUpCustomMessage("Select up to 3 Digimon or Tamers to suspend.", "The opponent is selecting up to 3 Digimon or Tamers to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectSuspendEffect.Activate());

                    IEnumerator SelectSuspendCoroutine(Permanent permanent)
                    {
                        if (permanent != null) isUsed = true;
                        yield return null;
                    }
                }
                #endregion

                #region By trashing 1 [Examon] or [X Antibody] source, activate [Main]
                Permanent thisPermanent = card.PermanentOfThisCard();

                if (thisPermanent != null
                    && !thisPermanent.ImmuneFromStackTrashing(activateClass)
                    && thisPermanent.DigivolutionCards.Any(CanSelectSource))
                {
                    CardSource selectedCard = null;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectSource,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 digivolution card to trash.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.DigivolutionCards,
                        customRootCardList: thisPermanent.DigivolutionCards,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to trash.", "The opponent is selecting 1 digivolution card to trash.");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    if (selectedCard != null)
                    {
                        isUsed = true;

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsAndProcessAccordingToResult(
                            targetPermanent: thisPermanent,
                            targetDigivolutionCards: new List<CardSource>() { selectedCard },
                            activateClass: activateClass,
                            successProcess: SuccessProcess,
                            failureProcess: null));

                        IEnumerator SuccessProcess(List<CardSource> cardSources)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ActivateMainOfOptionSide(card, activateClass));
                        }
                    }
                }
                #endregion

                if (!isUsed) activateClass.RemoveUse();
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: true,
                maxCountPerTurn: 1,
                hashValue: "BT27_034_WD_WA_C",
                whenDigivolving: true,
                whenAttacking: true,
                counter: true);

            #endregion
            #endregion

            #region Option Side
            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete up to 9000 DP total of opponent's Digimon, if none deleted trash 1 of their Options", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] Delete up to 9000 DP's total worth of your opponent's Digimon. For each of their Digimon and Tamers, add 3000 to this effect's DP deletion maximum. If this effect didn't delete, trash 1 of their Option cards in the battle area.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectOptionCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                        && permanent.IsOption;

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    int digimonAndTamerCount = card.Owner.Enemy.GetBattleAreaPermanents().Count(permanent => permanent.IsDigimon || permanent.IsTamer);
                    int maxTotalDP = card.Owner.MaxDP_DeleteEffect(9000 + 3000 * digimonAndTamerCount, activateClass);

                    bool CanSelectPermanentCondition(Permanent permanent)
                        => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                            && permanent.DP <= maxTotalDP;

                    bool CanTargetConditionByPreSelectedList(List<Permanent> permanents, Permanent permanent)
                        => permanents.Sum(selected => selected.DP) + permanent.DP <= maxTotalDP;

                    bool CanEndSelectCondition(List<Permanent> permanents)
                        => permanents.Count >= 1
                            && permanents.Sum(selected => selected.DP) <= maxTotalDP;

                    List<Permanent> deleteTargetPermanents = new List<Permanent>();

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: CanTargetConditionByPreSelectedList,
                            canEndSelectCondition: CanEndSelectCondition,
                            maxCount: CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition),
                            canNoSelect: false,
                            canEndNotMax: true,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select Digimon to delete.", "The opponent is selecting Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            deleteTargetPermanents = permanents.Clone();
                            yield return null;
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                        targetPermanents: deleteTargetPermanents,
                        activateClass: activateClass,
                        successProcess: null,
                        failureProcess: FailureProcess));

                    IEnumerator FailureProcess()
                    {
                        if (!CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectOptionCondition)) yield break;

                        SelectPermanentEffect selectOptionEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectOptionEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOptionCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectOptionEffect.SetUpCustomMessage("Select 1 Option card in the battle area to trash.", "The opponent is selecting 1 Option card in the battle area to trash.");

                        yield return ContinuousController.instance.StartCoroutine(selectOptionEffect.Activate());
                    }
                }
            }
            #endregion

            #region Arts Digivolution
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }
            #endregion
            #endregion

            return cardEffects;
        }
    }
}
