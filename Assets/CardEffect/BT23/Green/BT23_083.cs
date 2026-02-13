using System.Collections;
using System.Collections.Generic;

// Fei
namespace DCGO.CardEffects.BT23
{
    public class BT23_083 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                bool PermamentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasRoyalBaseTraits || permanent.TopCard.HasCSTraits;
                }

                bool Condition()
                {
                    return CardEffectCommons.IsOwnerTurn(card);
                }

                cardEffects.Add(CardEffectFactory.Gain1MemoryTamerOwnerDigimonConditionalEffect(
                    effectDescription: "[Start of Your Main Phase] If you have a Digimon with the [Royal Base] or [CS] trait, gain 1 memory.",
                    permamentCondition: PermamentCondition,
                    condition: Condition,
                    card: card));
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnAddSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspending this tamer, Gain 1 memory. then if you has 7- in hand, draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When cards are placed face up in your security stack, if any of them have the [Zaxon] or [Royal Base] trait, by suspending this Tamer, gain 1 memory. Then, if you have 7 or fewer cards in your hand, <Draw 1>";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    List<CardSource> securityCards = CardEffectCommons.GetCardSourcesFromHashtable(hashtable).Filter(SecurityCondition);

                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenAddSecurity(hashtable, player => player == card.Owner)
                        && securityCards.Count > 0;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool SecurityCondition(CardSource cardSource)
                {
                    return !cardSource.IsFlipped && 
                           (cardSource.HasRoyalBaseTraits || cardSource.EqualsTraits("Zaxon"));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    if (card.Owner.CanAddMemory(activateClass)) yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                    if (card.Owner.HandCards.Count <= 7) yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
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