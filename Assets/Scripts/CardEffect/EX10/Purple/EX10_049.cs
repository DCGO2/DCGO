using System;
using System.Collections;
using System.Collections.Generic;

// SkullSatamon
namespace DCGO.CardEffects.EX10
{
    public class EX10_049 : CEntity_Effect
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

            #region WD/OD Shared

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Check Opponent Trash & Trash 3 if true

                bool trashDeck = card.Owner.Enemy.TrashCards.Count <= 10;

                if (trashDeck)
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

                #endregion

                int deleteLevel = card.Owner.Enemy.TrashCards.Count >= 10 ? 5 : 3;

                bool PermamentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasLevel && permanent.TopCard.Level <= deleteLevel;
                }

                #region Select 1 Digimon to Delete

                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(PermamentCondition));

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: PermamentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: null,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Destroy,
                    cardEffect: activateClass);

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                #endregion
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If opponent has 10 or more trash cards, trash top 3 of both player decks. then delete 1 level 3 or lower digimon, add 2 levels if enemy trash is 10 or more", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If your opponent has 10 or fewer cards in their trash, trash the top 3 cards of both players' decks. Then, delete 1 of your opponent's level 3 or lower Digimon. If your opponent has 10 or more cards in their trash, add 2 to this effect's level maximum.";
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

            #region On Deletion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If opponent has 10 or more trash cards, trash top 3 of both player decks. then delete 1 level 3 or lower digimon, add 2 levels if enemy trash is 10 or more", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] If your opponent has 10 or fewer cards in their trash, trash the top 3 cards of both players' decks. Then, delete 1 of your opponent's level 3 or lower Digimon. If your opponent has 10 or more cards in their trash, add 2 to this effect's level maximum.";
                }

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

            #region ESS Once Per Turn

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If opponent has 10 or less trash cards, both player trash 2 cards from top deck, otherwise Sec +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("EX10_049_WA");
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