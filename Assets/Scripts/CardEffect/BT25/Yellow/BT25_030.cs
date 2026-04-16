using System.Collections;
using System.Collections.Generic;

// Elecmon
namespace DCGO.CardEffects.BT25
{
    public class BT25_030 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolve Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 0, false, card, null, level: 2));
            }
            #endregion

            #region On Start of Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By adding the top security card to hand, gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] By adding your top security card to the hand, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.SecurityCards.Count > 0)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Add to hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Not add to hand", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Will you add the card to hand?";
                        string notSelectPlayerMessage = "The opponent is choosing whether to add the card to hand.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool addHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (addHand)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { card.Owner.SecurityCards[0] }, false, activateClass));
                            yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                                player: card.Owner,
                                refSkillInfos: ref ContinuousController.instance.nullSkillInfos,
                                activateClass).ReduceSecurity());
                            yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                        }
                    }
                }
            }
            #endregion

            #region When Attacking - Inherited

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May add top sec card to hand, if at 0 <Recovery +1>.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT25_030_Inherited");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] [Once Per Turn] You may add your top security card to the hand. Then, if you have 0 security cards, <Recovery +1 (Deck)>.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card)
                        && CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.SecurityCards.Count > 0)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Add to hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Not add to hand", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Will you add the card to hand?";
                        string notSelectPlayerMessage = "The opponent is choosing whether to add the card to hand.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool addHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (addHand)
                        {
                            CardSource topCard = card.Owner.SecurityCards[0];

                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { topCard }, false, activateClass));

                            yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                                player: card.Owner,
                                refSkillInfos: ref ContinuousController.instance.nullSkillInfos,
                                activateClass).ReduceSecurity());
                        }
                    }

                    if (card.Owner.SecurityCards.Count == 0)
                        yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}