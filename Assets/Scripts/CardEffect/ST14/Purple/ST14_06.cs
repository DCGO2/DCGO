using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ST14_06 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Trash 3 cards from deck top", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Trash the top 3 cards of your deck.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.LibraryCards.Count >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(3, card.Owner, activateClass).AddTrashCardsFromLibraryTop());
            }
        }

        if (timing == EffectTiming.None)
        {
            bool Condition()
            {
                if (CardEffectCommons.IsOwnerTurn(card))
                {
                    if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("Wizard"))
                    {
                        return true;
                    }

                    if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("Demon Lord"))
                    {
                        return true;
                    }

                    if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("DemonLord"))
                    {
                        return true;
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 2000, isInheritedEffect: true, card: card, condition: Condition));
        }

        return cardEffects;
    }
}
