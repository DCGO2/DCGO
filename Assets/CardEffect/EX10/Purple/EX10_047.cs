using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Arukenimon
namespace DCGO.CardEffects.EX10
{
    public class EX10_047 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 card from hand to delete up to 6000DP of opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By trashing 1 card in your hand, delete up to 6000 DP total worth of your opponent's Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                       card.Owner.HandCards.Count >= 1;
                }

                int DeletionMaxDP() => 6000;

                bool CanSelectDeletePermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(DeletionMaxDP(), activateClass))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    // First select the card to discard
                    bool discarded = false;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        canTargetCondition: _ => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectHandEffect.Mode.Discard,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectHandEffect.SetUpCustomMessage("Select 1 card to discard.",
                        "The opponent is selecting 1 card to discard.");

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        discarded = cardSources.Count > 0;

                        yield return null;
                    }

                    // If the player discarded a card, continue with the rest of the effect
                    if (discarded && CardEffectCommons.HasMatchConditionPermanent(CanSelectDeletePermanentCondition))
                    {
                        int maxCount = Math.Min(card.Owner.Enemy.GetBattleAreaDigimons().Count(CanSelectDeletePermanentCondition),
                            CardEffectCommons.MatchConditionPermanentCount(CanSelectDeletePermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDeletePermanentCondition,
                            canTargetCondition_ByPreSelecetedList: CanTargetConditionByPreSelectedList,
                            canEndSelectCondition: CanEndSelectCondition,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: true,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Choose digimon to delete", "Opponent is choosing digimon to delete");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        bool CanEndSelectCondition(List<Permanent> permanents)
                        {
                            if (permanents.Count <= 0)
                            {
                                return false;
                            }

                            int sumDP = 0;

                            foreach (Permanent permanent in permanents)
                            {
                                sumDP += permanent.DP;
                            }

                            if (sumDP > card.Owner.MaxDP_DeleteEffect(DeletionMaxDP(), activateClass))
                            {
                                return false;
                            }

                            return true;
                        }

                        bool CanTargetConditionByPreSelectedList(List<Permanent> permanents, Permanent permanent)
                        {
                            int sumDP = 0;

                            foreach (Permanent permanent1 in permanents)
                            {
                                sumDP += permanent1.DP;
                            }

                            sumDP += permanent.DP;

                            if (sumDP > card.Owner.MaxDP_DeleteEffect(DeletionMaxDP(), activateClass))
                            {
                                return false;
                            }

                            return true;
                        }
                    }
                }
            }
            #endregion

            #region On Deletion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 tamer with [Myotismon] in it's text from your trash.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] You may play 1 Tamer card with [Myotismon] in its text from your trash without paying the cost. This effect can't play cards with the same name as any of your Tamers.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            {
                                if (cardSource.HasText("Myotismon") && cardSource.IsTamer)
                                {
                                    if (!card.Owner.GetBattleAreaPermanents().Some(permanent => cardSource.HasSameCardName(permanent.TopCard)))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanActivateOnDeletion(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int maxCount = 1;

                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectCardCondition,
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

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    SelectCardEffect.Root root = SelectCardEffect.Root.Trash;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: selectedCards,
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: root,
                        activateETB: true));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}