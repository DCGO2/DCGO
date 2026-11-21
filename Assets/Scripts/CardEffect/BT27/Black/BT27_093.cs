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
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceFromHandOrTrashToDrawAndGainMemory(
                        card: card,
                        cardEffect: activateClass,
                        canSelectCardCondition: CanSelectCardCondition,
                        customMessage: "Select 1 card with [Chessmon] in its text to place face down under this Tamer.",
                        faceDown: true));
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

                    //Suspending and trashing are one cost, so the trash still happens if the suspend was prevented
                    if (!CanPayFaceDownCost()) yield break;

                    yield return StartCoroutine(CardEffectCommons.TrashTamerBottomFaceDownSources(card, activateClass, 2, successProcess: SuccessProcess()));

                    IEnumerator SuccessProcess()
                    {
                        #region 1 Digimon may digivolve
                        if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectDigimonCondition)
                        || !CardEffectCommons.HasMatchConditionOwnersHand(card, ChessmonCardCondition)
                        || !suspended) yield break;

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
