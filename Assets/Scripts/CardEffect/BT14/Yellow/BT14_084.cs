using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT14
{
    public class BT14_084 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Add 1 card from security to hand to place 1 card from hand at the bottom of security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By returning the top card of your security stack to the hand, you may place 1 yellow card with the [Vaccine] trait from your hand at the bottom of your security stack.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("Vaccine"))
                    {
                        if (cardSource.CardColors.Contains(CardColor.Yellow))
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
                        if (card.Owner.SecurityCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        CardSource topCard = card.Owner.SecurityCards[0];

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { topCard }, false, activateClass));

                        yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                            player: card.Owner,
                            refSkillInfos: ref ContinuousController.instance.nullSkillInfos,
                            activateClass).ReduceSecurity());

                        if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            int maxCount = 1;

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine1,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectHandEffect.SetUpCustomMessage("Select 1 card to place at the bottom of security.", "The opponent is selecting 1 card to place at the bottom of security.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Security Bottom Card");

                            yield return StartCoroutine(selectHandEffect.Activate());

                            IEnumerator SelectCardCoroutine1(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            foreach (CardSource cardSource in selectedCards)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(cardSource, toTop: false));
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnAddSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When a card is added to your security stack, by suspending this Tamer, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerWhenAddSecurity(hashtable, player => player == card.Owner))
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
                        if (CardEffectCommons.CanActivateSuspendCostEffect(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            return cardEffects;
        }
    }
}