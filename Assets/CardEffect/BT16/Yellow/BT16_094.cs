using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT16
{
    public class BT16_094 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Option Skill

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] Reveal the top 4 cards of your deck. Add 1 yellow card or 1 card with the [Four Great Dragons] trait among them to your hand. Place the remaining cards at the bottom of your deck in any order. Then, place this card in your battle area.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardColors.Contains(CardColor.Yellow) || cardSource.EqualsTraits("Four Great Dragons"))
                    {
                        return true;
                    }
                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndSelect(
                    revealCount: 4,
                    selectCardConditions:
                    new SelectCardConditionClass[]
                    {
                        new SelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList:null,
                            canEndSelectCondition:null,
                            canNoSelect:false,
                            selectCardCoroutine: SelectCardCoroutine,
                            message: "Select 1 yellow card or 1 card with the [Four Great Dragons] trait.",
                            maxCount: 1,
                            canEndNotMax:false,
                            mode: SelectCardEffect.Mode.AddHand
                            ),
                    },
                    remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                    activateClass: activateClass,
                    canNoAction: false));

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }

            #endregion

            #region Delay Effect

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Trial of the Four Great Dragons] or trash 1 card and apply effects.", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] <Delay>\r\n• Place 1 [Trial of the Four Great Dragons] from your hand in the battle area, or you may trash 1 card with the [Four Great Dragons] trait in your hand. If you did either, 1 of your opponent's Digimon gets -7000 DP for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanDeclareOptionDelayEffect(card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (card.Owner.HandCards.Count >= 1)
                    {
                        if (cardSource.EqualsCardName("Trial of the Four Great Dragons"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        return true;
                    }
                    return false;
                }

                bool CanSelect4gdInHand(CardSource card)
                {
                    if (card.Owner.HandCards.Count >= 1)
                    {
                        if (card.EqualsTraits("Four Great Dragons"))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool deleted = false;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        deleted = true;

                        yield return null;
                    }

                    if (deleted)
                    {
                        bool canPlayT4GD = card.Owner.HandCards.Count(CanSelectCardCondition) >= 1;
                        bool canTrash4GD = card.Owner.TrashCards.Count(CanSelect4gdInHand) >= 1;

                        if (canPlayT4GD || canTrash4GD)
                        {
                            if (canPlayT4GD && canTrash4GD)
                            {
                                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Play [Trial of the Four Great Dragons]", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Trash a card", value : false, spriteIndex: 1),
                        };

                                string selectPlayerMessage = "Which will you choose?";
                                string notSelectPlayerMessage = "The opponent is choosing effects.";

                                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                            }
                            else
                            {
                                GManager.instance.userSelectionManager.SetBool(canPlayT4GD);
                            }

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            bool decision = GManager.instance.userSelectionManager.SelectedBoolValue;

                            List<CardSource> selectedCards = new List<CardSource>();
                            bool discarded = false;

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            if (decision)
                            {
                                int maxCount1 = 1;

                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount1,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectHandEffect.SetUpCustomMessage("Select 1 card to place in the battle area.", "The opponent is selecting 1 place in the battle area.");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                                yield return StartCoroutine(selectHandEffect.Activate());

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(
                                card: selectedCards[0],
                                cardEffect: activateClass,
                                root: SelectCardEffect.Root.Hand));
                            }
                            else
                            {
                                int discardCount = 1;

                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelect4gdInHand,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: discardCount,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                    mode: SelectHandEffect.Mode.Discard,
                                    cardEffect: activateClass);

                                yield return StartCoroutine(selectHandEffect.Activate());

                                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                                {
                                    if (cardSources.Count >= 1)
                                    {
                                        discarded = true;

                                        yield return null;
                                    }
                                }
                            }

                            if (discarded || selectedCards.Count > 0)
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                                {
                                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectPermanentCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: true,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to give -7000 DP.", "The opponent is selecting 1 Digimon to give -7000 DP.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        Permanent selectedPermanent = permanent;

                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                            selectedPermanent,
                                            -7000,
                                            EffectDuration.UntilEachTurnEnd,
                                            activateClass)
                                            );
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Security Skill

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Give 1 Digimon -7000 DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] 1 of your opponent's Digimon gets -7000 DP for the turn. Then, place this card in the battle area.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 Digimon that will get DP -7000.",
                        "The opponent is selecting 1 Digimon that will get DP -7000.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                selectedPermanent,
                                -7000,
                                EffectDuration.UntilEachTurnEnd,
                                activateClass)
                                );
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}