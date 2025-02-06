using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.EX2
{
    public class EX2_044 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnDiscardLibrary)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Impmon] from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When this card is trashed from your deck, you may play 1 [Impmon] from your trash without paying its memory cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                    {
                        if (cardSource.CardNames.Contains("Impmon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenSelfDiscardLibrary(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnTrash(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition(cardSource)))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition(cardSource)))
                    {
                        int maxCount = Math.Min(1, card.Owner.TrashCards.Count((cardSource) => CanSelectCardCondition(cardSource)));

                        List<CardSource> selectedCards = new List<CardSource>();

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

                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Trash, activateETB: true));
                    }
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 2 cards from deck top and delete 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may trash the top 2 cards of your deck. Then, delete 1 of your opponent's level 3 or lower Digimon. For every 10 cards in your trash, add 1 to the maximum level of the Digimon you can choose with this effect.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (permanent != null)
                    {
                        if (permanent.TopCard != null)
                        {
                            if (permanent.IsDigimon)
                            {
                                if (permanent.TopCard.Owner == card.Owner.Enemy)
                                {
                                    if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                    {
                                        int maxLevel = 3 + card.Owner.TrashCards.Count / 10;

                                        if (permanent.Level <= maxLevel)
                                        {
                                            if (permanent.TopCard.HasLevel)
                                            {
                                                return true;
                                            }
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
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            if (card.Owner.LibraryCards.Count >= 1)
                            {
                                if (card.Owner.isYou)
                                {
                                    GManager.instance.commandText.OpenCommandText("Will you trash the top cards of your deck?");

                                    List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>()
                                {
                                    new Command_SelectCommand($"Trash", () => photonView.RPC("SetDoTrash", RpcTarget.All, true), 0),
                                    new Command_SelectCommand($"Not trash", () => photonView.RPC("SetDoTrash", RpcTarget.All, false), 1),
                                };

                                    GManager.instance.selectCommandPanel.SetUpCommandButton(command_SelectCommands);
                                }

                                else
                                {
                                    GManager.instance.commandText.OpenCommandText("The opponent is choosing wheter to trash the top cards of deck.");

                                    #region AIモード
                                    if (GManager.instance.IsAI)
                                    {
                                        SetDoTrash(RandomUtility.IsSucceedProbability(0.5f));
                                    }
                                    #endregion
                                }

                                yield return new WaitWhile(() => !endSelect);
                                endSelect = false;

                                GManager.instance.commandText.CloseCommandText();
                                yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                                if (doTrash)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(2, card.Owner, activateClass).AddTrashCardsFromLibraryTop());
                                }
                            }

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
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Destroy,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 2 cards from deck top and delete 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] You may trash the top 2 cards of your deck. Then, delete 1 of your opponent's level 3 or lower Digimon. For every 10 cards in your trash, add 1 to the maximum level of the Digimon you can choose with this effect.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (permanent != null)
                    {
                        if (permanent.TopCard != null)
                        {
                            if (permanent.IsDigimon)
                            {
                                if (permanent.TopCard.Owner == card.Owner.Enemy)
                                {
                                    if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                    {
                                        int maxLevel = 3 + card.Owner.TrashCards.Count / 10;

                                        if (permanent.Level <= maxLevel)
                                        {
                                            if (permanent.TopCard.HasLevel)
                                            {
                                                return true;
                                            }
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
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            if (card.Owner.LibraryCards.Count >= 1)
                            {
                                if (card.Owner.isYou)
                                {
                                    GManager.instance.commandText.OpenCommandText("Will you trash the top cards of your deck?");

                                    List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>()
                                {
                                    new Command_SelectCommand($"Trash", () => photonView.RPC("SetDoTrash", RpcTarget.All, true), 0),
                                    new Command_SelectCommand($"Not trash", () => photonView.RPC("SetDoTrash", RpcTarget.All, false), 1),
                                };

                                    GManager.instance.selectCommandPanel.SetUpCommandButton(command_SelectCommands);
                                }

                                else
                                {
                                    GManager.instance.commandText.OpenCommandText("The opponent is choosing wheter to trash the top cards of deck.");

                                    #region AIモード
                                    if (GManager.instance.IsAI)
                                    {
                                        SetDoTrash(RandomUtility.IsSucceedProbability(0.5f));
                                    }
                                    #endregion
                                }

                                yield return new WaitWhile(() => !endSelect);
                                endSelect = false;

                                GManager.instance.commandText.CloseCommandText();
                                yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                                if (doTrash)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(2, card.Owner, activateClass).AddTrashCardsFromLibraryTop());
                                }
                            }

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
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Destroy,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }

        bool endSelect = false;
        bool doTrash = false;

        [PunRPC]
        public void SetDoTrash(bool doTrash)
        {
            this.doTrash = doTrash;
            endSelect = true;
        }
    }
}