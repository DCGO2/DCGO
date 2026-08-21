using System;
using System.Collections;
using System.Collections.Generic;

// Junomon: Hysteric Mode
namespace DCGO.CardEffects.BT26
{
    public class BT26_083 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("TS");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 6));
            }
            #endregion

            #region Rush
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RushSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Execute
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.ExecuteSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Decode
            if (timing == EffectTiming.WhenRemoveField)
            {
                bool SourceCondition(CardSource cardSource)
                    => cardSource.IsDigimon
                        && (cardSource.EqualsCardName("Junomon")
                            || (cardSource.HasLevel && cardSource.Level <= 5 && cardSource.EqualsTraits("Iliad")));

                string[] decodeStrings = { "(w/[Junomon] in name or Lv.5 or lower w/[Iliad] trait)", "Digimon card with [Junomon] in its name or a level 5 or lower Digimon card with the [Iliad] trait" };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(card: card, isInheritedEffect: false, decodeStrings: decodeStrings, sourceCondition: SourceCondition, condition: null));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName() => "Trash all security, delete that many opponent Digimon, then Recovery +3";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Trash all of your security cards. For each card this effect trashed, delete 1 of your opponent's Digimon. Then, <Recovery +3>.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                int securityCount = card.Owner.SecurityCards.Count;

                if (securityCount >= 1)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner,
                        destroySecurityCount: securityCount,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                }

                bool CanSelectDeleteTargetCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                int deleteCount = Math.Min(securityCount, CardEffectCommons.MatchConditionPermanentCount(CanSelectDeleteTargetCondition));

                if (deleteCount >= 1)
                {
                    SelectPermanentEffect selectDeleteEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectDeleteEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectDeleteTargetCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: deleteCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    selectDeleteEffect.SetUpCustomMessage($"Select {deleteCount} Digimon to delete.", "The opponent is selecting Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectDeleteEffect.Activate());
                }

                yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 3, activateClass).Recovery());
            }

            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent's Digimon get Security A. -1 until their turn ends", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Deletion] Give all of your opponent's Digimon <Security A. -1> until their turn ends.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.CanActivateOnDeletion(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool PermanentCondition(Permanent permanent)
                        => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttackPlayerEffect(
                        permanentCondition: PermanentCondition,
                        changeValue: -1,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass));
                }
            }
            #endregion

            #region Assembly
            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect("Assembly", CanUseCondition, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool CanUseCondition(Hashtable hashtable) => true;

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        AssemblyConditionElement element = new AssemblyConditionElement(CanSelectCardCondition);

                        bool CanSelectCardCondition(CardSource cs)
                            => cs.EqualsCardName("Junomon");

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element: element,
                            CanTargetCondition_ByPreSelecetedList: null,
                            selectMessage: "w/[Junomon] in name",
                            elementCount: 1,
                            reduceCost: 4);

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
