using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Dragomon
namespace DCGO.CardEffects.BT25
{
    public class BT25_073 : CEntity_Effect
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

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 4, permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Jamming
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP / WD
            string SharedEffectName = "By trashing 1 linked card, may play/use 1 [TS] card with play/use cost of 5 or less from hand for free.";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    additionalActivateCondition: HasMatchingPermanent,
                    optional: false,
                    onPlay: true,
                    whenDigivolving: true);

            string SharedEffectDescription(string tag) => $"[{tag}] By trashing 1 of your Digimon's link cards, you may play or use 1 [TS] trait card with a play or use cost of 5 or less from your hand without paying the cost.";

            bool HasMatchingPermanent(Hashtable hashtable)
            {
                return CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && !permanent.HasNoLinkCards;
            }        

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent selectedPermanent = null;
                CardSource selectedCard = null;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectedPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to trash linked card from.", "The opponent is selecting 1 Digimon to trash linked card from.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectedPermanentCoroutine(Permanent permanent)
                {
                    selectedPermanent = permanent;
                    return null;
                }

                if (selectedPermanent != null)
                {
                    int maxCount = Math.Min(1, selectedPermanent.LinkedCards.Count);
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: _ => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 linked card to trash.",
                        maxCount: maxCount,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Custom,
                        customRootCardList: selectedPermanent.LinkedCards,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    selectCardEffect.SetUpCustomMessage("Select 1 linked card to trash.", "The opponent is selecting 1 linked card to trash.");
                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashLinkCardsAndProcessAccordingToResult(
                        targetPermanent: selectedPermanent,
                        targetLinkCards: new List<CardSource>() { selectedCard },
                        activateClass: activateClass,
                        successProcess: SuccessProcess,
                        failureProcess: null));

                    IEnumerator SuccessProcess(List<CardSource> trashedLinkCards)
                    {
                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            return cardSource.EqualsTraits("TS")
                                && cardSource.GetCostItself <= 5
                                && ((cardSource.IsOption
                                && !cardSource.CanNotPlayThisOption)
                                    || (cardSource.HasPlayCost
                                && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass)));
                        }

                        CardSource selectCard = null;
                        List<CardSource> selectedCards = null;

                        int maxCount = Math.Min(1, card.Owner.HandCards.Count(CanSelectCardCondition));

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 card to play/use.", "The opponent is selecting 1 card to play/use.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectCard = cardSource;
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                        if (selectCard != null)
                        {
                            if (selectCard.IsOption)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                                    cardSources: selectedCards,
                                    activateClass: activateClass,
                                    payCost: false,
                                    root: SelectCardEffect.Root.Hand));
                            }
                            else
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    new List<CardSource>() { selectCard },
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Hand,
                                    activateETB: true));
                            }
                        }
                    }
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 linked card to not leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When this Digimon would leave the battle area, by trashing 1 of its link cards, it doesn't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && !card.PermanentOfThisCard().HasNoLinkCards;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();
                    bool trashed = false;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                                canTargetCondition: _=> true,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: SelectCardCoroutine,
                                message: "Select 1 link card to trash.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Discard,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: card.PermanentOfThisCard().LinkedCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 link card to trash.", "The opponent is selecting 1 link card to trash.");

                    yield return StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count > 0)
                            trashed = true;

                        yield return null;
                    }

                    if (trashed)
                    {
                        thisPermanent.willBeRemoveField = false;

                        thisPermanent.HideHandBounceEffect();
                        thisPermanent.HideDeckBounceEffect();
                        thisPermanent.HideWillRemoveFieldEffect();
                        thisPermanent.HideDeleteEffect();
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
