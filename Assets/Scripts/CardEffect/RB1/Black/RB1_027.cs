using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RB1_027 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Reveal the top card of opponent's security", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] Reveal the top card of your opponent's security stack. If that card is a Digimon card, gain 1 memory. If it's a non-Digimon card, <Draw 1>. Place the revealed card at the top or bottom of your opponent's security stack face down.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.Enemy.SecurityCards.Count >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (card.Owner.Enemy.SecurityCards.Count >= 1)
                {
                    CardSource topCard = card.Owner.Enemy.SecurityCards[0];

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { topCard }, "Security Top card", true, true));

                    if (topCard.IsDigimon)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                    }

                    else
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    }

                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Security Top", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Security Bottom", value : false, spriteIndex: 1),
                        };

                    string selectPlayerMessage = "Do you place the card on the top or bottom of the security?";
                    string notSelectPlayerMessage = "The opponent is choosing whether to place the card on the top or bottom of security.";

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool toTop = GManager.instance.userSelectionManager.SelectedBoolValue;

                    if (toTop)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(new List<CardSource>() { topCard }, "Card placed at Security Top", true, true));
                    }

                    else
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(new List<CardSource>() { topCard }, "Card placed at Security Bottom", true, true));

                        topCard.Owner.SecurityCards.Remove(topCard);
                        topCard.Owner.SecurityCards.Add(topCard);
                    }

                    topCard.SetReverse();
                }
            }
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Reveal the top card of opponent's security", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Reveal the top card of your opponent's security stack. If that card is a Digimon card, gain 1 memory. If it's a non-Digimon card, <Draw 1>. Place the revealed card at the top or bottom of your opponent's security stack face down.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.Enemy.SecurityCards.Count >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (card.Owner.Enemy.SecurityCards.Count >= 1)
                {
                    CardSource topCard = card.Owner.Enemy.SecurityCards[0];

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { topCard }, "Security Top card", true, true));

                    if (topCard.IsDigimon)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                    }

                    else
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    }

                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Security Top", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Security Bottom", value : false, spriteIndex: 1),
                        };

                    string selectPlayerMessage = "Do you place the card on the top or bottom of the security?";
                    string notSelectPlayerMessage = "The opponent is choosing whether to place the card on the top or bottom of security.";

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool toTop = GManager.instance.userSelectionManager.SelectedBoolValue;

                    if (toTop)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(new List<CardSource>() { topCard }, "Card placed at Security Top", true, true));
                    }

                    else
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(new List<CardSource>() { topCard }, "Card placed at Security Bottom", true, true));

                        topCard.Owner.SecurityCards.Remove(topCard);
                        topCard.Owner.SecurityCards.Add(topCard);
                    }

                    topCard.SetReverse();
                }
            }
        }

        if (timing == EffectTiming.None)
        {
            bool Condition()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionPermanent((permanent) => permanent.IsTamer))
                    {
                        return true;
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: Condition));
        }

        if (timing == EffectTiming.None)
        {
            bool PermanentCondition(Permanent permanent)
            {
                return permanent == card.PermanentOfThisCard();
            }

            bool CardEffectCondition(ICardEffect cardEffect)
            {
                return CardEffectCommons.IsOpponentEffect(cardEffect, card);
            }

            bool CanUseCondition()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionPermanent((permanent) => permanent.IsTamer))
                    {
                        return true;
                    }
                }

                return false;
            }

            string effectName = "This Digimon can't be deleted by opponent's effects";

            cardEffects.Add(CardEffectFactory.CanNotBeDestroyedBySkillStaticEffect(
                permanentCondition: PermanentCondition,
                cardEffectCondition: CardEffectCondition,
                isInheritedEffect: false,
                card: card,
                condition: CanUseCondition,
                effectName: effectName
            ));
        }

        return cardEffects;
    }
}
