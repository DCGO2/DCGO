using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT11
{
    public class BT11_057 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash cards from hand to suspend opponent's Digimons and gain Memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may trash up to 3 cards in your hand. If you do, for each card trashed, suspend 1 of your opponent's Digimon. Then, for each suspended Digimon your opponent has in play, gain 1 memory.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.CanSelectBySkill(activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.IsSuspended)
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
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            if (card.Owner.HandCards.Count >= 1)
                            {
                                return true;
                            }

                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                            {
                                if (card.Owner.CanAddMemory(activateClass))
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
                            if (card.Owner.HandCards.Count >= 1)
                            {
                                List<CardSource> discardedCards = new List<CardSource>();

                                int discardCount = 3;

                                if (card.Owner.HandCards.Count < discardCount)
                                {
                                    discardCount = card.Owner.HandCards.Count;
                                }

                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: (cardSource) => true,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: discardCount,
                                    canNoSelect: true,
                                    canEndNotMax: true,
                                    isShowOpponent: true,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                    mode: SelectHandEffect.Mode.Discard,
                                    cardEffect: activateClass);

                                yield return StartCoroutine(selectHandEffect.Activate());

                                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                                {
                                    if (cardSources.Count >= 1)
                                    {
                                        foreach (CardSource cardSource in cardSources)
                                        {
                                            discardedCards.Add(cardSource);
                                        }

                                        yield return null;
                                    }
                                }

                                if (discardedCards.Count >= 1)
                                {
                                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                                    {
                                        int maxCount = Math.Min(discardedCards.Count, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                        selectPermanentEffect.SetUp(
                                            selectPlayer: card.Owner,
                                            canTargetCondition: CanSelectPermanentCondition,
                                            canTargetCondition_ByPreSelecetedList: null,
                                            canEndSelectCondition: CanEndSelectCondition,
                                            maxCount: maxCount,
                                            canNoSelect: false,
                                            canEndNotMax: false,
                                            selectPermanentCoroutine: null,
                                            afterSelectPermanentCoroutine: null,
                                            mode: SelectPermanentEffect.Mode.Tap,
                                            cardEffect: activateClass);

                                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                        bool CanEndSelectCondition(List<Permanent> permanents)
                                        {
                                            if (permanents.Count <= 0)
                                            {
                                                return false;
                                            }

                                            return true;
                                        }
                                    }
                                }
                            }

                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                            {
                                if (card.Owner.CanAddMemory(activateClass))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(card.Owner.Enemy.GetBattleAreaDigimons().Count(CanSelectPermanentCondition1), activateClass));
                                }
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}