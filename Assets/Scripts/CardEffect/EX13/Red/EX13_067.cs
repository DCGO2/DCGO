using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Nokia Shiramine
namespace DCGO.CardEffects.EX13
{
    public class EX13_067 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                cardEffects.Add(CardEffectFactory.Gain1MemoryTamerOpponentDigimonEffect(card));
            }

            #endregion

            #region Your Turn

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend this Tamer to play [Gabumon]/[Agumon] from hand or trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] When any of your Digimon digivolve, if you have 1 or fewer Digimon, by suspending this Tamer, you may play 1 [Gabumon] if that Digimon has [Greymon] in its name and 1 [Agumon] if it has [Garurumon] in its name from your hand or trash without paying the cost.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass) &&
                           CardEffectCommons.IsOwnerTurn(card) &&
                           CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass) &&
                           CardEffectCommons.CanActivateSuspendCostEffect(card) &&
                           card.Owner.GetBattleAreaDigimons().Count <= 1;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> digivolvedPermanents = CardEffectCommons.GetHashtablesFromHashtable(hashtable)
                        .Select(CardEffectCommons.GetPermanentFromHashtable)
                        .Where(permanent => permanent != null && PermanentCondition(permanent))
                        .ToList();

                    bool hasGreymon = digivolvedPermanents.Any(permanent => permanent.TopCard.HasGreymonName);
                    bool hasGarurumon = digivolvedPermanents.Any(permanent => permanent.TopCard.HasGarurumonName);

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                        new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass,
                        SuccessProcess,
                        null));

                    IEnumerator SuccessProcess(List<Permanent> suspendedPermanents)
                    {
                        List<CardSource> cardsToPlay = new List<CardSource>();

                        if (hasGreymon)
                        {
                            yield return ContinuousController.instance.StartCoroutine(SelectFromHandOrTrash("Gabumon"));
                        }

                        if (hasGarurumon)
                        {
                            yield return ContinuousController.instance.StartCoroutine(SelectFromHandOrTrash("Agumon"));
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: cardsToPlay,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Hand,
                            activateETB: true));

                        IEnumerator SelectFromHandOrTrash(string cardName)
                        {
                            bool CanSelectCardCondition(CardSource cardSource)
                            {
                                return cardSource.EqualsCardName(cardName) &&
                                       CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                            }

                            bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                            bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                            if (!canSelectHand && !canSelectTrash)
                            {
                                yield break;
                            }

                            bool fromHand = canSelectHand;

                            if (canSelectHand && canSelectTrash)
                            {
                                List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                                {
                                    new(message: "Play from hand", value: 1, spriteIndex: 0),
                                    new(message: "Play from trash", value: 2, spriteIndex: 0),
                                    new(message: "Don't play a card", value: 3, spriteIndex: 1),
                                };

                                GManager.instance.userSelectionManager.SetIntSelection(
                                    selectionElements: selectionElements,
                                    selectPlayer: card.Owner,
                                    selectPlayerMessage: $"From which area will you play [{cardName}]?",
                                    notSelectPlayerMessage: "The opponent is choosing from which area to play a card.");

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                                int selected = GManager.instance.userSelectionManager.SelectedIntValue;

                                if (selected == 3)
                                {
                                    yield break;
                                }

                                fromHand = selected == 1;
                            }

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                cardsToPlay.Add(cardSource);
                                yield return null;
                            }

                            if (fromHand)
                            {
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

                                selectHandEffect.SetUpCustomMessage($"Select 1 [{cardName}] to play.", "The opponent is selecting 1 card to play.");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                            }
                            else
                            {
                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: $"Select 1 [{cardName}] to play.",
                                    maxCount: 1,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage($"Select 1 [{cardName}] to play.", "The opponent is selecting 1 card to play.");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                            }
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
