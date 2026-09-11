using System.Collections;
using System.Collections.Generic;

// Wingdramon
namespace DCGO.CardEffects.EX13
{
    public class EX13_021 : CEntity_Effect
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

            #region Jamming
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName = "Trash bottom 2 digivolution cards of 1 opponent's Digimon, then 1 of their Digimon/Tamers can't suspend";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Trash the bottom 2 digivolution cards of 1 of your opponent's Digimon. Then, 1 of their Digimon or Tamers can't suspend until their turn ends.";

            bool IsOpponentDigimon(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            bool IsOpponentDigimonOrTamer(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentDigimon))
                {
                    SelectPermanentEffect trashSelectEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    trashSelectEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOpponentDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectTrashTargetCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    trashSelectEffect.SetUpCustomMessage(
                        "Select 1 Digimon that will trash its bottom 2 digivolution cards.",
                        "The opponent is selecting 1 Digimon that will trash its bottom 2 digivolution cards.");

                    yield return ContinuousController.instance.StartCoroutine(trashSelectEffect.Activate());

                    IEnumerator SelectTrashTargetCoroutine(Permanent trashTarget)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                            targetPermanent: trashTarget,
                            trashCount: 2,
                            isFromTop: false,
                            activateClass: activateClass));
                    }
                }

                if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentDigimonOrTamer))
                {
                    SelectPermanentEffect canNotSuspendSelectEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    canNotSuspendSelectEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOpponentDigimonOrTamer,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectCanNotSuspendCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    canNotSuspendSelectEffect.SetUpCustomMessage(
                        "Select 1 Digimon or Tamer that can't suspend until their turn ends.",
                        "The opponent is selecting 1 Digimon or Tamer that can't suspend until your turn ends.");

                    yield return ContinuousController.instance.StartCoroutine(canNotSuspendSelectEffect.Activate());

                    IEnumerator SelectCanNotSuspendCoroutine(Permanent selectedPermanent)
                    {
                        CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                        canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCanNotSuspendCondition, card);
                        canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: CanNotSuspendPermanentCondition);
                        selectedPermanent.UntilOwnerTurnEndEffects.Add(_ => canNotSuspendClass);

                        if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                        }

                        bool CanUseCanNotSuspendCondition(Hashtable effectHashtable)
                            => selectedPermanent.TopCard != null
                                && !selectedPermanent.TopCard.CanNotBeAffected(activateClass);

                        bool CanNotSuspendPermanentCondition(Permanent targetPermanent)
                            => targetPermanent == selectedPermanent;
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

            #region All Turns - Also treated as Lv.6 [Slayerdramon] for [Examon]'s DNA digivolution
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
                        && cardSource.CardNames.Contains("Examon"))
                    {
                        levels.Add(6);
                    }

                    return levels;
                }
            }

            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Also treated as [Slayerdramon]", CanUseCondition, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: ChangeCardNames);
                cardEffects.Add(changeCardNamesClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

                List<string> ChangeCardNames(CardSource cardSource, List<string> cardNames)
                {
                    if (cardSource == card)
                    {
                        cardNames.Add("Slayerdramon");
                    }

                    return cardNames;
                }
            }
            #endregion

            #region All Turns - ESS
            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon may unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("EX13_021_ESS_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When this Digimon with [Dracomon] or [Examon] in its text suspends, it may unsuspend.";

                bool IsThisDracomonOrExamonDigimon(Permanent permanent)
                    => permanent == card.PermanentOfThisCard()
                        && CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.HasText("Dracomon") || permanent.TopCard.HasText("Examon"));

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, IsThisDracomonOrExamonDigimon);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.PermanentOfThisCard().IsSuspended
                        && card.PermanentOfThisCard().CanUnsuspend;

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                        permanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        cardEffect: activateClass).Unsuspend());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
