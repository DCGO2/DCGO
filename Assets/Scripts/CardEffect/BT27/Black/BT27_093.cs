using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Miki Kurosaki & Megumi Shirakawa
namespace DCGO.CardEffects.BT27
{
    public class BT27_093 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 1 [Chessmon] text hand card face down under this Tamer, <Draw 1> and gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Start of Your Main Phase] By placing 1 card with [Chessmon] in its text from your hand face down under this Tamer, <Draw 1> and gain 1 memory.";

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.HasText("Chessmon");

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
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

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    if (selectedCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass, isFacedown: true));

                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                        if (card.Owner.CanAddMemory(activateClass)) yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                    }
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspending this Tamer and trashing 2 Tamer face-down cards, 1 Digimon may digivolve into [Chessmon] in name for 2 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] By suspending this Tamer and trashing 2 bottom face-down cards from under any of your Tamers, 1 of your Digimon may digivolve into a Digimon card with [Chessmon] in its name in the hand with the cost reduced by 2.";

                bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;

                bool TamerWithOneFaceDownSource(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.DigivolutionCards.Any(FaceDownCards)
                        && !permanent.ImmuneFromStackTrashing(activateClass);

                bool TamerWith2OrMoreFaceDownSources(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.DigivolutionCards.Count(FaceDownCards) >= 2
                        && !permanent.ImmuneFromStackTrashing(activateClass);

                bool CanPayFaceDownCost()
                    => CardEffectCommons.HasMatchConditionPermanent(TamerWith2OrMoreFaceDownSources)
                        || CardEffectCommons.MatchConditionPermanentCount(TamerWithOneFaceDownSource) >= 2;

                bool ChessmonCardCondition(CardSource cardSource)
                    => cardSource.IsDigimon
                        && cardSource.ContainsCardName("Chessmon");

                bool CanSelectDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card)
                        && CanPayFaceDownCost();

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool suspended = false;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass,
                        successProcess: SuspendSuccess,
                        failureProcess: null));

                    IEnumerator SuspendSuccess(List<Permanent> _permanents)
                    {
                        suspended = true;
                        yield return null;
                    }

                    if (!suspended || !CanPayFaceDownCost()) yield break;

                    #region Trash 2 bottom face-down cards
                    bool paid = false;

                    SelectPermanentEffect selectTamerEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(TamerWithOneFaceDownSource));

                    selectTamerEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: TamerWithOneFaceDownSource,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: CanEndSelectCondition,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: true,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectTamerEffect.SetUpCustomMessage("Select all Tamer(s) to trash bottom face-down cards from.", "The opponent is selecting Tamer(s) to trash bottom face-down cards from.");

                    yield return ContinuousController.instance.StartCoroutine(selectTamerEffect.Activate());

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

                    if (!suspended || !paid) yield break;
                    #endregion

                    #region 1 Digimon may digivolve
                    if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectDigimonCondition)
                        || !CardEffectCommons.HasMatchConditionOwnersHand(card, ChessmonCardCondition)) yield break;

                    Permanent selectedDigimon = null;

                    SelectPermanentEffect selectDigimonEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectDigimonEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectDigimonCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectDigimonCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectDigimonEffect.SetUpCustomMessage("Select 1 Digimon to digivolve.", "The opponent is selecting 1 Digimon to digivolve.");

                    yield return ContinuousController.instance.StartCoroutine(selectDigimonEffect.Activate());

                    IEnumerator SelectDigimonCoroutine(Permanent permanent)
                    {
                        selectedDigimon = permanent;
                        yield return null;
                    }

                    if (selectedDigimon != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: selectedDigimon,
                            cardCondition: ChessmonCardCondition,
                            payCost: true,
                            reduceCostTuple: (reduceCost: 2, reduceCostCardCondition: null),
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null));
                    }
                    #endregion
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}
