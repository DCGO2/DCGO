using System.Collections;
using System.Collections.Generic;

// Yukinamon
namespace DCGO.CardEffects.BT27
{
    public class BT27_033 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Assembly -5 - Lv.5 x Lv.4 x Lv.3, all w/[Ice-Snow] trait
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
                        && assemblyCard.EqualsTraits("Ice-Snow");

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource != card) return null;

                    return new AssemblyCondition(
                        elements: new List<AssemblyConditionElement>()
                        {
                            new AssemblyConditionElement(assemblyCard => IsAssemblyCard(assemblyCard, 5), selectMessage: "Lv.5 w/[Ice-Snow] trait", elementCount: 1),
                            new AssemblyConditionElement(assemblyCard => IsAssemblyCard(assemblyCard, 4), selectMessage: "Lv.4 w/[Ice-Snow] trait", elementCount: 1),
                            new AssemblyConditionElement(assemblyCard => IsAssemblyCard(assemblyCard, 3), selectMessage: "Lv.3 w/[Ice-Snow] trait", elementCount: 1),
                        },
                        reduceCost: 5);
                }
            }
            #endregion

            #region Iceclad
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.IcecladSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "Trash the top 2 sources of all opponent's Digimon, then their sourceless Digimon can't digivolve or suspend";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Trash the top 2 digivolution cards of all of your opponent's Digimon. Then, until their turn ends, none of their Digimon without digivolution cards can digivolve or suspend.";

            bool OpponentDigimonCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            bool OpponentSourcelessDigimonCondition(Permanent permanent)
                => OpponentDigimonCondition(permanent)
                    && permanent.DigivolutionCards.Count == 0;

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaDigimons().Filter(OpponentDigimonCondition))
                {
                    if (permanent.DigivolutionCards.Count == 0) continue;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                        targetPermanent: permanent, trashCount: 2, isFromTop: true, activateClass: activateClass));
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotDigivolvePlayerEffect(
                    permanentCondition: OpponentSourcelessDigimonCondition,
                    cardCondition: null,
                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                    activateClass: activateClass,
                    isOnlyActivePhase: false,
                    effectName: "Digimon without digivolution cards can't digivolve"));

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotSuspendPlayerEffect(
                    permanentCondition: OpponentSourcelessDigimonCondition,
                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                    activateClass: activateClass,
                    isOnlyActivePhase: false,
                    effectName: "Digimon without digivolution cards can't suspend"));
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

            #region End of Your Turn - OPT
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 of your [Ice-Snow] Digimon may gain <Rush> and attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("BT27_033_EOT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[End of Your Turn] [Once Per Turn] If your opponent has no Digimon with digivolution cards, 1 of your [Ice-Snow] trait Digimon may gain <Rush> for the turn and attack.";

                bool OpponentDigimonWithSourcesCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.DigivolutionCards.Count >= 1;

                bool IceSnowDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsTraits("Ice-Snow")
                        && !permanent.IsSuspended;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && !CardEffectCommons.HasMatchConditionPermanent(OpponentDigimonWithSourcesCondition)
                        && CardEffectCommons.HasMatchConditionPermanent(IceSnowDigimonCondition);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IceSnowDigimonCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 [Ice-Snow] Digimon that will gain <Rush> and attack.", "The opponent is selecting 1 Digimon that will gain <Rush> and attack.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    if (selectedPermanent == null)
                    {
                        activateClass.RemoveUse();
                        yield break;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRush(
                        targetPermanent: selectedPermanent, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));

                    if (!CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent) || !selectedPermanent.CanAttack(activateClass)) yield break;

                    SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                    selectAttackEffect.SetUp(
                        attacker: selectedPermanent,
                        canAttackPlayerCondition: () => true,
                        defenderCondition: _ => true,
                        cardEffect: activateClass);

                    selectAttackEffect.SetCanNotSelectNotAttack();

                    yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
