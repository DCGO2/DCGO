using System;
using System.Collections;
using System.Collections.Generic;

// Devimon
namespace DCGO.CardEffects.EX10
{
    public class EX10_046 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region SOYMP/WD Shared

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Check Opponent Trash & Trash 2 if true

                bool trashDeck = card.Owner.Enemy.TrashCards.Count <= 10;

                if (trashDeck)
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

                #endregion

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.HasFallenAngelTraits || cardSource.HasUndeadTraits;
                }

                if (card.Owner.Enemy.TrashCards.Count >= 10 && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CardCondition))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CardCondition));
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 [Fallen Angel]/[Undead] to add to your hand.",
                        maxCount: maxCount,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.AddHand,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                }
            }

            #endregion

            #region Start of Your Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If opponent has 10 or less in trash, trash top 2 from both players deck, then if 10 or more, add 1 [Fallen Angel]/[Undead] from trash to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] If your opponent has 10 or fewer cards in their trash, trash the top 2 cards of both players' decks. Then, if they have 10 or more cards in their trash, you may return 1 card with the [Fallen Angel] or [Undead] trait from your trash to the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If opponent has 10 or less in trash, trash top 2 from both players deck, then if 10 or more, add 1 [Fallen Angel]/[Undead] from trash to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If your opponent has 10 or fewer cards in their trash, trash the top 2 cards of both players' decks. Then, if they have 10 or more cards in their trash, you may return 1 card with the [Fallen Angel] or [Undead] trait from your trash to the hand.";
                }

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

            #region ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash top card from both players deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("EX10_046_TrashTopDeck");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] Trash the top card of both players' decks.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnField(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnField(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(
                        addTrashCount: 1,
                        player: card.Owner,
                        cardEffect: activateClass).AddTrashCardsFromLibraryTop());

                    yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(
                        addTrashCount: 1,
                        player: card.Owner.Enemy,
                        cardEffect: activateClass).AddTrashCardsFromLibraryTop());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
