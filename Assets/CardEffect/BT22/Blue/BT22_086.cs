using System;
using System.Collections;
using System.Collections.Generic;

// Yao Qinglan
namespace DCGO.CardEffects.BT22
{
    public class BT22_086 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Bottom deck this tamer, play 1 [Yao Qinglan] from hand, Then if no digimon play 1 [Sangomon] from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] By returning this Tamer to the bottom of the deck, you may play 1 [Yao Qinglan] from your hand without paying the cost. Then, if you don't have a Digimon, you may play 1 [Sangomon] from your trash without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool BottomDeckCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && permanent == card.PermanentOfThisCard();
                }

                bool IsYaoQinglan(CardSource cardSource)
                {
                    return CardEffectCommons.IsExistOnHand(cardSource)
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass)
                        && cardSource.EqualsCardName("Yao Qinglan");
                }

                bool IsSangomon(CardSource cardSource)
                {
                    return CardEffectCommons.IsExistOnTrash(cardSource)
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass)
                        && cardSource.EqualsCardName("Sangomon");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    #region Bottom Deck Yao Qinglan

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, BottomDeckCondition));
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: BottomDeckCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 tamer to bottom deck.", "The opponent is selecting a tamer to bottom deck.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    #endregion

                    if (!CardEffectCommons.IsExistOnField(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, IsYaoQinglan))
                        {
                            #region Play Yao Qinglan

                            CardSource yaoQinglan = null;
                            int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, IsYaoQinglan));

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsYaoQinglan,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                yaoQinglan = cardSource;
                                yield return null;
                            }

                            selectHandEffect.SetUpCustomMessage("Select 1 [Yao Qinglan] to play.", "The opponent is selecting 1 [Yao Qinglan] to play.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                            yield return StartCoroutine(selectHandEffect.Activate());

                            if (yaoQinglan != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: new List<CardSource>() { yaoQinglan },
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.Hand,
                                activateETB: true
                            ));

                            #endregion
                        }

                        if (card.Owner.GetBattleAreaDigimons().Count == 0 && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsSangomon))
                        {
                            #region Play Sangomon From Trash

                            CardSource sangomon = null;
                            int maxCount2 = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, IsSangomon));
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                            selectCardEffect.SetUp(
                                        canTargetCondition: IsSangomon,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => false,
                                        selectCardCoroutine: SelectCardCoroutine1,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 [Sangomon] to play",
                                        maxCount: maxCount2,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Trash,
                                        customRootCardList: null,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                            IEnumerator SelectCardCoroutine1(CardSource cardSource)
                            {
                                sangomon = cardSource;
                                yield return null;
                            }

                            selectCardEffect.SetUpCustomMessage("Select 1 [Sangomon] to play", "The opponent is selecting 1 [Sangomon] to play.");
                            yield return StartCoroutine(selectCardEffect.Activate());

                            if (sangomon != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: new List<CardSource>() { sangomon },
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.Trash,
                                activateETB: true
                            ));

                            #endregion
                        }
                    }
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnAddDigivolutionCards)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend tamer, Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When effects add digivolution cards to any of your Digimon with [Aqua] or [Sea Animal] in any of their traits, by suspending this Tamer, <Draw 1> (Draw 1 card from your deck.)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAddDigivolutionCard(hashtable, IsAquaOrSeaAnimalTrait, null, null);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool IsAquaOrSeaAnimalTrait(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.HasAquaTraits || permanent.TopCard.HasSeaAnimalTraits);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, hashtable).Tap());
                    if (card.PermanentOfThisCard().IsSuspended) yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}