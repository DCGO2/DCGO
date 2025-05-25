using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace DCGO.CardEffects.BT20
{
    public class BT20_077 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Ace - Blast Digivolve

            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }

            #endregion

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 5)
                    {
                        if (targetPermanent.TopCard.EqualsTraits("Dark Dragon") || targetPermanent.TopCard.EqualsTraits("Evil Dragon"))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region On Play/When Digivolving Shared
            bool CardTrashed(CardSource source)
            {
                return true;
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash until 4 left and play a Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Trash cards in your hand until it has 4 left. Then, play 1 8000 DP or lower Digimon card from your trash without paying the cost. For each card this effect trashed, remove 2000 from this effect's DP maximum.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> trashed = new List<CardSource>();

                    if (card.Owner.HandCards.Count > 4)
                    {
                        int maxCount = card.Owner.HandCards.Count - 4;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CardTrashed,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AllSelectedCards,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage($"Select {maxCount} card(s) to trash.", $"The opponent is selecting {maxCount} card(s) to trash.");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator AllSelectedCards(List<CardSource> sources)
                        {
                            trashed = sources;
                            yield return null;
                        }
                    }


                    bool CanSelectCardInTrash(CardSource cardSource)
                    {
                        int maxDP = 8000 - (trashed.Count() * 2000);

                        if (cardSource.IsDigimon && cardSource.HasDP)
                        {
                            if (cardSource.CardDP <= maxDP)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardInTrash))
                    {
                        int maxCount = Math.Min(1, card.Owner.TrashCards.Count(CanSelectCardInTrash));

                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardInTrash,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 card to play.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
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
                                root: SelectCardEffect.Root.Trash,
                                activateETB: true));
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash until 4 left and play a Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Trash cards in your hand until it has 4 left. Then, play 1 8000 DP or lower Digimon card from your trash without paying the cost. For each card this effect trashed, remove 2000 from this effect's DP maximum.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }



                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> trashed = new List<CardSource>();

                    if (card.Owner.HandCards.Count > 4)
                    {
                        int maxCount = card.Owner.HandCards.Count - 4;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CardTrashed,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AllSelectedCards,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage($"Select {maxCount} card(s) to trash.", $"The opponent is selecting {maxCount} card(s) to trash.");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator AllSelectedCards(List<CardSource> sources)
                        {
                            trashed = sources;
                            yield return null;
                        }
                    }


                    bool CanSelectCardInTrash(CardSource cardSource)
                    {
                        int maxDP = 8000 - (trashed.Count() * 2000);

                        if (cardSource.IsDigimon && cardSource.HasDP)
                        {
                            if (cardSource.CardDP <= maxDP)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardInTrash))
                    {
                        int maxCount = Math.Min(1, card.Owner.TrashCards.Count(CanSelectCardInTrash));

                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardInTrash,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 card to play.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
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
                                root: SelectCardEffect.Root.Trash,
                                activateETB: true));
                    }
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.None)
            {
                bool CanUseCondition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.EqualsTraits("Dark Dragon") || permanent.TopCard.EqualsTraits("Evil Dragon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.RushStaticEffect(
                    permanentCondition: PermanentCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition));

                cardEffects.Add(CardEffectFactory.BlockerStaticEffect(
                    permanentCondition: PermanentCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition));

                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                permanentCondition: PermanentCondition,
                changeValue: 2000,
                isInheritedEffect: false,
                card: card,
                condition: CanUseCondition,
                effectName: () => "Your Digimon with [Dark Dragon] or [Evil Dragon] traits gain DP +2000"));

            }
            #endregion

            return cardEffects;
        }
    }
}