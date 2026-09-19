using System.Collections;
using System.Collections.Generic;

// Vilemon
namespace DCGO.CardEffects.BT24
{
    public class BT24_069 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared WM / WD

            string SharedEffectName = "Trash 1. Opponent trashes 1 or mills 2";

            string SharedEffectDescription(string tag) => $"[{tag}] Trash 1 card in your hand. Then, your opponent may trash 1 card in their hand. If your opponent didn't trash with this effect, trash the top 2 cards of your opponent's deck.";

            bool SharedCanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Trash 1 card
                if (card.Owner.HandCards.Count >= 1)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: (cardSource) => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    selectHandEffect.SetUpCustomMessage("Select 1 Card to trash.", "The opponent is selecting 1 card to trash from their hand.");

                    yield return StartCoroutine(selectHandEffect.Activate());
                }
                #endregion

                bool discarded = false;

                #region Opponent Trash 1 Card
                if (card.Owner.Enemy.HandCards.Count >= 1)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner.Enemy,
                        canTargetCondition: (cardSource) => true,
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

                    selectHandEffect.SetUpCustomMessage("Select 1 Card to trash. If you don't you will trash 2 cards from deck.", "The opponent is selecting 1 card to trash from their hand.");

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
                #endregion

                #region Mill 2
                if (!discarded)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(
                        addTrashCount: 2,
                        player: card.Owner.Enemy,
                        cardEffect: activateClass).AddTrashCardsFromLibraryTop());
                }
                #endregion
            }

            #endregion

            #region When Moving
            if (timing == EffectTiming.OnMove)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, SharedEffectDescription("When Moving"));
                cardEffects.Add(activateClass);

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnMove(hashtable, PermanentCondition);
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }
            #endregion

            #region All Turns

            if (timing == EffectTiming.None)
            {
                bool Condition() => card.Owner.Enemy.TrashCards.Count >= 10;

                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(false, card, Condition));
                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 2000,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition));
            }
            #endregion

            #region ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash top card from both players deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT24_069_TrashTopDeck");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] [Once Per Turn] Trash the top card of both players' decks.";
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
