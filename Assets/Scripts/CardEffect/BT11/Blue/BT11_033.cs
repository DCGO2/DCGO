using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT11
{
    public class BT11_033 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 level 5 or lower Digimon to hand or opponent adds the top card of Security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Return 1 of your opponent's level 5 or lower Digimon to its owner's hand. If no Digimon was returned by this effect, your opponent adds the top card of their security stack to their hand.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.Level <= 5)
                        {
                            if (permanent.TopCard.HasLevel)
                            {
                                if (permanent.CanSelectBySkill(activateClass))
                                {
                                    return true;
                                }
                            }
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
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                return true;
                            }

                            if (card.Owner.Enemy.SecurityCards.Count >= 1)
                            {
                                return true;
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
                            List<Permanent> selectedPermanents = new List<Permanent>();

                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                                    mode: SelectPermanentEffect.Mode.Bounce,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                                {
                                    foreach (Permanent permanent in permanents)
                                    {
                                        selectedPermanents.Add(permanent);
                                    }

                                    yield return null;
                                }
                            }

                            bool bounced()
                            {
                                foreach (Permanent permanent in selectedPermanents)
                                {
                                    if (permanent.TopCard == null)
                                    {
                                        if (permanent.willBeRemoveField)
                                        {
                                            if (permanent.HandBounceEffect == activateClass)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }

                                return false;
                            }

                            if (!bounced())
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

            if (timing == EffectTiming.OnAddHand)
            {
                int count()
                {
                    return card.Owner.Enemy.HandCards.Count / 4;
                }

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain Memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("GainMemory_BT11_033");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns][Once Per Turn] When an effect adds cards to your opponent's hand, gain 1 memory for every 4 cards in your opponent's hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenAddHand(hashtable, player => player == card.Owner.Enemy, cardEffect => cardEffect != null))
                        {
                            return true;
                        }
                    }

                    return false;
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
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(count(), activateClass));
                }
            }

            return cardEffects;
        }
    }
}