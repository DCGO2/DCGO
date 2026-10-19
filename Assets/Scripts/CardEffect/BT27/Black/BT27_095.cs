using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Homer Yushima
namespace DCGO.CardEffects.BT27
{
    public class BT27_095 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 1 [Cyborg] or [Aquatic] hand card face down under this Tamer, Draw 1 and gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Start of Your Main Phase] By placing 1 [Cyborg] or [Aquatic] trait card from your hand face down under this Tamer, <Draw 1> and gain 1 memory.";

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.EqualsTraits("Cyborg") || cardSource.EqualsTraits("Aquatic");

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

                        if (card.Owner.CanAddMemory(activateClass))
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                        }
                    }
                }
            }
            #endregion


            #region Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend this Tamer, trash 2 bottom face-down Tamer cards, digivolve for 2 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] By suspending this Tamer and trashing 2 bottom face-down cards from under any of your Tamers, 1 of your Digimon may digivolve into a [Cyborg] or [Aquatic] trait Digimon card in the hand with the cost reduced by 2.";

                bool IsEligibleHandCard(CardSource candidate)
                    => candidate.IsDigimon && (candidate.EqualsTraits("Cyborg") || candidate.EqualsTraits("Aquatic"));

                bool IsEligibleDigimon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool IsFaceDown(CardSource source) => source.IsFaceDown;

                bool TamerWithSource(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.DigivolutionCards.Any(IsFaceDown);

                int AvailableSourceCount()
                    => card.Owner.GetBattleAreaPermanents()
                        .Where(TamerWithSource)
                        .Sum(permanent => permanent.DigivolutionCards.Count(IsFaceDown));

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card)
                        && AvailableSourceCount() >= 2
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, IsEligibleHandCard)
                        && CardEffectCommons.HasMatchConditionPermanent(IsEligibleDigimon);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    // Pay suspension cost first, following the existing Tamer activation pattern.
                    bool suspended = false;
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent> { card.PermanentOfThisCard() },
                            activateClass: activateClass,
                            successProcess: OnSuspended,
                            failureProcess: null));

                    IEnumerator OnSuspended(List<Permanent> permanents)
                    {
                        suspended = permanents != null && permanents.Count > 0;
                        yield return null;
                    }

                    if (!suspended) yield break;

                    // Each payment removes the bottom-most eligible face-down source
                    // from any selected friendly Tamer; the same Tamer may be chosen twice.
                    for (int i = 0; i < 2; i++)
                    {
                        Permanent chosenTamer = null;
                        SelectPermanentEffect selector = GManager.instance.GetComponent<SelectPermanentEffect>();
                        selector.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: TamerWithSource,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectTamer,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);
                        selector.SetUpCustomMessage(
                            "Select a Tamer to trash its bottom face-down card.",
                            "The opponent is selecting a Tamer.");
                        yield return ContinuousController.instance.StartCoroutine(selector.Activate());

                        IEnumerator SelectTamer(Permanent permanent)
                        {
                            chosenTamer = permanent;
                            yield return null;
                        }

                        if (chosenTamer == null) yield break;
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                                targetPermanent: chosenTamer,
                                trashCount: 1,
                                isFromTop: false,
                                activateClass: activateClass,
                                cardCondition: IsFaceDown));
                    }

                    Permanent chosenDigimon = null;
                    SelectPermanentEffect digimonSelector = GManager.instance.GetComponent<SelectPermanentEffect>();
                    digimonSelector.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsEligibleDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectDigimon,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);
                    digimonSelector.SetUpCustomMessage(
                        "Select a Digimon to digivolve.",
                        "The opponent is selecting a Digimon.");
                    yield return ContinuousController.instance.StartCoroutine(digimonSelector.Activate());

                    IEnumerator SelectDigimon(Permanent permanent)
                    {
                        chosenDigimon = permanent;
                        yield return null;
                    }

                    if (chosenDigimon != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: chosenDigimon,
                                cardCondition: IsEligibleHandCard,
                                payCost: true,
                                reduceCostTuple: (reduceCost: 2, reduceCostCardCondition: null),
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: true,
                                activateClass: activateClass,
                                successProcess: null));
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            #endregion

            return cardEffects;
        }
    }
}
