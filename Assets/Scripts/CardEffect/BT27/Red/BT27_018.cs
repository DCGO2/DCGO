using System.Collections;
using System.Collections.Generic;

// Rubeusdramon
namespace DCGO.CardEffects.BT27
{
    public class BT27_018 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - Lv.5 w/[Gemmon] in text
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasText("Gemmon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Assembly -5 - Lv.5 x Lv.4 x Lv.3, all red w/[Gemmon] in text
            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect("Assembly", _ => true, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool IsAssemblyCard(CardSource assemblyCard, int level)
                    => assemblyCard != null
                        && assemblyCard.Owner == card.Owner
                        && assemblyCard.IsDigimon
                        && assemblyCard.HasLevel
                        && assemblyCard.Level == level
                        && assemblyCard.HasCardColor(CardColor.Red)
                        && assemblyCard.HasText("Gemmon");

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource != card) return null;

                    return new AssemblyCondition(
                        elements: new List<AssemblyConditionElement>()
                        {
                            new AssemblyConditionElement(assemblyCard => IsAssemblyCard(assemblyCard, 5), selectMessage: "Lv.5 red w/[Gemmon] in text", elementCount: 1),
                            new AssemblyConditionElement(assemblyCard => IsAssemblyCard(assemblyCard, 4), selectMessage: "Lv.4 red w/[Gemmon] in text", elementCount: 1),
                            new AssemblyConditionElement(assemblyCard => IsAssemblyCard(assemblyCard, 3), selectMessage: "Lv.3 red w/[Gemmon] in text", elementCount: 1),
                        },
                        reduceCost: 5);
                }
            }
            #endregion

            #region Rush
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RushSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Progress
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ProgressSelfStaticEffect(false, card, null));
            }
            #endregion

            #region Shared WD/WA - OPT
            string SharedEffectName = "For every 8000 DP, may delete 1 of the opponent's lowest DP Digimon";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] For every 8000 DP this Digimon has, you may activate the effect below:\n・Delete 1 of your opponent's lowest DP Digimon.";

            bool LowestDPCondition(Permanent permanent)
                => CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent thisPermanent = card.PermanentOfThisCard();

                if (thisPermanent == null || !thisPermanent.HasDP) yield break;

                int activationCount = thisPermanent.DP / 8000;

                for (int i = 0; i < activationCount; i++)
                {
                    if (!CardEffectCommons.HasMatchConditionPermanent(LowestDPCondition)) yield break;

                    bool selected = false;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: LowestDPCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage(
                        $"Select 1 Digimon to delete ({i + 1}/{activationCount}).",
                        "The opponent is selecting 1 Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        selected = permanents.Count > 0;
                        yield return null;
                    }

                    // Declining one activation declines the rest
                    if (!selected) yield break;
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                isSkippable: true,
                maxCountPerTurn: 1,
                hashValue: "BT27_018_WD_WA",
                whenDigivolving: true,
                whenAttacking: true);
            #endregion

            #region All Turns - OPT - Opponent's Digimon deleted
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash the opponent's top security card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT27_018_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When any of your opponent's Digimon are deleted, trash their top security card.";

                bool OpponentDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, OpponentDigimonCondition, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && card.Owner.Enemy.SecurityCards.Count >= 1;

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
