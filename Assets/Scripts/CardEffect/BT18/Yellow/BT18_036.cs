using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT18
{
    public class BT18_036 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region When Digivolving
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 security to draw and gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By trashing your top security card, [Draw 1] and gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if(CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (card.Owner.SecurityCards.Count >= 1)
                            return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());

                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));

                    yield return ContinuousController.instance.StartCoroutine(
                        new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }
            #endregion
            
            #region All Turns - Inherited

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 security to stop this Digimon from leaving Battle Area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("TrashSecurityToStay_Wizardmon_BT18_036");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When this yellow Digimon with the [Data] or [Witchelny] trait would leave the battle area by your opponent's effect, by trashing your top security card, it doesn't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card))
                        {
                            if (CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOpponentEffect(cardEffect, card)))
                                return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (card.PermanentOfThisCard().TopCard.CardColors.Contains(CardColor.Yellow) && 
                            (card.PermanentOfThisCard().TopCard.CardTraits.Contains("Data") || card.PermanentOfThisCard().TopCard.CardTraits.Contains("Witchelny")))
                        {
                            if (card.Owner.SecurityCards.Count >= 1 )
                                return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card) || card.Owner.SecurityCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner,
                            destroySecurityCount: 1,
                            cardEffect: activateClass,
                            fromTop: true).DestroySecurity());


                        Permanent thisCardPermanent = card.PermanentOfThisCard();

                        thisCardPermanent.willBeRemoveField = false;

                        thisCardPermanent.HideDeleteEffect();
                        thisCardPermanent.HideHandBounceEffect();
                        thisCardPermanent.HideDeckBounceEffect();
                        thisCardPermanent.HideWillRemoveFieldEffect();

                        yield return null;
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}