using System.Collections;
using System.Collections.Generic;

// Erlangmon
namespace DCGO.CardEffects.EX12
{
    public class EX12_034 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Shambala");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 5)
                );
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "May play [Kotenken] Token";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: true,
                onPlay: true,
                whenDigivolving: true,
                counter: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] You may play 1 [Kotenken] Token. (Digimon/Black/9000 DP/<Blocker>)";
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayKotenkenToken(activateClass));
            }
            #endregion

            #region All Turns 1
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 enemy lowest level Digimon to bottom of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("EX12_034_AT_1");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When any of your Digimon are played, return 1 of your opponent's lowest level Digimon to the bottom of the deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if(CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            #region All Turns 2
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May play 1 lvl 5 or lower [SW] from hand/this Digimon's sources for free", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("EX12_034_AT_2");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [One Per Turn] When any of your Digimon would leave the battle area, you may play 1 level 5 or lower [SW] trait card from your hand or this Digimon's digivolution cards without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.HasLevel
                        && cardSource.Level <= 5
                        && cardSource.EqualsTraits("SW")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool validHandCard = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                    bool validDigivolutionCard = card.PermanentOfThisCard().DigivolutionCards.Some(CanSelectCardCondition);

                    if (validHandCard || validDigivolutionCard)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                        if (validHandCard)
                        {
                            selectionElements.Add(new(message: $"Play from hand", value: 1, spriteIndex: 0));
                        }
                        if (validDigivolutionCard)
                        {
                            selectionElements.Add(new(message: $"Play from this Digimon's Digivolution Cards", value: 2, spriteIndex: 0));
                        }
                        ;
                        selectionElements.Add(new(message: $"Don't play", value: 3, spriteIndex: 1));

                        string selectPlayerMessage = "Will you play a card?";
                        string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool doPlay = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                        bool fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;

                        if (doPlay)
                        {
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
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.PlayForFree,
                                    cardEffect: activateClass);

                                yield return StartCoroutine(selectHandEffect.Activate());
                            }
                            else
                            {
                                List<CardSource> selectedCards = new List<CardSource>();

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 digivolution card to play.",
                                    maxCount: 1,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.DigivolutionCards,
                                    customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to play.", "The opponent is selecting 1 digivolution card to play.");
                                selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                                yield return StartCoroutine(selectCardEffect.Activate());

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCards.Add(cardSource);

                                    yield return null;
                                }

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: selectedCards,
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.DigivolutionCards,
                                    activateETB: true));
                            }
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
