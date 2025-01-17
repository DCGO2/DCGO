using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT14
{
    public class BT14_074 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 card from hand to Draw 1, and gain Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] By trashing 1 card in your hand, <Draw 1>. If [Eiji Nagasumi] is in this Digimon's digivolution cards, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.HandCards.Count >= 1)
                        {
                            return true;
                        }

                        if (card.PermanentOfThisCard().DigivolutionCards.Some(cardSource => cardSource.CardNames.Contains("Eiji Nagasumi")))
                        {
                            return true;
                        }

                        if (card.PermanentOfThisCard().DigivolutionCards.Some(cardSource => cardSource.CardNames.Contains("EijiNagasumi")))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.HandCards.Count >= 1)
                    {
                        bool discarded = false;

                        int discardCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: (cardSource) => true,
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
                            yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                        }
                    }

                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Some(cardSource => cardSource.CardNames.Contains("Eiji Nagasumi")) || card.PermanentOfThisCard().DigivolutionCards.Some(cardSource => cardSource.CardNames.Contains("EijiNagasumi")))
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Memory+1_BT14_074");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn][Once Per Turn] When you play a card with the [Dark Animal] or [SoC] trait, gain 1 memory.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.CardTraits.Contains("Dark Animal"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardTraits.Contains("DarkAnimal"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardTraits.Contains("SoC"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition))
                            {
                                return true;
                            }
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
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            return cardEffects;
        }
    }
}