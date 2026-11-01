using System.Collections;
using System.Collections.Generic;

// Sagomon
namespace DCGO.CardEffects.EX12
{
    public class EX12_029 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolve Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Shambala");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, false, card, null, level: 4));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None) cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            #endregion

            #region Shared OP / WD
            string SharedEffectName = "Stun 1 enemy Digimon/Tamer, then 1 of your other [SW] Digimon may gain <Alliance> and attack.";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    onPlay: true,
                    whenDigivolving: true);

            string SharedEffectDescription(string tag) => $"[{tag}] 1 of your opponent's Digimon or Tamers can't suspend until their turn ends. Then, 1 of your other [SW] trait Digimon may gain <Alliance> for the turn and attack.";

            bool CanSelectEnemyPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon
                        || permanent.IsTamer);
            }

            bool CanSelectOwnerPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.EqualsTraits("SW")
                    && permanent != card.PermanentOfThisCard();
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectEnemyPermanentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectEnemyPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or Tamer that will get unable to suspend.", "The opponent is selecting 1 Digimon or Tamer that will get unable to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        Permanent selectedPermanent = permanent;

                        if (selectedPermanent != null)
                        {
                            CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                            canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCondition1, card);
                            canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCondition);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => canNotSuspendClass);

                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                            }

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                return selectedPermanent.TopCard != null
                                    && !selectedPermanent.TopCard.CanNotBeAffected(activateClass);
                            }

                            bool PermanentCondition(Permanent permanent)
                            {
                                return permanent == selectedPermanent;
                            }
                        }
                    }
                }

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnerPermanentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOwnerPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to gain <Alliance> and attack", "The opponent is selecting 1 Digimon to gain <Alliance> and attack");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        if (permanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainAlliance(targetPermanent: permanent, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));

                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                            selectAttackEffect.SetUp(
                                attacker: permanent,
                                canAttackPlayerCondition: () => true,
                                defenderCondition: _ => true,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                        }
                    }
                }
            }
            #endregion

            #region DigiXros
            if (timing == EffectTiming.None)
            {
                AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
                addDigiXrosConditionClass.SetUpICardEffect($"DigiXros -2", CanUseCondition, card);
                addDigiXrosConditionClass.SetUpAddDigiXrosConditionClass(getDigiXrosCondition: GetDigiXros);
                addDigiXrosConditionClass.SetNotShowUI(true);
                cardEffects.Add(addDigiXrosConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                DigiXrosCondition GetDigiXros(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        DigiXrosConditionElement element = new DigiXrosConditionElement(CanSelectCardCondition,
                            "1 Lv.5 or lower Digimon card w/[Gokuumon] in text or w/[SW] trait");

                        bool CanSelectCardCondition(CardSource xrosCardSource)
                        {
                            return xrosCardSource != null
                                && xrosCardSource.Owner == card.Owner
                                && xrosCardSource.IsDigimon
                                && xrosCardSource.HasLevel
                                && xrosCardSource.Level <= 5
                                && (xrosCardSource.HasText("Gokuumon")
                                    || xrosCardSource.EqualsTraits("SW"));
                        }

                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>() { element };

                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, null, 2);

                        return digiXrosCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Inherited
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash bot 2 sources of 1 enemy Digimon, 1 of their sourceless Digimon can't suspend until their turn ends", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("EX12_029_Inherited");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] [Once Per Turn] Trash the bottom 2 digivolution cards of 1 of your opponent's Digimon. Then, 1 of their Digimon with no digivolution cards can't suspend until their turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool OpponentsDigimonWithoutSources(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.DigivolutionCards.Count == 0;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to trash digivolution cards from.",
                            "The opponent is selecting 1 Digimon to trash digivolution cards from.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                                targetPermanent: permanent,
                                trashCount: 2,
                                isFromTop: false,
                                activateClass: activateClass));
                        }
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimonWithoutSources))
                    {
                        SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect1.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentsDigimonWithoutSources,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect1.SetUpCustomMessage("Select 1 Digimon to gain can't suspend until end of opponent's turn.", "The opponent is selecting 1 Digimon that will gain can't suspend until end of opponent's turn.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotSuspendPlayerEffect(
                                permanentCondition: (permanent) => permanent == permanents[0],
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass,
                                isOnlyActivePhase: false,
                                effectName: "Can't Suspend"));
                        }
                    }

                }
            }
            #endregion

            return cardEffects;
        }
    }
}
