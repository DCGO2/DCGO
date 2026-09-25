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
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When any of your Digimon digivolve, if you have 1 or fewer Digimon, by suspending this Tamer, you may play 1 [Gabumon] if that Digimon has [Greymon] in its name and 1 [Agumon] if it has [Garurumon] in its name from your hand or trash without paying the cost.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           (permanent.TopCard.ContainsCardName("Greymon") || permanent.TopCard.ContainsCardName("Garurumon"));
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.IsOwnerTurn(card) &&
                           CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.CanActivateSuspendCostEffect(card) &&
                           card.Owner.GetBattleAreaDigimons().Count <= 1;
                }

                List<Permanent> DigivolvedPermanents(Hashtable hashtable)
                {
                    List<Permanent> permanents = new List<Permanent>();

                    foreach (Hashtable hash in CardEffectCommons.GetHashtablesFromHashtable(hashtable))
                    {
                        Permanent permanent = CardEffectCommons.GetPermanentFromHashtable(hash);

                        if (permanent != null && PermanentCondition(permanent))
                        {
                            permanents.Add(permanent);
                        }
                    }

                    return permanents;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> digivolvedPermanents = DigivolvedPermanents(hashtable);

                    bool hasGreymon = digivolvedPermanents.Any(permanent => permanent.TopCard.ContainsCardName("Greymon"));
                    bool hasGarurumon = digivolvedPermanents.Any(permanent => permanent.TopCard.ContainsCardName("Garurumon"));

                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    List<CardSource> handCards = new List<CardSource>();
                    List<CardSource> trashCards = new List<CardSource>();

                    if (hasGreymon)
                    {
                        yield return ContinuousController.instance.StartCoroutine(SelectFromHandOrTrash("Gabumon"));
                    }

                    if (hasGarurumon)
                    {
                        yield return ContinuousController.instance.StartCoroutine(SelectFromHandOrTrash("Agumon"));
                    }

                    if (handCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: handCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Hand,
                            activateETB: true));
                    }

                    if (trashCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: trashCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Trash,
                            activateETB: true));
                    }

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

                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                            {
                                new SelectionElement<bool>(message: "From hand", value: true, spriteIndex: 0),
                                new SelectionElement<bool>(message: "From trash", value: false, spriteIndex: 1),
                            };

                            string selectPlayerMessage = $"From which area do you play [{cardName}]?";
                            string notSelectPlayerMessage = "The opponent is choosing from which area to play a card.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectHand);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

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
                                selectCardCoroutine: SelectHandCardCoroutine,
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
                                selectCardCoroutine: SelectTrashCardCoroutine,
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
                            selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }

                        IEnumerator SelectHandCardCoroutine(CardSource cardSource)
                        {
                            handCards.Add(cardSource);
                            yield return null;
                        }

                        IEnumerator SelectTrashCardCoroutine(CardSource cardSource)
                        {
                            trashCards.Add(cardSource);
                            yield return null;
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
