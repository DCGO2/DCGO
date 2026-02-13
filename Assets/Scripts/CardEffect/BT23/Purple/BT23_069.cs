using System;
using System.Collections;
using System.Collections.Generic;

// Necromon
namespace DCGO.CardEffects.BT23
{
    public class BT23_069 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Execute

            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.ExecuteSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region OP/OD Shared

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool HasCorrectTrait(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.EqualsTraits("Ghost") &&
                           cardSource.HasLevel && cardSource.Level <= 5 &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, HasCorrectTrait))
                {
                    CardSource selectedCard = null;

                    #region Select Digimon

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, HasCorrectTrait));

                    selectCardEffect.SetUp(
                        canTargetCondition: HasCorrectTrait,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 Digimon card to play.",
                        maxCount: maxCount,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    selectCardEffect.SetUpCustomMessage("Select 1 Digimon card to play.", "The opponent is selecting 1 Digimon card to play.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");
                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    #endregion

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: new List<CardSource>() { selectedCard },
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Trash,
                        activateETB: true)
                    );
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("You may play 1 level 5 or lower Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDescriptionShared());
                cardEffects.Add(activateClass);

                string EffectDescriptionShared()
                {
                    return "[On Play] You may play 1 level 5 or lower Digimon card with the [Ghost] trait from your trash without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            #region On Deletion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("You may play 1 level 5 or lower Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDescriptionShared());
                cardEffects.Add(activateClass);

                string EffectDescriptionShared()
                {
                    return "[On Deletion] You may play 1 level 5 or lower Digimon card with the [Ghost] trait from your trash without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card);
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnAllyAttack)
            {
                Permanent attackingPermament = null;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By deleting this digimon, delete 1 level 6 or lower digimon. if you didnt delete, you may end the attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When another Digimon attacks, by deleting this Digimon, delete 1 of your opponent's level 6 or lower Digimon. If this effect didn't delete your opponent's Digimon, you may end that attack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, AttackingPermament);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool AttackingPermament(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleArea(permanent) &&
                           permanent != card.PermanentOfThisCard();
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasLevel && permanent.TopCard.Level <= 6;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent thisPermament = card.PermanentOfThisCard();

                    #region Setup User Selection

                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                        };

                    string selectPlayerMessage = "Will you delete Necromon?";
                    string notSelectPlayerMessage = "The opponent is choosing to delete Necromon.";

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    #endregion

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                    bool isDeleting = GManager.instance.userSelectionManager.SelectedBoolValue;

                    if (isDeleting) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { thisPermament },
                        activateClass: activateClass,
                        successProcess: DeleteNecromonSuccessProcess,
                        failureProcess: null));

                    IEnumerator DeleteNecromonSuccessProcess(List<Permanent> permanents)
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

                            selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to delete", "Your opponent is selecting 1 digimon to delete");

                            #endregion

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            if (selectPermanentEffect != null) 
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                new List<Permanent>() { selectedPermament },
                                activateClass: activateClass,
                                successProcess: null,
                                failureProcess: OpponentLevel6FailureProcess));
                        }
                        else
                        {
                            yield return ContinuousController.instance.StartCoroutine(OpponentLevel6FailureProcess());
                        }                        
                    }

                    IEnumerator OpponentLevel6FailureProcess()
                    {
                        #region Setup User Selection

                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                                    {
                                        new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                                        new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                                    };

                        string selectPlayerMessage = "Will you end the attack?";
                        string notSelectPlayerMessage = "The opponent is choosing to end the attack.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        #endregion

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                        bool endAttack = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (endAttack)
                        {
                            GManager.instance.attackProcess.IsEndAttack = true;
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}