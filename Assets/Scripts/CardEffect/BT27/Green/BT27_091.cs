using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Tomoro Tenma
namespace DCGO.CardEffects.BT27
{
    public class BT27_091 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 1 [BEATBREAK] hand card face down under this Tamer, <Draw 1> and gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Start of Your Main Phase] By placing 1 [BEATBREAK] trait card from your hand face down under this Tamer, <Draw 1> and gain 1 memory.";

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.HasBeatBreakTraits;

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

            #region All Turns
            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspending this Tamer, place top deck card face down under this Tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] When any Digimon suspend, by suspending this Tamer, you may place the top card of your deck face down under this Tamer.";

                bool SuspendedDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, SuspendedDigimonCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>
                    {
                        new(message: $"Suspend and place 1 card from deck under this card", value: 1, spriteIndex: 0),
                        new(message: $"Only suspend without placing cards", value: 2, spriteIndex: 0),
                        new(message: $"No", value: 3, spriteIndex: 1)
                    };

                    GManager.instance.userSelectionManager.SetIntSelection(
                        selectionElements: selectionElements,
                        selectPlayer: card.Owner,
                        selectPlayerMessage: "Will you suspend this tamer to place the top card of your deck under this Tamer face down?",
                        notSelectPlayerMessage: "The opponent is choosing whether to place the top card of their deck under their Tamer face down.");

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    int answer = GManager.instance.userSelectionManager.SelectedIntValue;

                    if (answer == 3) yield break;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass,
                        successProcess: SuccessProcess,
                        failureProcess: null));

                    IEnumerator SuccessProcess(List<Permanent> _permanents)
                    {
                        List<CardSource> topCards = card.Owner.LibraryCards.Take(1).ToList();

                        if (answer == 1 && topCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(topCards, activateClass, isFacedown: true));
                        }
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
