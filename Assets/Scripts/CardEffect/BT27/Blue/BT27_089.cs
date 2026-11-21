using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Thomas H. Norstein
namespace DCGO.CardEffects.BT27
{
    public class BT27_089 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 1 [Gaomon]/[Gaogamon] in name Digimon face down under this Tamer, <Draw 1> and gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Start of Your Main Phase] By placing 1 Digimon card with [Gaomon] or [Gaogamon] in its name from your hand face down under this Tamer, <Draw 1> and gain 1 memory.";

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.ContainsCardName("Gaomon")
                        || cardSource.ContainsCardName("Gaogamon");

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
                        customMessage: "Select 1 card with [Gaomon] or [Gaogamon] in its name to place face down under this Tamer.",
                        faceDown: true));
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnAddHand)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend to may place top 1 from deck face down under this tamer, them make 1 [Gaomon] in text Digimon not deleted by battle", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When effects add to your opponent's hand, by suspending this Tamer, you may place the top card of your deck face down under this Tamer. After, 1 of your Digimon with [Gaogamon] in its name can't be deleted in battle for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenAddHand(hashtable, player => player == card.Owner.Enemy, cardEffect => cardEffect != null);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.ContainsCardName("Gaogamon");
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>
                    {
                        new(message: $"Suspend and place 1 card from deck under this card", value: 1, spriteIndex: 0),
                        new(message: $"Only suspend without placing cards", value: 2, spriteIndex: 0),
                        new(message: $"No", value: 3, spriteIndex: 1)
                    };

                    string selectPlayerMessage = "Will you suspend this tamer to place the top card from your deck under this Tamer face down?";
                    string notSelectPlayerMessage = "The opponent is choosing whether to place the top card from their deck under their Tamer face down.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    int answer = GManager.instance.userSelectionManager.SelectedIntValue;

                    if (answer != 3)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                            new List<Permanent>() { card.PermanentOfThisCard() },
                            activateClass,
                            SuccessProcess,
                            null));
                    }

                    IEnumerator SuccessProcess(List<Permanent> suspendedPermaments)
                    {
                        List<CardSource> topCards = card.Owner.LibraryCards.Take(1).ToList();

                        if (answer == 1 && topCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(topCards, activateClass, isFacedown: true));
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            SelectPermanentEffect selectProtectEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectProtectEffect.SetUp(
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

                            selectProtectEffect.SetUpCustomMessage("Select 1 Digimon that can't be deleted in battle.", "The opponent is selecting 1 Digimon that can't be deleted in battle.");

                            yield return ContinuousController.instance.StartCoroutine(selectProtectEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                bool CanNotBeDestroyedByBattleCondition(Permanent permanent1, Permanent attackingPermanent, Permanent defendingPermanent, CardSource defendingCard)
                                    => permanent1 == attackingPermanent || permanent1 == defendingPermanent;

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotBeDeletedByBattle(
                                    targetPermanent: permanent,
                                    canNotBeDestroyedByBattleCondition: CanNotBeDestroyedByBattleCondition,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                    activateClass: activateClass,
                                    effectName: "Can't be deleted in battle"));
                            }
                        }
                    }
                }
            }
            #endregion

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}
