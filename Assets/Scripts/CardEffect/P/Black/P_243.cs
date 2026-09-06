using System.Collections;
using System.Collections.Generic;

// Digiseabass
namespace DCGO.CardEffects.P
{
    public class P_243 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Use Req
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("DM");
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 to <Draw 2> and place in battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] By trashing 1 card from your hand, <Draw 2>. After, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: _ => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
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
                            yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 2, activateClass).Draw());
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                        }
                    }
                }
            }
            #endregion

            #region Delay
            if (timing == EffectTiming.OnStartTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By top decking a [DM] Digimon from trash, may play a 3 cost or lower [DM] from trash for free", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Start of Your Turn] If your opponent has a Digimon, <Delay>.\r\n• By returning 1 [DM] trait Digimon card from your trash to the top of the deck, you may play 1 play cost 3 or lower [DM] trait card from your trash without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.HasMatchConditionPermanent(PermanentCondition)
                        && CardEffectCommons.CanDeclareOptionDelayEffect(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.EqualsTraits("DM");
                }

                bool CanSelectCardCondition2(CardSource cardSource)
                {
                    return cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 3
                        && cardSource.EqualsTraits("DM")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                            activateClass: activateClass, successProcess: _ => SuccessProcess(),
                            failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        if(CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition1))
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            bool cardsAdded = false;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition1,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            message: "Select card to place at the top of the deck",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: false,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);
                                yield return null;
                            }

                            IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                            {
                                if (selectedCards.Count == 1)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryTopCards(selectedCards, cardEffect: activateClass));
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(cardSources, "Deck Top Cards", true, true));
                                    cardsAdded = true;
                                }
                            }

                            if (cardsAdded)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                                    canTargetCondition: CanSelectCardCondition2,
                                    SelectCardEffect.Root.Trash,
                                    activateClass,
                                    payCost: false
                                ));
                            }
                        }
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 3 cost or lower [DM] from hand/trash for free", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                 => "[Security] You may play 1 play cost 3 or lower [DM] trait card from your hand or trash without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                bool CanPlayCondition(CardSource cardSource)
                {
                    return cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 3
                        && cardSource.EqualsTraits("DM")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanPlayCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanPlayCondition);

                    if (canSelectHand || canSelectTrash)
                    {
                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<int>> selectionElements1 = new List<SelectionElement<int>>()
                        {
                            new (message: $"From hand", value : 1, spriteIndex: 0),
                            new (message: $"From trash", value : 2, spriteIndex: 0),
                            new (message: $"Don't play", value: 3, spriteIndex: 1)
                        };

                            string selectPlayerMessage1 = "From which area will you play a card?";
                            string notSelectPlayerMessage1 = "The opponent is choosing from which area to select a card.";

                            GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage1, notSelectPlayerMessage: notSelectPlayerMessage1);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetInt(canSelectHand ? 1 : 2);
                        }
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        SelectCardEffect.Root root = GManager.instance.userSelectionManager.SelectedIntValue == 1 ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                        bool doSelect = GManager.instance.userSelectionManager.SelectedIntValue != 3;

                        if (doSelect)
                        {
                            #region Hand/Trash Card Selection & Play
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                                canTargetCondition: CanPlayCondition,
                                root,
                                activateClass,
                                payCost: false
                            ));
                            #endregion
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
