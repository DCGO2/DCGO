using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;


public class P_115 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnDestroyedAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Play 1 [Nene Amano] or [Yuu Amano] from hand or trash, and save", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Deletion] You may play 1 tamer card with [Nene Amano]/[Yuu Amano] in its name from your hand or trash without paying the cost. Then, <Save> (You may place this card under one of your Tamers).";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource != null)
                {
                    if (cardSource.Owner == card.Owner)
                    {
                        if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                        {
                            if (cardSource.ContainsCardName("Nene Amano"))
                            {
                                return true;
                            }

                            if (cardSource.ContainsCardName("Yuu Amano"))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (permanent != null)
                {
                    if (permanent.TopCard != null)
                    {
                        if (permanent.IsTamer)
                        {
                            if (permanent.TopCard.Owner == card.Owner)
                            {
                                if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                {
                                    if (!permanent.IsToken)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.CanActivateOnDeletion(card))
                {
                    if (card.Owner.HandCards.Count >= 1)
                    {
                        return true;
                    }

                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        return true;
                    }
                }

                if (CardEffectCommons.CanActivateSave(hashtable, CanSelectPermanentCondition))
                {
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1 || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                {
                    if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1 && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        if (card.Owner.isYou)
                        {
                            GManager.instance.commandText.OpenCommandText("From which area do you play a card?");

                            List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>()
                                {
                                    new Command_SelectCommand($"From hand", () => photonView.RPC("SetFromHand", RpcTarget.All, true), 0),
                                    new Command_SelectCommand($"From trash", () => photonView.RPC("SetFromHand", RpcTarget.All, false), 1),
                                };

                            GManager.instance.selectCommandPanel.SetUpCommandButton(command_SelectCommands);
                        }

                        else
                        {
                            GManager.instance.commandText.OpenCommandText("The opponent is choosing from which area to play a card.");

                            #region AIモード
                            if (GManager.instance.IsAI)
                            {
                                SetFromHand(RandomUtility.IsSucceedProbability(0.5f));
                            }
                            #endregion
                        }
                    }

                    else if (card.Owner.HandCards.Count(CanSelectCardCondition) == 0 && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        SetFromHand(false);
                    }

                    else if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1 && card.Owner.TrashCards.Count(CanSelectCardCondition) == 0)
                    {
                        SetFromHand(true);
                    }

                    yield return new WaitWhile(() => !endSelect);
                    endSelect = false;

                    GManager.instance.commandText.CloseCommandText();
                    yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                    List<CardSource> selectedCards = new List<CardSource>();

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    if (fromHand)
                    {
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

                        selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectHandEffect.Activate());
                    }

                    else
                    {
                        int maxCount = 1;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to play.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }

                    SelectCardEffect.Root root = SelectCardEffect.Root.Hand;

                    if (!fromHand)
                    {
                        root = SelectCardEffect.Root.Trash;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: selectedCards,
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: root,
                        activateETB: true));
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SaveProcess(_hashtable, activateClass, card, CanSelectPermanentCondition));
            }
        }

        if (timing == EffectTiming.None)
        {
            bool Condition()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (card.PermanentOfThisCard().Level >= 5)
                        {
                            if (card.PermanentOfThisCard().TopCard.HasLevel)
                            {
                                if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("Bagra Army"))
                                {
                                    return true;
                                }

                                if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("BagraArmy"))
                                {
                                    return true;
                                }

                                if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("Twilight"))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: true, card: card, condition: Condition));
        }

        return cardEffects;
    }

    bool endSelect = false;
    bool fromHand = false;

    [PunRPC]
    public void SetFromHand(bool fromHand)
    {
        this.fromHand = fromHand;
        endSelect = true;
    }
}
