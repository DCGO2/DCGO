using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.EX3
{
    public class EX3_066 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);

                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent.TopCard.CardTraits.Contains("Machine") && permanent.TopCard.Level == 6 && permanent.TopCard.HasLevel))
                    {
                        return true;
                    }

                    return false;
                }



                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        return true;
                    }

                    return false;
                }
            }
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] <De-Digivolve 3> 1 of your opponent's Digimon. Then, by placing 1 card with [Cyborg] in its traits from your hand or trash under 1 of your level 6 Digimon with [Machine] in its traits as its bottom digivolution card, delete 1 of your opponent's Digimon with 6000 DP or less.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.CardTraits.Contains("Cyborg"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.CardTraits.Contains("Machine") && permanent.TopCard.Level == 6 && permanent.TopCard.HasLevel)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition2(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(6000, activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
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

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve.", "The opponent is selecting 1 Digimon to De-Digivolve.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            yield return ContinuousController.instance.StartCoroutine(new IDegeneration(selectedPermanent, 3, activateClass).Degeneration());
                        }
                    }

                    bool addedDigivolutionCard = false;

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                    {
                        if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1 || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1 && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                            {
                                if (card.Owner.isYou)
                                {
                                    GManager.instance.commandText.OpenCommandText("From which area do you select a card?");

                                    List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>()
                                {
                                    new Command_SelectCommand($"From hand", () => photonView.RPC("SetFromHand", RpcTarget.All, true), 0),
                                    new Command_SelectCommand($"From trash", () => photonView.RPC("SetFromHand", RpcTarget.All, false), 1),
                                };

                                    GManager.instance.selectCommandPanel.SetUpCommandButton(command_SelectCommands);
                                }

                                else
                                {
                                    GManager.instance.commandText.OpenCommandText("The opponent is choosing from which area to select a card.");

                                    #region AIƒ‚[ƒh
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

                                selectHandEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

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
                                    message: "Select 1 card to place on bottom of digivolution cards.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                                selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                            }

                            if (selectedCards.Count >= 1)
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                                {
                                    int maxCount = 1;

                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectPermanentCondition1,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: true,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get a digivolution card.", "The opponent is selecting 1 Digimon that will get a digivolution card.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        Permanent selectedPermanent = permanent;

                                        if (selectedPermanent != null)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(selectedCards, activateClass));

                                            addedDigivolutionCard = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (addedDigivolutionCard)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition2))
                        {
                            int maxCount = 1;

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition2,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects, effectName: $"De-Digivolve 3 and place a card to digivolution cards to delete 1 Digimon with 6000 DP or less");
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