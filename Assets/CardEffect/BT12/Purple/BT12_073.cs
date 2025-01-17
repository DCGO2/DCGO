using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT12
{
    public class BT12_073 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("Impmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 0,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 option card from hand to return 1 Digimon card from trash to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By trashing 1 Option card in your hand, you may return 1 Digimon card with an [Evil], [Wizard], or [Demon Lord] trait from your trash to your hand.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsOption;
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.CardTraits.Contains("Evil"))
                        {
                            return true;
                        }

                        if (cardSource.CardTraits.Contains("Wizard"))
                        {
                            return true;
                        }

                        if (cardSource.CardTraits.Contains("Demon Lord"))
                        {
                            return true;
                        }

                        if (cardSource.CardTraits.Contains("DemonLord"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.HandCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                    {
                        bool discarded = false;

                        int discardCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: discardCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage(
                            "Select 1 option card to trash.",
                            "The opponent is selecting 1 option card to trash."
                            );
                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                discarded = true;

                                yield return null;
                            }
                        }

                        if (discarded)
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition1))
                            {
                                int maxCount = Math.Min(1, card.Owner.TrashCards.Count(CanSelectCardCondition1));

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition1,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 Digimon card to add to your hand.",
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
                    }
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 option card from hand to return 1 Digimon card from trash to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By trashing 1 Option card in your hand, you may return 1 Digimon card with an [Evil], [Wizard], or [Demon Lord] trait from your trash to your hand.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsOption;
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.CardTraits.Contains("Evil"))
                        {
                            return true;
                        }

                        if (cardSource.CardTraits.Contains("Wizard"))
                        {
                            return true;
                        }

                        if (cardSource.CardTraits.Contains("Demon Lord"))
                        {
                            return true;
                        }

                        if (cardSource.CardTraits.Contains("DemonLord"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.HandCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                    {
                        bool discarded = false;

                        int discardCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: discardCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage(
                            "Select 1 option card to trash.",
                            "The opponent is selecting 1 option card to trash."
                            );
                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                discarded = true;

                                yield return null;
                            }
                        }

                        if (discarded)
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition1))
                            {
                                int maxCount = Math.Min(1, card.Owner.TrashCards.Count(CanSelectCardCondition1));

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition1,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 Digimon card to add to your hand.",
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
                    }
                }
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 2 cards from deck top", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("TrashDeck_BT12_073");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking][Once Per Turn] Trash the top 2 cards of your deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(2, card.Owner, activateClass).AddTrashCardsFromLibraryTop());
                }
            }

            return cardEffects;
        }
    }
}