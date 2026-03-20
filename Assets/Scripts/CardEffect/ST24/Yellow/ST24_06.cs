using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// RizeGreymon
namespace DCGO.CardEffects.ST24
{
    public class ST24_06 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("GeoGreymon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("DATA SQUAD");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 4, permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Shared OP/WD/WA
            string SharedEffectName = "- 5K DP to 1 enemy Digimon for turn, by trashing 2 bottom face-down cards from your Tamers, may play/use 1 [DATA SQUAD] card with a play/use cost of 5 or less from hand for free";

            string SharedEffectHash = "ST24_06_SharedEffect";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    maxCountPerTurn: 1,
                    hashValue: SharedEffectHash,
                    onPlay: true,
                    whenDigivolving: true,
                    whenAttacking: true);

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] 1 of your opponent's Digimon gets -5000 DP for the turn. Then, by trashing 2 bottom face-down cards from under any of your Tamers, you may play or use 1 [DATA SQUAD] trait card with a play cost or use cost of 5 or less from your hand without paying the cost.";

            bool CanSelectPermanentConditionForDPMinus(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool TamerWithOneFaceDownSource(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.DigivolutionCards.Any(CanSelectTrashSourceCardCondition);
            }

            bool TamerWith2OrMoreFaceDownSources(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.DigivolutionCards.Count(CanSelectTrashSourceCardCondition) > 1;
            }

            bool CanSelectTrashSourceCardCondition(CardSource cardSource)
            {
                return cardSource.IsFlipped;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentConditionForDPMinus));

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectPermanentConditionForDPMinus,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get -5000 DP", "The opponent is selecting 1 Digimon that will get -5000 DP");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: -5000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                }

                if (CardEffectCommons.MatchConditionPermanentCount(TamerWithOneFaceDownSource) > 1 || CardEffectCommons.MatchConditionPermanentCount(TamerWith2OrMoreFaceDownSources) > 0)
                {
                    string selectPlayerMessage = "Will you trash 2 bottom face-down cards from under any of your Tamers?";
                    string notSelectPlayerMessage = "The opponent is choosing if they will trash 2 bottom face-down cards from under any of your Tamers.";

                    List<SelectionElement<bool>> command_SelectCommands = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Yes", value: true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"No", value: false, spriteIndex: 1),
                        };

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: command_SelectCommands, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool trash = GManager.instance.userSelectionManager.SelectedBoolValue;

                    if (trash)
                    {
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount1 = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(TamerWithOneFaceDownSource));

                        selectPermanentEffect1.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: TamerWithOneFaceDownSource,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: CanEndSelectCondition,
                            maxCount: maxCount1,
                            canNoSelect: false,
                            canEndNotMax: true,
                            selectPermanentCoroutine: SelectPermanentCoroutine1,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect1.SetUpCustomMessage("Select 1 Tamer to trash 1 bottom face-down card from", "The opponent is selecting 1 Tamer to trash 1 bottom face-down card from");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                        bool CanEndSelectCondition(List<Permanent> permanents)
                        {
                            return permanents.Count == 2 || (permanents.Count > 0 && permanents[0].DigivolutionCards.Count(CanSelectTrashSourceCardCondition) >= 2);
                        }

                        IEnumerator SelectPermanentCoroutine1(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            return null;
                        }

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            if (permanents.Count == 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanents[0], trashCount: 2, isFromTop: false, activateClass: activateClass, CanSelectTrashSourceCardCondition));
                            }
                            else
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedPermanent, trashCount: 1, isFromTop: false, activateClass: activateClass, CanSelectTrashSourceCardCondition));
                            }
                        }

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            return cardSource.EqualsTraits("DATA SQUAD")
                                && cardSource.GetCostItself <= 5
                                && ((cardSource.IsOption
                                && !cardSource.CanNotPlayThisOption)
                                    || (cardSource.HasPlayCost
                                && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass)));
                        }

                        CardSource selectCard = null;
                        List<CardSource> selectedCards = null;

                        int maxCount2 = Math.Min(1, card.Owner.HandCards.Count(CanSelectCardCondition));

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount2,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 card to play/use.", "The opponent is selecting 1 card to play/use.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectCard = cardSource;
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                        if (selectCard != null && selectCard.IsOption)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                                cardSources: selectedCards,
                                activateClass: activateClass,
                                payCost: false,
                                root: SelectCardEffect.Root.Hand));
                        }
                        else if (selectedCards[0] != null)
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
            #endregion

            #region Inherited
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash the bottom face-down card from 1 Tamer to not leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("ST24_06_Inherited");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon with [ShineGreymon] in its name or the [DATA SQUAD] trait would leave the battle area, by trashing the bottom face-down card from under any of your Tamers, it doesn't leave";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && (card.PermanentOfThisCard().TopCard.ContainsCardName("ShineGreymon")
                            || card.PermanentOfThisCard().TopCard.EqualsTraits("DATA SQUAD"))
                        && CardEffectCommons.HasMatchConditionPermanent(TamerWithOneFaceDownSource);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(TamerWithOneFaceDownSource))
                    {
                        Permanent thisCardPermanent = card.PermanentOfThisCard();
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: TamerWithOneFaceDownSource,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer to trash 1 bottom face-down card from", "The opponent is selecting 1 Tamer to trash 1 bottom face-down card from");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: new List<Permanent>() { permanent }[0], trashCount: 1, isFromTop: false, activateClass: activateClass, CanSelectTrashSourceCardCondition));

                            thisCardPermanent.willBeRemoveField = false;

                            thisCardPermanent.HideDeleteEffect();
                            thisCardPermanent.HideHandBounceEffect();
                            thisCardPermanent.HideDeckBounceEffect();
                            thisCardPermanent.HideWillRemoveFieldEffect();

                            yield return null;
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
