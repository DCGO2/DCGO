using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR;

//Haru Shinkai
namespace DCGO.CardEffects.P
{
    public class P_217 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal 3, add 2, bot deck rest", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Reveal the top 3 cards of your deck, Add 1 [Social] trait card and 1 [Creation], [Navi] or [Tool] trait card among them to the hand. Return the rest to the bottom of the deck.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("Social");
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("Creation")
                        || cardSource.EqualsTraits("Navi")
                        || cardSource.EqualsTraits("Tool");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition:CanSelectCardCondition,
                                message: "Select 1 card with the [Social] trait.",
                                mode: SelectCardEffect.Mode.AddHand,
                                maxCount: 1,
                                selectCardCoroutine: null),
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition:CanSelectCardCondition1,
                                message: "Select 1 card with the [Creation], [Navi], or [Tool] trait.",
                                mode: SelectCardEffect.Mode.AddHand,
                                maxCount: 1,
                                selectCardCoroutine: null),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                        activateClass: activateClass
                    ));
                }
            }

            #endregion

            #region Your Turn - When Linked
            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend to gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When any of your Digimon get linked to a [Social], [Navi], or [Tool] trait card, by suspending this Tamer, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerWhenLinked(hashtable, LinkPermanentCondition, LinkCardCondition);
                }
    
                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }
    
                bool LinkPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool LinkCardCondition(CardSource source)
                {
                    return source.EqualsTraits("Social")
                        || source.EqualsTraits("Navi")
                        || source.EqualsTraits("Tool");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());
                    if (card.Owner.CanAddMemory(activateClass))
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
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
