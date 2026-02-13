using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.EX2
{
    public class EX2_007 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                cardEffects.Add(CardEffectFactory.CanNotAttackSelfStaticEffect(
                    defenderCondition: null,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition,
                    effectName: "Can't Attack"));
            }

            if (timing == EffectTiming.None)
            {
                CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's effects", CanUseCondition, card);
                canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition1, SkillCondition: SkillCondition);

                cardEffects.Add(canNotAffectedClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                bool CardCondition1(CardSource cardSource)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card == card.PermanentOfThisCard().TopCard)
                        {
                            if (cardSource == card.PermanentOfThisCard().TopCard)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool SkillCondition(ICardEffect cardEffect)
                {
                    return CardEffectCommons.IsOpponentEffect(cardEffect, card);
                }
            }

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 card under this Digimon's digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main][Once Per Turn] If you don't have another [Mother D-Reaper] in play, place 1 of your [ADR-02 Searcher] from in play or from your hand under this Digimon as its bottom digivolution card.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (!permanent.IsToken)
                        {
                            if (permanent.TopCard.CardNames.Contains("ADR-02 Searcher"))
                            {
                                return true;
                            }

                            if (permanent.TopCard.CardNames.Contains("ADR-02Searcher"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (!cardSource.IsToken)
                    {
                        if (cardSource.CardNames.Contains("ADR-02 Searcher"))
                        {
                            return true;
                        }

                        if (cardSource.CardNames.Contains("ADR-02Searcher"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (!card.PermanentOfThisCard().IsToken)
                        {
                            if (card.Owner.GetBattleAreaDigimons().Count((permanent) => (permanent.TopCard.CardNames.Contains("Mother D-Reaper") || permanent.TopCard.CardNames.Contains("MotherD-Reaper")) && permanent != card.PermanentOfThisCard()) == 0)
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                                {
                                    return true;
                                }

                                if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
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
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition) || card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition) && card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                                {
                                    if (card.Owner.isYou)
                                    {
                                        GManager.instance.commandText.OpenCommandText("From which area do you select a card?");

                                        List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>()
                                {
                                    new Command_SelectCommand($"From Field", () => photonView.RPC("SetFromHand", RpcTarget.All, false), 0),
                                    new Command_SelectCommand($"From hand", () => photonView.RPC("SetFromHand", RpcTarget.All, true), 1),
                                };

                                        GManager.instance.selectCommandPanel.SetUpCommandButton(command_SelectCommands);
                                    }

                                    else
                                    {
                                        GManager.instance.commandText.OpenCommandText("The opponent is choosing from which area to select a card.");

                                        #region AI
                                        if (GManager.instance.IsAI)
                                        {
                                            SetFromHand(RandomUtility.IsSucceedProbability(0.5f));
                                        }
                                        #endregion
                                    }
                                }

                                else if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition) && card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                                {
                                    SetFromHand(true);
                                }

                                else if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition) && card.Owner.HandCards.Count(CanSelectCardCondition) == 0)
                                {
                                    SetFromHand(false);
                                }

                                yield return new WaitWhile(() => !endSelect);
                                endSelect = false;

                                GManager.instance.commandText.CloseCommandText();
                                yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                                if (fromHand)
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

                                        selectHandEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                                        selectHandEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                                        yield return StartCoroutine(selectHandEffect.Activate());

                                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                                        {
                                            selectedCards.Add(cardSource);

                                            yield return null;
                                        }

                                        if (selectedCards.Count >= 1)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(selectedCards, activateClass));
                                        }
                                    }
                                }

                                else
                                {
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

                                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place in digivolution cards.", "The opponent is selecting 1 Digimon to place in digivolution cards.");

                                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                        {
                                            Permanent selectedPermanent = permanent;

                                            yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(new List<Permanent[]>() { new Permanent[] { selectedPermanent, card.PermanentOfThisCard() } }, false, activateClass).PlacePermanentToDigivolutionCards());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            ActivateClass activateClass2 = new ActivateClass();
            activateClass2.SetUpICardEffect("Reduce play cost", CanUseCondition2, card);
            activateClass2.SetUpActivateClass(CanActivateCondition2, ActivateCoroutine2, 1, true, EffectDiscription2());
            activateClass2.SetHashString("Reduce_EX2_007");

            string EffectDiscription2()
            {
                return "[Your Turn][Once Per Turn] When you would play a card with [D-Reaper] in its traits from your hand, you may reduce its play cost by 1 for each of this Digimon's digivolution cards.";
            }

            bool CardCondition(CardSource cardSource)
            {
                if (cardSource.Owner == card.Owner)
                {
                    if (CardEffectCommons.IsExistOnHand(cardSource))
                    {
                        if (cardSource.CardTraits.Contains("D-Reaper"))
                        {
                            if(!cardSource.IsOption)
                                return true;
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition2(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanActivateCondition2(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.PermanentOfThisCard().DigivolutionCards.Count >= 1)
                    {
                        PlayCardClass playCard = CardEffectCommons.GetPlayCardClassFromHashtable(hashtable);

                        if (playCard != null)
                        {
                            if (playCard.PayCost)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine2(Hashtable _hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.PermanentOfThisCard().DigivolutionCards.Count >= 1)
                    {
                        int reduceCount = card.PermanentOfThisCard().DigivolutionCards.Count;

                        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect($"Play Cost -{reduceCount}", CanUseCondition1, card);
                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                        card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                        yield return new WaitForSeconds(0.2f);

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

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
                                        Cost -= reduceCount;
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
                            if (cardSource != null)
                            {
                                if (cardSource.Owner == card.Owner)
                                {
                                    return true;
                                }
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
                    }
                }
            }

            if (timing == EffectTiming.BeforePayCost)
            {
                cardEffects.Add(activateClass2);
            }

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => true, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (!card.Owner.isYou && GManager.instance.IsAI)
                    {
                        return false;
                    }

                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (card.PermanentOfThisCard().DigivolutionCards.Count >= 1)
                            {
                                if (activateClass2 != null)
                                {
                                    if (!card.cEntity_EffectController.isOverMaxCountPerTurn(activateClass2, activateClass2.MaxCountPerTurn))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource))
                    {
                        if (RootCondition(root))
                        {
                            if (PermanentsCondition(targetPermanents))
                            {
                                Cost -= card.PermanentOfThisCard().DigivolutionCards.Count;
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
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.Owner.HandCards.Contains(cardSource))
                            {
                                if (cardSource.CardTraits.Contains("D-Reaper"))
                                {
                                    return true;
                                }
                            }
                        }
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
}