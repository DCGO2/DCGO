using System;
using System.Collections;
using System.Collections.Generic;

// Ryugumon
namespace DCGO.CardEffects.BT22
{
    public class BT22_027 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Decode

            if (timing == EffectTiming.WhenRemoveField)
            {
                bool SourceCondition(CardSource source)
                {
                    return source.IsDigimon
                    && source.HasLevel && source.IsLevel5
                    && (source.ContainsTraits("Aqua") || source.ContainsTraits("Sea Animal"));
                }

                string[] decodeStrings = { "(Lv.5 w/[Aqua]/[Sea Animal] trait)", "Level 5 Digimon card with [Aqua] or [Sea Animal] " };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(card: card, isInheritedEffect: false, decodeStrings: decodeStrings, sourceCondition: SourceCondition, condition: null));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 level 5 or lower [Aqua]/[Sea Animal] digimon in sources, 1 digimon or tamer cant suspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By placing 1 level 5 or lower Digimon card with [Aqua] or [Sea Animal] in any of its traits from your hand as this Digimon's bottom digivolution card, 1 of your opponent's Digimon or Tamers can't suspend until their turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                           && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, HandCondition);
                }

                bool HandCondition(CardSource source)
                {
                    return source.IsDigimon &&
                        source.HasLevel && source.Level <= 5 &&
                        source.HasAquaTraits;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                        && (permanent.IsDigimon || permanent.IsTamer);
                }

                bool IsRyugumon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent == card.PermanentOfThisCard();
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, HandCondition) && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsRyugumon))
                    {
                        #region Select Hand Card

                        CardSource selectedCard = null;
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, HandCondition));
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: HandCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
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
                        selectHandEffect.SetUpCustomMessage("Select 1 card to add as digivolution card,", "The opponent is selecting 1 card to add as digivolution card,");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");
                        yield return StartCoroutine(selectHandEffect.Activate());

                        #endregion

                        if (selectedCard != null)
                        {
                            Permanent selectedPermanent = card.PermanentOfThisCard();

                            if (selectedPermanent != null && selectedCard != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass));

                                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, PermanentCondition))
                                {
                                    #region Select Opponent Permanent

                                    Permanent opponentPermanent = null;
                                    int maxCount2 = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, PermanentCondition));

                                    SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect1.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: PermanentCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount2,
                                        canNoSelect: false,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine1,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    IEnumerator SelectPermanentCoroutine1(Permanent permanent)
                                    {
                                        opponentPermanent = permanent;
                                        yield return null;
                                    }
                                    selectPermanentEffect1.SetUpCustomMessage("Select 1 Digimon/Tamer that can't suspend.", "The opponent is selecting 1 Digimon/Tamer that can't suspend.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                                    #endregion

                                    if (opponentPermanent != null)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantSuspendUntilOpponentTurnEnd(
                                            targetPermanent: opponentPermanent,
                                            activateClass: activateClass
                                        ));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 level 5 or lower [Aqua]/[Sea Animal] digimon in sources, 1 digimon or tamer cant suspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By placing 1 level 5 or lower Digimon card with [Aqua] or [Sea Animal] in any of its traits from your hand as this Digimon's bottom digivolution card, 1 of your opponent's Digimon or Tamers can't suspend until their turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, HandCondition);
                }

                bool HandCondition(CardSource source)
                {
                    return source.IsDigimon &&
                        source.HasLevel && source.Level <= 5 &&
                        source.HasAquaTraits;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                        && (permanent.IsDigimon || permanent.IsTamer);
                }

                bool IsRyugumon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent == card.PermanentOfThisCard();
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, HandCondition) && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsRyugumon))
                    {
                        #region Select Hand Card

                        CardSource selectedCard = null;
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, HandCondition));
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: HandCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
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
                        selectHandEffect.SetUpCustomMessage("Select 1 card to add as digivolution card,", "The opponent is selecting 1 card to add as digivolution card,");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");
                        yield return StartCoroutine(selectHandEffect.Activate());

                        #endregion

                        if (selectedCard != null)
                        {
                            Permanent selectedPermanent = card.PermanentOfThisCard();

                            if (selectedPermanent != null && selectedCard != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass));

                                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, PermanentCondition))
                                {
                                    #region Select Opponent Permanent

                                    Permanent opponentPermanent = null;
                                    int maxCount2 = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, PermanentCondition));

                                    SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect1.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: PermanentCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount2,
                                        canNoSelect: false,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine1,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    IEnumerator SelectPermanentCoroutine1(Permanent permanent)
                                    {
                                        opponentPermanent = permanent;
                                        yield return null;
                                    }
                                    selectPermanentEffect1.SetUpCustomMessage("Select 1 Digimon/Tamer that can't suspend.", "The opponent is selecting 1 Digimon/Tamer that can't suspend.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                                    #endregion

                                    if (opponentPermanent != null)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantSuspendUntilOpponentTurnEnd(
                                            targetPermanent: opponentPermanent,
                                            activateClass: activateClass
                                        ));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnAddDigivolutionCards)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Bottom deck 1 level 5 or lower digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("BT22_027_OnAddDigivolutionCards");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When effects add to this Digimon's digivolution cards, return 1 of your opponent's level 5 or lower Digimon to the bottom of the deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAddDigivolutionCard(
                        hashtable,
                        IsRyugumon,
                        cardEffectCondition: cardEffect => cardEffect.EffectSourceCard != null,
                        null);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool IsRyugumon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent == card.PermanentOfThisCard();
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasLevel && permanent.TopCard.Level <= 5;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, CanSelectPermanentCondition));
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to bottom deck.", "The opponent is selecting 1 digimon to bottom deck.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}