using System;
using System.Collections;
using System.Collections.Generic;

// Thundermon
namespace DCGO.CardEffects.EX13
{
    public class EX13_053 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared On Play / On Deletion

            string SharedEffectName = "May return up to 3 [Mamemon] in text Digimon cards from trash to deck top, then delete 1 Digimon";

            string SharedEffectDescription(string tag)
                => $"[{tag}] You may return up to 3 Digimon cards with [Mamemon] in their texts from your trash to the top of the deck. Then, delete 1 of your opponent's Digimon with a play cost of 3 or less. For each one this effect returned, add 1 to this effect's play cost maximum.";

            bool CanSelectCardCondition(CardSource cardSource)
                => cardSource.IsDigimon
                    && cardSource.HasText("Mamemon");

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                int returnedCount = 0;

                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                {
                    int maxCount = Math.Min(3, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CanSelectCardCondition));

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        message: "Select cards to place at the top of the deck\n(cards will be placed back to the top of the deck so that cards with lower numbers are on top).",
                        maxCount: maxCount,
                        canEndNotMax: true,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage_ShowCard("Deck Top Cards");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count >= 1)
                        {
                            List<CardSource> returnCards = new List<CardSource>(cardSources);
                            returnCards.Reverse();

                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryTopCards(returnCards, cardEffect: activateClass));

                            returnedCount = returnCards.Filter(cardSource => card.Owner.LibraryCards.Contains(cardSource)).Count;
                        }
                    }
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasPlayCost
                        && permanent.TopCard.GetCostItself <= 3 + returnedCount;

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
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
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                hashValue: "EX13_053_OP_OD",
                onPlay: true,
                onDeletion: true);

            #endregion

            #region On Deletion - ESS
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<De-Digivolve 1> 1 of your opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("EX13_053_ESS_OD");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Deletion] <De-Digivolve 1> 1 of your opponent's Digimon.";

                bool CanSelectPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.CanActivateOnDeletion(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
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
                            mode: SelectPermanentEffect.Mode.Degenerate,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve.", "The opponent is selecting 1 Digimon to De-Digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
