using System.Collections;
using System.Collections.Generic;

// Karakurumon
namespace DCGO.CardEffects.EX12
{
    public class EX12_063 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Cost
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return !targetPermanent.HasPermanentColor(CardColor.White) 
                        && (targetPermanent.TopCard.EqualsTraits("Puppet")
                            || targetPermanent.TopCard.EqualsTraits("Shambala"));
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 4
                    )
                );
            }
            #endregion

            #region Shared OP / WD

            string SharedEffectName = "Suspend 1 enemy Digimon/Tamer. 1 of their Digimon/Tamers can't unsuspend until their turn ends";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Suspend 1 of your opponent's Digimon or Tamers. Then, 1 of their Digimon or Tamers can't unsuspend until their turn ends.";

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
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
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

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

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or Tamer that will not unsuspend.", "The opponent is selecting 1 Digimon or Tamer that will not unsuspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        Permanent selectedPermanent = permanent;

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantUnsuspendUntilOpponentTurnEnd(
                                    targetPermanent: selectedPermanent,
                                    activateClass: activateClass
                                ));

                    }
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    onPlay: true,
                    whenDigivolving: true);

            #endregion

            #region Shared On Deletion
            
            string OnDeletionEffectName = "May play 1 Level 4 or less [Puppet]/[TB] from trash";

            string OnDeletionEffectDescription() => "[On Deletion] You may play 1 level 4 or lower [Puppet] or [TB] trait Digimon from your trash without paying the cost.";

            bool OnDeletionUseCondition(Hashtable hashtable, ActivateClass activateClass) => CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);

            bool OnDeletionActivateCondition(Hashtable hashtable, ActivateClass activateClass) => CardEffectCommons.CanActivateOnDeletion(card, activateClass);

            IEnumerator OnDeletionActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool CanPlayTrashDigimon(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel
                        && cardSource.Level <= 4
                        && (cardSource.EqualsTraits("Puppet") || cardSource.EqualsTraits("TB"))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }
                
                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanPlayTrashDigimon))
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                        canTargetCondition: CanPlayTrashDigimon,
                        SelectCardEffect.Root.Trash,
                        activateClass,
                        payCost: false
                    ));
                }
            }

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new();
                activateClass.SetUpICardEffect(OnDeletionEffectName, hash => OnDeletionUseCondition(hash, activateClass), card);
                activateClass.SetUpActivateClass(hash => OnDeletionActivateCondition(hash, activateClass), hash => OnDeletionActivateCoroutine(hash, activateClass), -1, false, OnDeletionEffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);
            }

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new();
                activateClass.SetUpICardEffect(OnDeletionEffectName, hash => OnDeletionUseCondition(hash, activateClass), card);
                activateClass.SetUpActivateClass(hash => OnDeletionActivateCondition(hash, activateClass), hash => OnDeletionActivateCoroutine(hash, activateClass), -1, false, OnDeletionEffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);
            }
            #endregion
            
            #region Assembly
            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect($"Assembly", CanUseCondition, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool CanUseCondition(Hashtable hashtable) => true;

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        AssemblyConditionElement element = new AssemblyConditionElement(CanSelectCardCondition);

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            return cardSource != null
                                && cardSource.Owner == card.Owner
                                && cardSource.HasLevel
                                && cardSource.Level <= 4
                                && (cardSource.EqualsTraits("Puppet")
                                    || cardSource.EqualsTraits("TB"));
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element: element,
                            CanTargetCondition_ByPreSelecetedList: null,
                            selectMessage: "Lv. 4 or Lower [Puppet]/[TB] trait card",
                            elementCount: 1,
                            reduceCost: 2);

                        return assemblyCondition;
                    }

                    return null;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
