using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

//Machinedramon
namespace DCGO.CardEffects.EX10
{
    public class EX10_035 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Hand - Main

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play for reduced cost of 5, delete at end of turn", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsDigimonEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Hand] [Main] If you don't have any Digimon other than Digimon with [Dark Masters] in their texts, you may play this card with the play cost reduced by 5. At turn end, delete the Digimon this effect played.";
                }

                bool IsDarkMaster(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(targetPermanent, card) &&
                           !targetPermanent.TopCard.HasText("Dark Masters");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnHand(card) &&
                           CardEffectCommons.MatchConditionPermanentCount(IsDarkMaster) == 0 &&
                           CardEffectCommons.CanPlayAsNewPermanent(card, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    #region reduce play cost

                    if (card.Owner.CanReduceCost(null, card))
                        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect($"Play Cost -5", CanUseCondition1, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                    card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(hashtable));

                    ICardEffect GetReduceCostCardEffect(EffectTiming _timing)
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
                                    Cost -= 5;
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
                        return true;
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

                    if (CardEffectCommons.CanPlayAsNewPermanent(card, true, activateClass))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                            cardSources: new List<CardSource> { card },
                                            activateClass: activateClass,
                                            payCost: true,
                                            isTapped: false,
                                            root: SelectCardEffect.Root.Hand,
                                            activateETB: true));

                        yield return new WaitForSeconds(0.2f);

                        #region Delete Digimon Played

                        Permanent playedPermanent = card.PermanentOfThisCard();

                        ActivateClass activateClass1 = new ActivateClass();
                        activateClass1.SetUpICardEffect("Delete the Digimon", CanUseCondition2, playedPermanent.TopCard);
                        activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                        activateClass1.SetEffectSourcePermanent(playedPermanent);
                        playedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                        if (!playedPermanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(playedPermanent));
                        }

                        string EffectDiscription1()
                        {
                            return "[End of Your Turn] Delete this Digimon.";
                        }

                        bool CanUseCondition2(Hashtable hashtable1)
                        {
                            if (CardEffectCommons.IsOwnerTurn(card))
                            {
                                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(playedPermanent, playedPermanent.TopCard))
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        bool CanActivateCondition1(Hashtable hashtable1)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnBattleArea(playedPermanent))
                            {
                                if (!playedPermanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnBattleArea(playedPermanent))
                            {
                                yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                                new List<Permanent>() { playedPermanent },
                                CardEffectCommons.CardEffectHashtable(activateClass1)).Destroy());
                            }
                        }

                        ICardEffect GetCardEffect(EffectTiming _timing)
                        {
                            if (_timing == EffectTiming.OnEndTurn)
                            {
                                return activateClass1;
                            }

                            return null;
                        }

                        #endregion
                    }

                    #region release reducing play cost

                    card.Owner.UntilCalculateFixedCostEffect.Remove(GetReduceCostCardEffect);

                    #endregion
                }
            }

            #endregion

            #region On Play/When Attacking Shared

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanActivateSharedCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable _hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    int degenrationMaxCount = 2;
                    int degenrationCount = 0;

                    SelectCountEffect selectCountEffect = GManager.instance.GetComponent<SelectCountEffect>();
                    if (selectCountEffect != null)
                    {
                        selectCountEffect.SetUp(
                            SelectPlayer: card.Owner,
                            targetPermanent: null,
                            MaxCount: degenrationMaxCount,
                            CanNoSelect: false,
                            Message: "How much will you De-Digivolve?",
                            Message_Enemy: "The opponent is choosing how much to De-Digivolve.",
                            SelectCountCoroutine: SelectCountCoroutine);

                        yield return ContinuousController.instance.StartCoroutine(selectCountEffect.Activate());

                        IEnumerator SelectCountCoroutine(int count)
                        {
                            degenrationCount = count;
                            yield return null;
                        }
                    }

                    List<Permanent> selectedPermaments = new List<Permanent>();
                    int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

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

                    selectPermanentEffect.SetUpCustomMessage("Select 2 Digimon to De-Digivolve.", "The opponent is selecting 2 Digimon to De-Digivolve.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermaments.Add(permanent);
                        yield return null;
                    }

                    if (selectedPermaments.Any())
                    {
                        foreach (var selectedPermanent in selectedPermaments)
                        {
                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IDegeneration(selectedPermanent, degenrationCount, activateClass, true).Degeneration());
                            }
                        }
                    }
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("De-digivolve 2, 2 digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, (hashtable) => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] <De-Digivolve 2> 2 of your opponent's Digimon. (Trash up to 2 cards from the top. You can't trash past level 3 cards.)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("De-digivolve 2, 2 digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, (hashtable) => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] <De-Digivolve 2> 2 of your opponent's Digimon. (Trash up to 2 cards from the top. You can't trash past level 3 cards.)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool CardCondition(CardSource cardSource)
                {
                    return !cardSource.EqualsCardName("Apocalymon");
                }

                cardEffects.Add(CardEffectFactory.CanNotDigivolveStaticSelfEffect(
                cardCondition: CardCondition,
                isInheritedEffect: false,
                card: card,
                condition: Condition,
                effectName: "[All Turns] This Digimon can only digivolve into [Apocalymon].")
                );
            }

            #endregion

            #region On Deletion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place this Digimon face up as bottom security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] If you have no black face-up security cards, place this Digimon face up as the bottom security card.";
                }

                bool FaceUpBlack(CardSource card)
                {
                    return !card.IsFlipped &&
                           card.CardColors.Contains(CardColor.Black);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card)
                        && card.Owner.CanAddSecurity(activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if(!CardEffectCommons.HasMatchConditionOwnersSecurity(card, FaceUpBlack, false))
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(card, false, true));
                }
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Play 1 Level 5 or lower digimon with [Dark Masters] in text from hand or trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] If this card was face-up, you may play 1 level 5 or lower card with [Dark Masters] in its text from your hand or trash without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card)
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition) || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        && !CardEffectCommons.GetFaceDownFromHashtable(hashtable);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel && cardSource.Level <= 5
                        && cardSource.HasText("Dark Masters");
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    if (canSelectHand || canSelectTrash)
                    {
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

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                        List<CardSource> selectedCards = new List<CardSource>();
                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        if (fromHand)
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardCondition,
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

                            selectHandEffect.SetUpCustomMessage("Select 1 digimon to play.", "The opponent is selecting 1 digimon to play.");
                            yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                        }
                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to add as source.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 card to add as source.", "The opponent is selecting 1 card to add as source.");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }

                        if (selectedCards.Any()) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: selectedCards,
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: fromHand ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash,
                                activateETB: true
                        ));
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}