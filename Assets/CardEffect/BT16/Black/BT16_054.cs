using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT16
{
    public class BT16_054 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play/When Digivolving Shared

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.CardTraits.Contains("D-Brigade"))
                {
                    return true;
                }

                if (cardSource.CardTraits.Contains("DigiPolice"))
                {
                    return true;
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.TrashCards.Count(CanSelectCardCondition) >= 3)
                    {
                        return true;
                    }
                }

                return false;
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<Rush>, Can't be Blocked", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By returning 3 cards with the [D-Brigade] or [DigiPolice] trait from your trash to the top of the deck, this Digimon gains <Rush>, and can't be blocked for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.TrashCards.Count(CanSelectCardCondition) >= 3)
                    {
                        int maxCount = 3;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                        canTargetCondition: (cardSource) => CanSelectCardCondition(cardSource),
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        message: "Select cards to place at the top of the deck\n(cards will be placed back to the top of the deck so that cards with lower numbers are on top).",
                        maxCount: maxCount,
                        canEndNotMax: false,
                        isShowOpponent: false,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: null);

                        selectCardEffect.SetNotAddLog();
                        selectCardEffect.SetUpCustomMessage_ShowCard("Deck Top Cards");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }
                }

                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                {
                    if (cardSources.Count == 3)
                    {
                        cardSources.Reverse();

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryTopCards(cardSources));

                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRush(
                                targetPermanent: card.PermanentOfThisCard(),
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass));

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotBeBlocked(
                                targetPermanent: card.PermanentOfThisCard(),
                                defenderCondition: null,
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass,
                                effectName: "Can't be Blocked"));
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<Rush>, Can't be Blocked", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By returning 3 cards with the [D-Brigade] or [DigiPolice] trait from your trash to the top of the deck, this Digimon gains <Rush>, and can't be blocked for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.TrashCards.Count(CanSelectCardCondition) >= 3)
                    {
                        int maxCount = 3;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                        canTargetCondition: (cardSource) => CanSelectCardCondition(cardSource),
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        message: "Select cards to place at the top of the deck\n(cards will be placed back to the top of the deck so that cards with lower numbers are on top).",
                        maxCount: maxCount,
                        canEndNotMax: false,
                        isShowOpponent: false,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: null);

                        selectCardEffect.SetNotAddLog();
                        selectCardEffect.SetUpCustomMessage_ShowCard("Deck Top Cards");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }
                }

                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                {
                    if (cardSources.Count == 3)
                    {
                        cardSources.Reverse();

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryTopCards(cardSources));

                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRush(
                                targetPermanent: card.PermanentOfThisCard(),
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass));

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotBeBlocked(
                                targetPermanent: card.PermanentOfThisCard(),
                                defenderCondition: null,
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass,
                                effectName: "Can't be Blocked"));
                        }
                    }
                }
            }

            #endregion

            #region Inherited Effect

            if (timing == EffectTiming.None)
            {
                string InheritedEffectDiscription()
                {
                    return "[All Turns] All of your other Digimon with the [D-Brigade] or [DigiPolice] trait get +1000 DP.";
                }

                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent != card.PermanentOfThisCard())
                        {
                            if (permanent.TopCard.CardTraits.Contains("D-Brigade") || permanent.TopCard.CardTraits.Contains("DigiPolice"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                permanentCondition: PermanentCondition,
                changeValue: 1000,
                isInheritedEffect: true,
                card: card,
                condition: Condition,
                effectName: InheritedEffectDiscription));
            }

            #endregion

            return cardEffects;
        }
    }
}