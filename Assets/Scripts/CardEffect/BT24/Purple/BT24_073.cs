using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

// SkullSatamon
namespace DCGO.CardEffects.BT24
{
    public class BT24_073 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Blocker

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region Shared WD/OD

            string SharedEffectName() => "Mill top 3 everyone and/or play a traited card from trash.";

            string SharedEffectDescription(string tag) => $"[{tag}] If your opponent has 10 or fewer cards in their trash, trash the top 3 cards of both player's decks. Then, if your opponent has 10 or more cards in their trash, you may play 1 level 4 or lower [Evil] or [Fallen Angel] trait Digimon card from your trash without paying the cost.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool CanPlayCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel
                        && cardSource.Level <= 4
                        && (cardSource.EqualsTraits("Evil")
                        || cardSource.EqualsTraits("Fallen Angel"))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                if (card.Owner.Enemy.TrashCards.Count <= 10)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(
                        addTrashCount: 3,
                        player: card.Owner,
                        cardEffect: activateClass).AddTrashCardsFromLibraryTop());

                    yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(
                        addTrashCount: 3,
                        player: card.Owner.Enemy,
                        cardEffect: activateClass).AddTrashCardsFromLibraryTop());
                }

                if (card.Owner.Enemy.TrashCards.Count >= 10)
                {
                    int maxCount = Math.Min(1, card.Owner.TrashCards.Count(CanPlayCardCondition));

                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                                canTargetCondition: CanPlayCardCondition,
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

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Trash, activateETB: true));
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

            }

            #endregion

            #region On Deletion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Deletion"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card);
                }
            }

            #endregion

            #region Inherited

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If opponent has 10 or less trash cards, both player trash 2 cards from top deck, otherwise Sec +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("BT24_073_Inherited");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] This Digimon gains <Security A. +1> for the turn. (This Digimon checks 1 additional security card.) If your opponent has 10 or fewer cards in their trash, instead trash the top 2 cards of both players' decks.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool hasSecAttack = card.Owner.Enemy.TrashCards.Count >= 11;

                    if (hasSecAttack) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(
                        targetPermanent: card.PermanentOfThisCard(),
                        changeValue: 1,
                        effectDuration: EffectDuration.UntilEachTurnEnd,
                        activateClass: activateClass));

                    if (!hasSecAttack)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(
                            addTrashCount: 2,
                            player: card.Owner,
                            cardEffect: activateClass).AddTrashCardsFromLibraryTop());

                        yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(
                            addTrashCount: 2,
                            player: card.Owner.Enemy,
                            cardEffect: activateClass).AddTrashCardsFromLibraryTop());
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
