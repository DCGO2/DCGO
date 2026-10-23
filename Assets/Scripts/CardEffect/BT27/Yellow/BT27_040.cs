using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Etemon
namespace DCGO.CardEffects.BT27
{
    public class BT27_040 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.ContainsCardName("Sukamon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, false, card, null, level: 4));
            }
            #endregion

            #region Overclock
            if (timing == EffectTiming.OnEndTurn)
            {
                bool PermanentCondition(Permanent permanent) => permanent.TopCard.ContainsCardNames("Chuumon", "Sukamon");

                cardEffects.Add(CardEffectFactory.OverclockSelfEffect(permanentCondition: PermanentCondition, isInheritedEffect: false, card: card,
                    condition: null, displayString: "[Chuumon]/[Sukamon] in name"));
            }
            #endregion

            #region Shared Condition
            bool IsYourPoopFamily(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.ContainsCardNames("Chuumon", "Sukamon", "Etemon");
            }
            
            bool IsOpponentsDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            #endregion

            #region Shared Security / OP / WD

            string SharedEffectName = "Reduce an enemy's DP based on number of [Chuumon], [Sukamon], [Etemon]";

            string SharedEffectDescription(string tag) => $"[{tag}] To 1 of your opponent's Digimon, give -3000 DP for the turn for each of your Digimon with [Chuumon], [Sukamon] or [Etemon] in their names";

            bool SharedIsSkippableFunction(Hashtable hashtable) => !CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon))
                {
                    int dpChange = CardEffectCommons.MatchConditionPermanentCount(IsYourPoopFamily) * -3000;
                    
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOpponentsDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage($"Choose a digimon to give {dpChange}DP to.", "Opponent is choosign a digimomn to reduce dp.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: dpChange, effectDuration: EffectDuration.UntilOwnerTurnEnd, activateClass: activateClass));
                    }
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                isSkippableFunction: SharedIsSkippableFunction,
                security: true,
                onPlay: true,
                whenDigivolving: true);

            #endregion

            #region Assembly
            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect("Assembly", CanUseCondition, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        AssemblyConditionElement element = new AssemblyConditionElement(CanSelectCardCondition);

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            return cardSource != null
                                && cardSource.Owner == card.Owner
                                && cardSource.IsDigimon
                                && cardSource.IsLevel4
                                && cardSource.ContainsCardName("Sukamon");
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element: element,
                            CanTargetCondition_ByPreSelecetedList: null,
                            selectMessage: "Lv. 4 Digimon Card w/ [Sukamon] in name",
                            elementCount: 1,
                            reduceCost: 3);

                        return assemblyCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<De-digivolve 1> 1 opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT27_040_AT_ESS");
                cardEffects.Add(activateClass);

                string EffectDescription() => "[All Turns] [Once Per Turn] When any of your other Digimon with [Chuumon], [Sukamon] or [Etemon] in their names are played, <De-Digivolve 1> 1 of your opponent's Digimon.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, IsYourPoopFamily);
                }

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOpponentsDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Degenerate,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
