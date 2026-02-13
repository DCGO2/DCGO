using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;

namespace DCGO.CardEffects.BT19
{
    public class BT19_099 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Composite] trait Digimon card from your trash with the play cost reduced by 4.", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] You may play 1 [Composite] trait Digimon card from your trash with the play cost reduced by 4. Then, place this card in the battle area.";
                }

                bool HasCompositeTrait(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                        return cardSource.ContainsTraits("Composite");

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if(CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, HasCompositeTrait))
                    {
                        #region reduce play cost
                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect($"Play Cost -4", CanUseCondition1, card);
                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                        Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                        card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                        ICardEffect GetCardEffect(EffectTiming _timing)
                        {
                            if (_timing == EffectTiming.None)
                            {
                                return changeCostClass;
                            }

                            return null;
                        }

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            return true;
                        }

                        int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                        {
                            if (CardSourceCondition(cardSource))
                            {
                                if (RootCondition(root))
                                {
                                    if (PermanentsCondition(targetPermanents))
                                    {
                                        Cost -= 4;
                                    }
                                }
                            }

                            return Cost;
                        }

                        bool PermanentsCondition(List<Permanent> targetPermanents)
                        {
                            if (targetPermanents == null)
                            {
                                return true;
                            }

                            else
                            {
                                if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        bool CardSourceCondition(CardSource cardSource)
                        {
                            if (cardSource.HasPlayCost)
                            {
                                if (cardSource.IsDigimon)
                                    return cardSource.ContainsTraits("Composite");
                            }

                            return false;
                        }

                        bool RootCondition(SelectCardEffect.Root root)
                        {
                            return true;
                        }

                        bool isUpDown()
                        {
                            return true;
                        }
                        #endregion

                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: HasCompositeTrait,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: SelectCardCoroutine,
                            message: "Select 1 [Composite] trait Digimon card to play.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage(
                "Select 1 [Composite] trait Digimon card to play.",
                "The opponent is selecting 1 [Composite] trait Digimon card to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Play Card");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(List<CardSource> sources)
                        {
                            selectedCards = sources;
                            yield return null;
                        }

                        #region Place As Source
                        if (selectedCards.Count > 0)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: selectedCards,
                                    activateClass: activateClass,
                                    payCost: true,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Trash,
                                    activateETB: true));
                        }
                        #endregion

                        #region Remove Cost effect after use
                        card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);
                        #endregion
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }
            #endregion

            #region All Turns - Delay
            if (timing == EffectTiming.WhenRemoveField)
            {
                int playCostValue = -1;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Wicked God] trait Digimon card from hand or trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When any of your Digimon with [Millenniummon] in its name would leave the battle area, <Delay>.\r\n• You may play 1 [Wicked God] trait Digimon card with a play cost 1 greater than that Digimon from your hand or trash without paying the cost.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsOwnerPermanent(permanent, card))
                    {
                        return permanent.TopCard.ContainsCardName("Millenniummon");
                    }

                    return false;
                }            

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, PermanentCondition))
                        {
                            if (CardEffectCommons.CanDeclareOptionDelayEffect(card))
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
                        if (card.PermanentOfThisCard().CanBeDestroyedBySkill(activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool deleted = false;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass,
                        successProcess: permanents => SuccessProcess(),
                        failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        deleted = true;

                        yield return null;
                    }

                    if (deleted)
                    {
                        List<Permanent> selectedPermanents = CardEffectCommons.GetPermanentsFromHashtable(_hashtable);

                        bool CanPlayWickedGodCondition(CardSource cardSource)
                        {
                            foreach (Permanent permanent in selectedPermanents)
                            {
                                if (PermanentCondition(permanent))
                                {
                                    if(permanent.TopCard.GetCostItself + 1 == cardSource.GetCostItself)
                                    {
                                        if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                                        {
                                            return cardSource.IsDigimon &&
                                                   cardSource.ContainsTraits("Wicked God");
                                        }
                                    }
                                }
                            }                         

                            return false;
                        }

                        if (selectedPermanents.Count > 0)
                        {
                            if (card.Owner.HandCards.Count(CanPlayWickedGodCondition) >= 1 || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanPlayWickedGodCondition))
                            {
                                if (card.Owner.HandCards.Count(CanPlayWickedGodCondition) >= 1 && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanPlayWickedGodCondition))
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

                                else if (card.Owner.HandCards.Count(CanPlayWickedGodCondition) == 0 && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanPlayWickedGodCondition))
                                {
                                    SetFromHand(false);
                                }

                                else if (card.Owner.HandCards.Count(CanPlayWickedGodCondition) >= 1 && card.Owner.TrashCards.Count(CanPlayWickedGodCondition) == 0)
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
                                        canTargetCondition: CanPlayWickedGodCondition,
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
                                        "Select 1 [Wicked God] trait Digimon card to play.",
                                        "The opponent is selecting 1 [Wicked God] trait Digimon card to play.");
                                    selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                                    yield return StartCoroutine(selectHandEffect.Activate());
                                }
                                else
                                {
                                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                    selectCardEffect.SetUp(
                                        canTargetCondition: CanPlayWickedGodCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => false,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 [Wicked God] trait Digimon card to play.",
                                        maxCount: 1,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Trash,
                                        customRootCardList: null,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                                    selectCardEffect.SetUpCustomMessage(
                                        "Select 1 [Wicked God] trait Digimon card to play.",
                                        "The opponent is selecting 1 [Wicked God] trait Digimon card to play.");
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
                        }
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaceSelfDelayOptionSecurityEffect(card));
            }
            #endregion

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
}