using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT13
{
    public class BT13_092 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                AddBurstDigivolutionConditionClass addBurstDigivolutionConditionClass = new AddBurstDigivolutionConditionClass();
                addBurstDigivolutionConditionClass.SetUpICardEffect($"Burst Digivolution", CanUseCondition, card);
                addBurstDigivolutionConditionClass.SetUpAddBurstDigivolutionConditionClass(getBurstDigivolutionCondition: GetBurstDigivolution);
                addBurstDigivolutionConditionClass.SetNotShowUI(true);
                cardEffects.Add(addBurstDigivolutionConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                BurstDigivolutionCondition GetBurstDigivolution(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        bool tamerCondition(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                        {
                                            if (!permanent.CannotReturnToHand(null))
                                            {
                                                if (permanent.TopCard.CardNames.Contains("Keenan Crier"))
                                                {
                                                    return true;
                                                }

                                                if (permanent.TopCard.CardNames.Contains("KeenanCrier"))
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        bool digimonCondition(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.TopCard.Owner.GetFieldPermanents().Contains(permanent))
                                        {
                                            if (!card.CanNotEvolve(permanent))
                                            {
                                                if (permanent.TopCard.CardNames.Contains("Ravemon"))
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        BurstDigivolutionCondition burstDigivolutionCondition = new BurstDigivolutionCondition(
                            tamerCondition: tamerCondition,
                            selectTamerMessage: "1 [Keenan Crier]",
                            digimonCondition: digimonCondition,
                            selectDigimonMessage: "1 [Ravemon]",
                            cost: 0);

                        return burstDigivolutionCondition;
                    }

                    return null;
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 opponent's hand and add the top card of opponent's security to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Search your opponent's hand, and trash 1 card among it. Then, if they have 7 or fewer cards in their hand, they add the top card of their security stack to the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            if (card.Owner.Enemy.HandCards.Count >= 1)
                            {
                                return true;
                            }

                            if (card.Owner.Enemy.HandCards.Count <= 7)
                            {
                                if (card.Owner.Enemy.SecurityCards.Count >= 1)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            if (card.Owner.Enemy.HandCards.Count >= 1)
                            {
                                int maxCount = 1;

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: (cardSource) => true,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 card to trash in opponent's hand.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Discard,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: card.Owner.Enemy.HandCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                            }

                            if (card.Owner.Enemy.HandCards.Count <= 7)
                            {
                                if (card.Owner.Enemy.SecurityCards.Count >= 1)
                                {
                                    CardSource topCard = card.Owner.Enemy.SecurityCards[0];

                                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { topCard }, false, activateClass));

                                    yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                                        player: card.Owner.Enemy,
                                        refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return opponent's 1 card from trash to the bottom of deck to delete opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] By returning 1 Digimon card from your opponent's trash to the bottom of the deck, delete all of your opponent's Digimon with the same name as that card.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner.Enemy)
                        {
                            if (cardSource.IsDigimon)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.Enemy.TrashCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    int maxCount = 1;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                    canTargetCondition: (cardSource) => CanSelectCardCondition(cardSource),
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    canNoSelect: () => true,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: AfterSelectCardCoroutine,
                    message: "Select 1 card in oppopnent's trash to place at the bottom of the deck.",
                    maxCount: maxCount,
                    canEndNotMax: false,
                    isShowOpponent: false,
                    mode: SelectCardEffect.Mode.Custom,
                    root: SelectCardEffect.Root.Custom,
                    customRootCardList: card.Owner.Enemy.TrashCards,
                    canLookReverseCard: true,
                    selectPlayer: card.Owner,
                    cardEffect: activateClass);

                    selectCardEffect.SetNotShowCard();
                    selectCardEffect.SetNotAddLog();

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count == 1)
                        {
                            CardSource returnedCard = cardSources[0];

                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(cardSources));

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(cardSources, "Deck Bottom Card", true, true));

                            if (returnedCard != null)
                            {
                                List<Permanent> destroyedPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Where((permanent1) => permanent1.TopCard.HasSameCardName(returnedCard)).ToList();

                                yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(destroyedPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}