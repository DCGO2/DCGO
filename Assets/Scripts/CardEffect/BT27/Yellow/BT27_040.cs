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

            // [Chuumon], [Sukamon] or [Etemon] in the name
            bool HasChuumonSukamonEtemonName(CardSource cardSource)
            {
                return cardSource != null
                    && (cardSource.ContainsCardName("Chuumon")
                        || cardSource.ContainsCardName("Sukamon")
                        || cardSource.ContainsCardName("Etemon"));
            }

            // [Chuumon] or [Sukamon] in the name (Overclock condition)
            bool HasChuumonSukamonName(CardSource cardSource)
            {
                return cardSource != null
                    && (cardSource.ContainsCardName("Chuumon")
                        || cardSource.ContainsCardName("Sukamon"));
            }

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.ContainsCardName("Sukamon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 4));
            }
            #endregion

            #region Assembly -3
            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect("Assembly -3", CanUseCondition, card);
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

                        bool CanSelectCardCondition(CardSource sourceCard)
                        {
                            return sourceCard.IsDigimon
                                && sourceCard.HasLevel
                                && sourceCard.IsLevel4
                                && sourceCard.ContainsCardName("Sukamon");
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element: element,
                            CanTargetCondition_ByPreSelecetedList: null,
                            selectMessage: "Lv.4 Digimon card with [Sukamon] in its name",
                            elementCount: 1,
                            reduceCost: 3);

                        return assemblyCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Overclock ([Chuumon]/[Sukamon] in name)
            if (timing == EffectTiming.OnEndTurn)
            {
                // The shared Overclock helper only matches by trait, so this card's name-based version is built here.
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Overclock", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "<Overclock ([Chuumon]/[Sukamon] in name)> (At the end of your turn, by deleting 1 of your Tokens or other specified Digimon, this Digimon attacks a player without suspending.)";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent != card.PermanentOfThisCard()
                        && (permanent.IsToken || HasChuumonSukamonName(permanent.TopCard));
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;
                    bool isDeleted = false;

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                    targetPermanents: new List<Permanent>() { selectedPermanent },
                                    activateClass: activateClass,
                                    successProcess: _ => SuccessProcess(),
                                    failureProcess: null));

                            IEnumerator SuccessProcess()
                            {
                                isDeleted = true;
                                yield return null;
                            }

                            if (isDeleted)
                            {
                                Permanent attacker = card.PermanentOfThisCard();

                                if (attacker != null && attacker.CanAttack(activateClass, withoutTap: true))
                                {
                                    SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                    selectAttackEffect.SetUp(
                                        attacker: attacker,
                                        canAttackPlayerCondition: () => true,
                                        defenderCondition: _ => false,
                                        cardEffect: activateClass);

                                    selectAttackEffect.SetWithoutTap();
                                    selectAttackEffect.SetCanNotSelectNotAttack();

                                    yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Shared Security / On Play / When Digivolving
            string SharedEffectName = "1 opponent's Digimon gets -3K DP for each of your [Chuumon]/[Sukamon]/[Etemon] Digimon";

            string SharedEffectDescription(string tag) => $"[{tag}] To 1 of your opponent's Digimon, give -3000 DP for the turn for each of your Digimon with [Chuumon], [Sukamon] or [Etemon] in their names.";

            bool CanSelectOpponentDigimonCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            int NameMatchCount()
            {
                return card.Owner.GetBattleAreaDigimons().Count(permanent => HasChuumonSukamonEtemonName(permanent.TopCard));
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                int dpChange = -3000 * NameMatchCount();

                if (dpChange < 0 && CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentDigimonCondition))
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOpponentDigimonCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage($"Select 1 Digimon to get {dpChange} DP.", $"The opponent is selecting 1 Digimon to get {dpChange} DP.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: selectedPermanent,
                                changeValue: dpChange,
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass));
                    }
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    security: true,
                    onPlay: true,
                    whenDigivolving: true);
            #endregion

            #region Inherited Effect - De-Digivolve
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<De-Digivolve 1> 1 of your opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT27_040_Inherited");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When any of your other Digimon with [Chuumon], [Sukamon] or [Etemon] in their names are played, <De-Digivolve 1> 1 of your opponent's Digimon. (Trash the top card. You can't trash past level 3 cards.)";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent != card.PermanentOfThisCard()
                        && HasChuumonSukamonEtemonName(permanent.TopCard);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentDigimonCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOpponentDigimonCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Degenerate,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 opponent's Digimon to <De-Digivolve 1>.", "The opponent is selecting 1 Digimon to <De-Digivolve 1>.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
