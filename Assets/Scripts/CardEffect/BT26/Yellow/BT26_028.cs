using System.Collections;
using System.Collections.Generic;

// Medicmon
namespace DCGO.CardEffects.BT26
{
    public class BT26_028 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Stnd Appmon Alternative Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasStandardAppTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region App Fusion (Aidmon & Supplemon & Spamon)
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.AddAppfuseMethodByName(new List<string>() { "Aidmon", "Supplemon", "Spamon" }, card));
            }
            #endregion

            #region Barrier
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Detach
            if (timing == EffectTiming.WhenRemoveField)
            {
                bool CanSelectLinkCardCondition(CardSource cardSource)
                    => cardSource.EqualsTraits("Seven Code");

                cardEffects.Add(CardEffectFactory.DetachSelfEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null,
                    conditionString: "[Seven Code] trait",
                    cardCondition: CanSelectLinkCardCondition));
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
                                && cardSource.IsLevel3
                                && (cardSource.EqualsTraits("Life") || cardSource.EqualsTraits("System") || cardSource.EqualsTraits("Seven Code"));
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element: element,
                            CanTargetCondition_ByPreSelecetedList: null,
                            selectMessage: "1 level 3 [Life]/[System]/[Seven Code] trait Digimon card",
                            elementCount: 1,
                            reduceCost: 2);

                        return assemblyCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName()
                => "May link 1 level 3 [Life]/[System]/[Seven Code] Digimon from digivolution cards to this Digimon";

            string SharedEffectDescription(string tag)
                => $"[{tag}] You may link 1 level 3 Digimon card with the [Life], [System] or [Seven Code] trait from this Digimon's digivolution cards to this Digimon without paying the cost.";

            bool CanSelectSourceCardCondition(CardSource cardSource)
                => cardSource.IsDigimon
                    && cardSource.IsLevel3
                    && (cardSource.EqualsTraits("Life") || cardSource.EqualsTraits("System") || cardSource.EqualsTraits("Seven Code"))
                    && cardSource.CanLinkToTargetPermanent(card.PermanentOfThisCard(), false);

            bool SharedAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => card.PermanentOfThisCard().DigivolutionCards.Exists(CanSelectSourceCardCondition);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                CardSource selectedCard = null;

                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                selectCardEffect.SetUp(
                    canTargetCondition: CanSelectSourceCardCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    canNoSelect: () => true,
                    selectCardCoroutine: SelectCardCoroutine,
                    afterSelectCardCoroutine: null,
                    message: "Select 1 card from this Digimon's digivolution cards to link.",
                    maxCount: 1,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    mode: SelectCardEffect.Mode.Custom,
                    root: SelectCardEffect.Root.DigivolutionCards,
                    customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                    canLookReverseCard: true,
                    selectPlayer: card.Owner,
                    cardEffect: activateClass);

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCard = cardSource;
                    yield return null;
                }

                selectCardEffect.SetUpCustomMessage("Select 1 card from this Digimon's digivolution cards to link.", "The opponent is selecting 1 card to link.");
                selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");

                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                if (selectedCard != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddLinkCard(selectedCard, activateClass));
                }
            }

            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                isSkippable: true,
                additionalActivateCondition: SharedAdditionalActivateCondition,
                onPlay: true,
                whenDigivolving: true);

            #region Link Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasAppmonTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: PermanentCondition, linkCost: 3, card: card));
            }
            #endregion

            #region Link
            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.LinkEffect(card));
            }
            #endregion

            #region Link Effect - When Linking
            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent's Digimon can't activate [When Digivolving] effects and gets -3000 DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsLinkedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Linking] Until your opponent's turn ends, 1 of their Digimon can't activate [When Digivolving] effects and gets -3000 DP.";

                bool CanSelectPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerWhenLinking(hashtable, null, card)
                        && CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
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

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that can't activate [When Digivolving] effects and gets -3000 DP.", "The opponent is selecting 1 Digimon that can't activate [When Digivolving] effects and gets -3000 DP.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent selectedPermanent)
                        {
                            DisableEffectClass invalidationClass = new DisableEffectClass();
                            invalidationClass.SetUpICardEffect("Ignore [When Digivolving] Effect", CanUseConditionDebuff, card);
                            invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                            selectedPermanent.UntilOpponentTurnEndEffects.Add(_ => invalidationClass);

                            bool CanUseConditionDebuff(Hashtable hashtableDebuff)
                                => selectedPermanent.TopCard != null;

                            bool InvalidateCondition(ICardEffect cardEffect)
                                => selectedPermanent.TopCard != null
                                    && cardEffect != null
                                    && cardEffect.EffectSourceCard != null
                                    && isExistOnField(cardEffect.EffectSourceCard)
                                    && cardEffect.EffectSourceCard.PermanentOfThisCard() == selectedPermanent
                                    && cardEffect.IsWhenDigivolving
                                    && !selectedPermanent.TopCard.CanNotBeAffected(activateClass);

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: selectedPermanent, changeValue: -3000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
