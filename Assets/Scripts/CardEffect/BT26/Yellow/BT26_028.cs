using System;
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
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash a [Seven Code] link card to prevent this Digimon from leaving", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] When this Digimon would leave the battle area other than by your effects, by trashing 1 of its link cards with the [Seven Code] trait, it doesn't leave.";

                bool CanSelectLinkCardCondition(CardSource cardSource)
                    => cardSource.EqualsTraits("Seven Code");

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card)
                        && !CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOwnerEffect(cardEffect, card));

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.PermanentOfThisCard().LinkedCards.Exists(CanSelectLinkCardCondition);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectLinkCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: SelectCardCoroutine,
                        message: "Select 1 [Seven Code] link card to trash.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Discard,
                        root: SelectCardEffect.Root.LinkedCards,
                        customRootCardList: thisPermanent.LinkedCards,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 [Seven Code] link card to trash.", "The opponent is selecting 1 [Seven Code] link card to trash.");

                    yield return StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count > 0)
                        {
                            thisPermanent.willBeRemoveField = false;

                            thisPermanent.HideHandBounceEffect();
                            thisPermanent.HideDeckBounceEffect();
                            thisPermanent.HideWillRemoveFieldEffect();
                            thisPermanent.HideDeleteEffect();
                        }

                        yield return null;
                    }
                }
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
                if (card.PermanentOfThisCard().DigivolutionCards.Exists(CanSelectSourceCardCondition))
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
            }

            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
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
                    => CardEffectCommons.CanTriggerWhenLinking(hashtable, null, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
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
                            => selectedPermanent.TopCard != null
                                && !selectedPermanent.TopCard.CanNotBeAffected(activateClass);

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
            #endregion

            return cardEffects;
        }
    }
}
