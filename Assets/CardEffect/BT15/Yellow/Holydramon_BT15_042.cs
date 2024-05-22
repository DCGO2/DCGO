using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class Holydramon_BT15_042 : CEntity_Effect
{
  public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
  {
    List<ICardEffect> cardEffects = new List<ICardEffect>();

    if (timing == EffectTiming.OnEnterFieldAnyone)
    {
      ActivateClass activateClass = new ActivateClass();
      activateClass.SetUpICardEffect("Trash the top card of your security so that opponent's 1 Digimon gains DP -9000 until the end of their turn", CanUseCondition, card);
      activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
      cardEffects.Add(activateClass);

      string EffectDiscription()
      {
        return "[On Play] You may trash the top card of your security stack to unsuspend this Digimon.";
      }

      bool CanSelectPermanentCondition(Permanent permanent)
      {
        return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
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
          List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                    {
                        new SelectionElement<bool>(message: $"Security Top", value : true, spriteIndex: 0),
                        new SelectionElement<bool>(message: $"Security Bottom", value : false, spriteIndex: 1),
                    };

          string selectPlayerMessage = "Which will you trash the top or bottom card of the security?";
          string notSelectPlayerMessage = "The opponent is selecting whether to trash the top or bottom card of security.";

          GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

          yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

          bool fromTop = GManager.instance.userSelectionManager.SelectedBoolValue;

          yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                  card.Owner,
                  1,
                  activateClass,
                  fromTop).DestroySecurity());

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
                selectPermanentCoroutine: SelectPermanentCoroutine,
                afterSelectPermanentCoroutine: null,
                mode: SelectPermanentEffect.Mode.Custom,
                cardEffect: activateClass);

            selectPermanentEffect.SetUpCustomMessage(
                "Select 1 Digimon that will get DP -9000.",
                "The opponent is selecting 1 Digimon that will get DP -9000.");

            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

            IEnumerator SelectPermanentCoroutine(Permanent permanent)
            {
              yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                targetPermanent: permanent,
                changeValue: -9000,
                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                activateClass: activateClass));
            }
          }
        }
      }
    }

    if (timing == EffectTiming.OnEnterFieldAnyone)
    {
      ActivateClass activateClass = new ActivateClass();
      activateClass.SetUpICardEffect("Trash the top card of your security so that opponent's 1 Digimon gains DP -9000", CanUseCondition, card);
      activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
      cardEffects.Add(activateClass);

      string EffectDiscription()
      {
        return "[When Digivolving] You may trash the top card of your security stack to unsuspend this Digimon.";
      }

      bool CanSelectPermanentCondition(Permanent permanent)
      {
        return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
      }

      bool CanUseCondition(Hashtable hashtable)
      {
        return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
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
          List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                    {
                        new SelectionElement<bool>(message: $"Security Top", value : true, spriteIndex: 0),
                        new SelectionElement<bool>(message: $"Security Bottom", value : false, spriteIndex: 1),
                    };

          string selectPlayerMessage = "Which will you trash the top or bottom card of the security?";
          string notSelectPlayerMessage = "The opponent is selecting whether to trash the top or bottom card of security.";

          GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

          yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

          bool fromTop = GManager.instance.userSelectionManager.SelectedBoolValue;

          yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                  card.Owner,
                  1,
                  activateClass,
                  fromTop).DestroySecurity());

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
                selectPermanentCoroutine: SelectPermanentCoroutine,
                afterSelectPermanentCoroutine: null,
                mode: SelectPermanentEffect.Mode.Custom,
                cardEffect: activateClass);

            selectPermanentEffect.SetUpCustomMessage(
                "Select 1 Digimon that will get DP -9000.",
                "The opponent is selecting 1 Digimon that will get DP -9000.");

            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

            IEnumerator SelectPermanentCoroutine(Permanent permanent)
            {
              yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                targetPermanent: permanent,
                changeValue: -9000,
                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                activateClass: activateClass));
            }
          }
        }
      }
    }

    if (timing == EffectTiming.OnLoseSecurity)
    {
      ActivateClass activateClass = new ActivateClass();
      activateClass.SetUpICardEffect("Place 1 card from hand to the top or bottom of the security", CanUseCondition, card);
      activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
      activateClass.SetHashString("Place1CardFromHandToSecurity_BT15_042");
      cardEffects.Add(activateClass);

      string EffectDiscription()
      {
        return "[All Turns][Once Per Turn] When a card is removed from your security stack, if you have 3 or fewer security cards, trigger <Recovery +1 (Deck)>. (Place the top card of your deck on top of your security stack.)";
      }

      bool CanSelectCardCondition(CardSource cardSource)
      {
        return cardSource.CardColors.Contains(CardColor.Yellow);
      }

      bool CanUseCondition(Hashtable hashtable)
      {
        if (CardEffectCommons.IsExistOnBattleArea(card))
        {
          if (CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, player => player == card.Owner))
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
          if (card.Owner.SecurityCards.Count <= 3)
          {
            if (card.Owner.HandCards.Count >= 1)
            {
              if (card.Owner.CanAddSecurity(activateClass))
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
        if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
        {
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
                selectCardCoroutine: SelectCardCoroutine,
                afterSelectCardCoroutine: null,
                mode: SelectHandEffect.Mode.Custom,
                cardEffect: activateClass);

            selectHandEffect.SetUpCustomMessage(
                "Select 1 card to place on the security.",
                "The opponent is selecting 1 card to place on the security.");

            yield return StartCoroutine(selectHandEffect.Activate());

            IEnumerator SelectCardCoroutine(CardSource cardSource)
            {
              selectedCards.Add(cardSource);

              yield return null;
            }

            foreach (CardSource cardSource in selectedCards)
            {
              List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                    {
                        new SelectionElement<bool>(message: $"Security Top", value : true, spriteIndex: 0),
                        new SelectionElement<bool>(message: $"Security Bottom", value : false, spriteIndex: 1),
                    };

              string selectPlayerMessage = "Which will you place the card on the top or bottom card of the security?";
              string notSelectPlayerMessage = "The opponent is selecting whether to place the card on the top or bottom card of security.";

              GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

              yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

              bool toTop = GManager.instance.userSelectionManager.SelectedBoolValue;

              yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(cardSource));

              yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateRecoveryEffect(cardSource.Owner));

              yield return ContinuousController.instance.StartCoroutine(new IAddSecurity(cardSource.Owner).AddSecurity());

              if (!toTop)
              {
                cardSource.Owner.SecurityCards.Remove(cardSource);
                cardSource.Owner.SecurityCards.Add(cardSource);
              }
            }
          }
        }
      }
    }

    return cardEffects;
  }
}
