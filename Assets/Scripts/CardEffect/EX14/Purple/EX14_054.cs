using System.Collections;
using System.Collections.Generic;

// Barbamon
namespace DCGO.CardEffects.EX14
{
    public class EX14_054 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasFallenAngelTraits || targetPermanent.TopCard.EqualsTraits("Greed");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
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

                bool CanUseCondition(Hashtable hashtable)
                    => true;

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        AssemblyConditionElement element = new AssemblyConditionElement(CanSelectCardCondition);

                        bool CanSelectCardCondition(CardSource cardSource)
                            => cardSource != null
                                && cardSource.Owner == card.Owner
                                && cardSource.IsDigimon
                                && cardSource.HasLevel
                                && cardSource.EqualsTraits("Greed");

                        bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                        {
                            List<int> cardLevels = new List<int>();

                            foreach (CardSource cardSource1 in cardSources)
                            {
                                if (!cardLevels.Contains(cardSource1.Level))
                                {
                                    cardLevels.Add(cardSource1.Level);
                                }
                                foreach (int level in cardSource1.Level_Assembly)
                                {
                                    if (!cardLevels.Contains(level))
                                    {
                                        cardLevels.Add(level);
                                    }
                                }
                            }

                            return !cardLevels.Contains(cardSource.Level)
                                && !cardSource.Level_Assembly.Some(level => cardLevels.Contains(level));
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element: element,
                            CanTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                            selectMessage: "3 [Greed] trait Digimon cards w/different levels",
                            elementCount: 3,
                            reduceCost: 5);

                        return assemblyCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Alliance
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Engage
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.EngageSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared On Play / When Digivolving / When Attacking

            string SharedEffectName()
                => "May play 1 play cost 6 or lower [Evil]/[Wizard]/[Fallen Angel]/[Greed] Digimon from trash, then opponent trashes 1 hand card";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] You may play 1 play cost 6 or lower [Evil], [Wizard], [Fallen Angel] or [Greed] trait Digimon card from your trash without paying the cost. Then, your opponent trashes 1 card in their hand.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool CanSelectTrashCardCondition(CardSource cardSource)
                    => cardSource.IsDigimon
                        && cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 6
                        && (cardSource.HasEvilTraits
                            || cardSource.HasWizardTraits
                            || cardSource.HasFallenAngelTraits
                            || cardSource.EqualsTraits("Greed"))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass, root: SelectCardEffect.Root.Trash);

                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectTrashCardCondition))
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                        canTargetCondition: CanSelectTrashCardCondition,
                        root: SelectCardEffect.Root.Trash,
                        cardEffect: activateClass,
                        payCost: false));
                }

                if (card.Owner.Enemy.HandCards.Count >= 1)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner.Enemy,
                        canTargetCondition: _ => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                }
            }

            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: 1,
                hashValue: "EX14_054_OP_WD_WA",
                onPlay: true,
                whenDigivolving: true,
                whenAttacking: true);

            #region All Turns
            if (timing == EffectTiming.OnDiscardHand)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May delete all opponent's lowest level Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("EX14_054_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When your opponent's hand is trashed from, you may delete all of their lowest level Digimon.";

                bool OpponentHandCardCondition(CardSource cardSource)
                    => cardSource.Owner == card.Owner.Enemy;

                bool CanSelectDeleteTargetCondition(Permanent permanent)
                    => CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnTrashHand(hashtable, null, OpponentHandCardCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteTargetCondition);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> deleteTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(CanSelectDeleteTargetCondition);
                    yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(deleteTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
