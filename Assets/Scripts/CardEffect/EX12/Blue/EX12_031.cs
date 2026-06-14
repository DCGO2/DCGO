using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// MarineBullmon
namespace DCGO.CardEffects.EX12
{
    public class EX12_031 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Aquatic")
                        || targetPermanent.TopCard.EqualsTraits("Shambala");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 4)
                );
            }
            #endregion

            #region Decode
            if (timing == EffectTiming.WhenRemoveField)
            {
                bool SourceCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel
                        && cardSource.Level <= 4
                        && (cardSource.ContainsTraits("Aqua")
                            || cardSource.ContainsTraits("Sea Animal")
                            || cardSource.EqualsTraits("TB"));
                }

                string[] decodeStrings = { "(Lv.4 or lower w/[Aqua]/[Sea Animal] in any trait or w/[TB] trait)", "Level 4 or lower Digimon card with [Aqua]/[Sea Animal] in any trait or [TB] trait" };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(card: card, isInheritedEffect: false, decodeStrings: decodeStrings, sourceCondition: SourceCondition, condition: null));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "By placing 1 lvl 6 or lower [Aqua]/[Sea Animal]/[TB] card from hand under, bounce 1 enemy Digimon with 1 or less sources";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                additionalActivateCondition: AdditionalActivateCondition,
                optional: false,
                isSkippable: true,
                onPlay: true,
                whenDigivolving: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] By placing 1 level 6 or lower card with [Aqua] or [Sea Animal] in any of its traits or the [TB] trait from your hand as this Digimon's bottom digivolution card, return 1 of your opponent's Digimon with 1 or fewer digivolution cards to the hand.";
            }

            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.HasLevel
                    && cardSource.Level <= 6
                    && (cardSource.ContainsTraits("Aqua")
                        || cardSource.ContainsTraits("Sea Animal")
                        || cardSource.EqualsTraits("TB"));
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.DigivolutionCards.Count() <= 1;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                CardSource selectedCard = null;
                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectCardCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    selectCardCoroutine: SelectCardCoroutine,
                    afterSelectCardCoroutine: null,
                    mode: SelectHandEffect.Mode.Custom,
                    cardEffect: activateClass);

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCard = cardSource;
                    yield return null;
                }
                selectHandEffect.SetUpCustomMessage("Select 1 card to add as digivolution card,", "The opponent is selecting 1 card to add as digivolution card,");
                selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");
                yield return StartCoroutine(selectHandEffect.Activate());

                if (selectedCard != null)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    if (selectedPermanent != null && selectedCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass));

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect1.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Bounce,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());
                        }
                    }
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
                            return cardSource.IsDigimon
                                && cardSource.HasLevel
                                && cardSource.Level <= 4
                                && (cardSource.ContainsTraits("Aqua")
                                    || cardSource.ContainsTraits("Sea Animal")
                                    || cardSource.EqualsTraits("TB"));
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element: element,
                            CanTargetCondition_ByPreSelecetedList: null,
                            selectMessage: "Lv.4 or lower w/[Aqua]/[Sea Animal] in any trait or w/[TB] trait",
                            elementCount: 1,
                            reduceCost: 2);

                        return assemblyCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Inherited
            if (timing == EffectTiming.WhenRemoveField)
            {
                bool SourceCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel
                        && cardSource.Level <= 4
                        && (cardSource.ContainsTraits("Aqua")
                            || cardSource.ContainsTraits("Sea Animal")
                            || cardSource.EqualsTraits("TB"));
                }

                string[] decodeStrings = { "(Lv.4 or lower w/[Aqua]/[Sea Animal] in any trait or w/[TB] trait)", "Level 4 or lower Digimon card with [Aqua]/[Sea Animal] in any trait or [TB] trait" };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(card: card, isInheritedEffect: true, decodeStrings: decodeStrings, sourceCondition: SourceCondition, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
