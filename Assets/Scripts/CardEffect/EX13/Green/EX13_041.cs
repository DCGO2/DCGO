using System.Collections;
using System.Collections.Generic;

// Groundramon
namespace DCGO.CardEffects.EX13
{
    public class EX13_041 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Coredramon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Fortitude
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.FortitudeSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName = "Suspend 1 opponent's Digimon/Tamer, then 1 of their Digimon/Tamers can't unsuspend";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Suspend 1 of your opponent's Digimon or Tamers. Then, 1 of their Digimon or Tamers can't unsuspend in their next unsuspend phase.";

            bool CanSelectOpponentPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentCondition))
                {
                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOpponentPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or Tamer to suspend.", "The opponent is selecting 1 Digimon or Tamer to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentCondition))
                {
                    Permanent selectedPermanent = null;

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOpponentPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 Digimon or Tamer that can't unsuspend in its next unsuspend phase.",
                        "The opponent is selecting 1 Digimon or Tamer that can't unsuspend in its next unsuspend phase.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        bool CanNotUnsuspendPermanentCondition(Permanent permanent)
                            => permanent == selectedPermanent;

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotUnsuspendPlayerEffect(
                            permanentCondition: CanNotUnsuspendPermanentCondition,
                            effectDuration: EffectDuration.UntilOwnerActivePhase,
                            activateClass: activateClass,
                            isOnlyActivePhase: true,
                            effectName: "Can't unsuspend"));
                    }
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

            #region All Turns - Also treated as Lv.6 [Breakdramon] for [Examon]'s DNA digivolution
            if (timing == EffectTiming.None)
            {
                AddJogressLevelsClass addJogressLevelsClass = new AddJogressLevelsClass();
                addJogressLevelsClass.SetUpICardEffect("Also treated as level 6 for DNA Digivolution", CanUseCondition, card);
                addJogressLevelsClass.SetUpAddJogressLevelsClass(AddJogressLevels);
                cardEffects.Add(addJogressLevelsClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

                List<int> AddJogressLevels(CardSource cardSource, Permanent permanent)
                {
                    List<int> levels = new List<int>();

                    if (permanent == card.PermanentOfThisCard()
                        && cardSource != null
                        && cardSource.Owner == card.Owner
                        && cardSource.Owner.HandCards.Contains(cardSource)
                        && cardSource.EqualsCardName("Examon"))
                    {
                        levels.Add(6);
                    }

                    return levels;
                }
            }

            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Also treated as [Breakdramon]", CanUseCondition, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: ChangeCardNames);
                cardEffects.Add(changeCardNamesClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

                List<string> ChangeCardNames(CardSource cardSource, List<string> cardNames)
                {
                    if (cardSource == card)
                    {
                        cardNames.Add("Breakdramon");
                    }

                    return cardNames;
                }
            }
            #endregion

            #region All Turns - ESS
            if (timing == EffectTiming.OnEndBattle)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash opponent's top security card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("EX13_041_ESS_TrashSecurity");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When any of your Digimon with [Dracomon] or [Examon] in their texts delete your opponent's Digimon in battle, trash their top security card.";

                // Battle hashtable winner copies have their card order reversed, so check text on the real winners' top cards
                bool WinnerCondition(Permanent permanent)
                    => CardEffectCommons.IsOwnerPermanent(permanent, card);

                bool WinnerRealCondition(Permanent permanent)
                    => CardEffectCommons.IsOwnerPermanent(permanent, card)
                        && (permanent.TopCard.HasText("Dracomon") || permanent.TopCard.HasText("Examon"));

                bool LoserCondition(Permanent permanent)
                    => CardEffectCommons.IsOpponentPermanent(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDeleteOpponentDigimonByBattle(
                            hashtable: hashtable, winnerCondition: WinnerCondition, loserCondition: LoserCondition, isOnlyWinnerSurvive: false,
                            winnerRealCondition: WinnerRealCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.Enemy.SecurityCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner.Enemy,
                            destroySecurityCount: 1,
                            cardEffect: activateClass,
                            fromTop: true).DestroySecurity());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
