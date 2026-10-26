using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Crowmon
namespace DCGO.CardEffects.BT27
{
    public class BT27_081 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - Lv.4 w/[DATA SQUAD] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasDataSquadTraits;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 4));
            }
            #endregion

            #region Shared On Play / When Digivolving / When Attacking - OPT

            string SharedEffectName = "If opponent has 6+ hand cards, they trash 1, then by trashing 2 Tamer face-down cards, may play or use 1 cost 5 or lower [DATA SQUAD] card from hand";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] If your opponent's hand has 6 or more cards, they trash 1 card in their hand. Then, by trashing 2 bottom face-down cards from under any of your Tamers, you may play or use 1 [DATA SQUAD] trait card with a play or use cost of 5 or less from your hand without paying the cost.";

            bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;

            bool TamerWithOneFaceDownSource(Permanent permanent, ICardEffect activateClass)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.DigivolutionCards.Any(FaceDownCards)
                    && !permanent.ImmuneFromStackTrashing(activateClass);

            bool TamerWith2OrMoreFaceDownSources(Permanent permanent, ICardEffect activateClass)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.DigivolutionCards.Count(FaceDownCards) >= 2
                    && !permanent.ImmuneFromStackTrashing(activateClass);

            bool CanPayFaceDownCost(ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionPermanent(permanent => TamerWith2OrMoreFaceDownSources(permanent, activateClass))
                    || CardEffectCommons.MatchConditionPermanentCount(permanent => TamerWithOneFaceDownSource(permanent, activateClass)) >= 2;

            bool CanSelectCardCondition(CardSource cardSource, ActivateClass activateClass)
                => cardSource.HasDataSquadTraits
                    && cardSource.GetCostItself <= 5
                    && CardEffectCommons.CanPlayOrUse(cardSource, activateClass);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Opponent trashes 1 hand card
                if (card.Owner.Enemy.HandCards.Count >= 6)
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
                #endregion

                #region By trashing 2 Tamer face-down cards, may play or use 1 [DATA SQUAD] card
                if (!CanPayFaceDownCost(activateClass)
                    || !CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => CanSelectCardCondition(cardSource, activateClass))) yield break;

                bool paid = false;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(permanent => TamerWithOneFaceDownSource(permanent, activateClass)));

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: permanent => TamerWithOneFaceDownSource(permanent, activateClass),
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: CanEndSelectCondition,
                    maxCount: maxCount,
                    canNoSelect: true,
                    canEndNotMax: true,
                    selectPermanentCoroutine: null,
                    afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select all Tamer(s) to trash bottom face-down cards from.", "The opponent is selecting Tamer(s) to trash bottom face-down cards from.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                bool CanEndSelectCondition(List<Permanent> permanents)
                {
                    return permanents.Count == 2
                        || (permanents.Count > 0 && permanents[0].DigivolutionCards.Count(FaceDownCards) >= 2);
                }

                IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                {
                    if (permanents.Count == 1)
                    {
                        paid = true;
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanents[0], trashCount: 2, isFromTop: false, activateClass: activateClass, cardCondition: FaceDownCards));
                    }
                    else if (permanents.Count == 2)
                    {
                        paid = true;
                        foreach (Permanent selectedPermanent in permanents)
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedPermanent, trashCount: 1, isFromTop: false, activateClass: activateClass, cardCondition: FaceDownCards));
                    }
                }

                if (!paid) yield break;

                CardSource selectedCard = null;

                SelectHandEffect selectPlayEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectPlayEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: cardSource => CanSelectCardCondition(cardSource, activateClass),
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

                selectPlayEffect.SetUpCustomMessage("Select 1 card to play/use.", "The opponent is selecting 1 card to play/use.");
                selectPlayEffect.SetUpCustomMessage_ShowCard("Played Card");

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCard = cardSource;
                    yield return null;
                }

                yield return ContinuousController.instance.StartCoroutine(selectPlayEffect.Activate());

                if (selectedCard == null) yield break;

                if (selectedCard.IsOption)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                        cardSources: new List<CardSource> { selectedCard },
                        activateClass: activateClass,
                        payCost: false,
                        root: SelectCardEffect.Root.Hand));
                }
                else
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: new List<CardSource> { selectedCard },
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Hand,
                        activateETB: true));
                }
                #endregion
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: 1,
                hashValue: "BT27_081_OP_WD_WA",
                onPlay: true,
                whenDigivolving: true,
                whenAttacking: true);

            #endregion

            #region On Deletion - ESS
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Avian]/[Bird]/[DATA SQUAD] card cost 5 or less from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Deletion] You may play 1 play cost 5 or lower card with [Avian] or [Bird] in any of its traits or the [DATA SQUAD] trait from your trash without paying the cost.";

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 5
                        && (cardSource.ContainsTraits("Avian") || cardSource.ContainsTraits("Bird") || cardSource.HasDataSquadTraits)
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.Trash);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.CanActivateOnDeletion(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                        canTargetCondition: CanSelectCardCondition,
                        root: SelectCardEffect.Root.Trash,
                        cardEffect: activateClass,
                        payCost: false));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
