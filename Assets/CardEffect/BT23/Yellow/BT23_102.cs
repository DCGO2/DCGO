using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Mastemon
namespace DCGO.CardEffects.BT23
{
    public class BT23_102 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Alt Digivolve Cost

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel5
                        && targetPermanent.TopCard.HasCSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 5,
                    ignoreDigivolutionRequirement: false,
                    card: card, condition: null)
                );
            }

            #endregion

            #region DNA Digivolution

            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect($"DNA Digivolution", CanUseCondition, card);
                addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
                addJogressConditionClass.SetNotShowUI(true);
                cardEffects.Add(addJogressConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                JogressCondition GetJogress(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        bool PermanentCondition1(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.IsDigimon)
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                                            {
                                                if (permanent.TopCard.CardColors.Contains(CardColor.Yellow))
                                                {
                                                    if (permanent.Levels_ForJogress(card).Contains(5))
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

                        bool PermanentCondition2(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.IsDigimon)
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                                            {
                                                if (permanent.TopCard.CardColors.Contains(CardColor.Purple))
                                                {
                                                    if (permanent.Levels_ForJogress(card).Contains(5))
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

                        JogressConditionElement[] elements =
                        {
                            new(PermanentCondition1, "a level Yellow Digimon"),
                            new(PermanentCondition2, "a level Purple Digimon")
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }

            #endregion

            #region Barrier

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region Partition

            List<PartitionCondition> partitionConditions = new List<PartitionCondition>
            {
                new PartitionCondition("Angewomon"),
                new PartitionCondition("LadyDevimon")
            };

            if (timing == EffectTiming.WhenRemoveField)
            {
                cardEffects.Add(CardEffectFactory.PartitionSelfEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null,
                    cardSourceConditions: partitionConditions));
            }

            #endregion

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 level 5 or lower yellow/purple card from hand or trash. then if digimon has 2+ same level cards in stack, trash both security till 3", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may play 1 level 5 or lower yellow or purple card from your hand or trash without paying the cost. Then, if this Digimon's stack has 2 or more same-level cards, trash the top cards of both players' security stacks so that they have 3 cards left.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanSelectCard(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel && cardSource.Level <= 5
                        && (cardSource.CardColors.Contains(CardColor.Yellow) || cardSource.CardColors.Contains(CardColor.Purple))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCard);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCard);

                    if (canSelectHand || canSelectTrash)
                    {
                        #region Setup user Selection

                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                            {
                                new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                                new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                            };

                            string selectPlayerMessage = "From which area do you select a card?";
                            string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectHand);
                        }

                        #endregion

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                        bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                        CardSource selectedCard = null;
                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        if (fromHand)
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCard,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectHandEffect.SetUpCustomMessage("Select 1 card to play", "The opponent is selecting 1 card to play.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Selected card");
                            yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                            if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                new List<CardSource>() { selectedCard },
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.Hand,
                                activateETB: transform)
                            );
                        }
                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCard,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to play.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 card to play", "The opponent is selecting 1 card to play.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");
                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                new List<CardSource>() { selectedCard },
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.Trash,
                                activateETB: transform)
                            );
                        }
                    }

                    if (card.PermanentOfThisCard().StackCards.Filter(x => !x.IsFlipped).GroupBy(x => x.Level).Any(g => g.Count() >= 2))
                    {
                        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                        {
                            if (player.SecurityCards.Count <= 3) continue;

                            var trashAmount = player.SecurityCards.Count - 3;

                            yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                player: player,
                                destroySecurityCount: trashAmount,
                                cardEffect: activateClass,
                                fromTop: true).DestroySecurity()
                            );
                        }
                    }
                }
            }

            #endregion

            #region All Turns -  OPT

            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 digimon as bottom security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT23_102_AT");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When security stacks are removed from, you may place 1 Digimon as the bottom security card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, null);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleArea(permanent)
                        && permanent.IsDigimon;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        Permanent selectedPermament = null;

                        #region Select Opponent Permament

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

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

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermament = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to send to bottom of security.", "Your opponent is selecting 1 digimon to send to bottom of security.");

                        #endregion

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedPermament != null)
                        {
                            if (!selectedPermament.TopCard.CanNotBeAffected(activateClass))
                            {
                                #region hashtable
                                Hashtable _hashtable = new Hashtable()
                                {
                                    {"CardEffect", activateClass}
                                };
                                #endregion

                                yield return ContinuousController.instance.StartCoroutine(new IPutSecurityPermanent(
                                    permanent: selectedPermament,
                                    hashtable: _hashtable,
                                    toTop: false).PutSecurity()
                                );
                            }
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}