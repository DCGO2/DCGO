using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

// Revivemon
namespace DCGO.CardEffects.BT24
{
    public class BT24_077 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition - Sup.
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasSuperAppTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Link Condition

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasAppmonTraits;
                }
                cardEffects.Add(CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: PermanentCondition, linkCost: 3, card: card));
            }

            #endregion

            #region App Fusion (Raidramon, Dezipmon)

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.AddAppfuseMethodByName(new List<string>() { "Raidramon", "Dezipmon" }, card));
            }

            #endregion

            #region Link
            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.LinkEffect(card));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared WD / OD

            string SharedEffectName() => "Link level 4 or lower from trash or this digimon";

            string SharedEffectDescription(string tag) => $"[{tag}] You may link 1 level 4 or lower Digimon card from your trash or this Digimon's digivolution cards to 1 of your Digimon without paying the cost.";

            bool CanSelectLinkCard(CardSource cardSource)
            {
                return cardSource.IsDigimon
                    && cardSource.HasLevel
                    && cardSource.Level <= 4
                    && cardSource.CanLink(false);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool hasInTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectLinkCard);
                bool hasInSources = card.PermanentOfThisCard() != null && card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectLinkCard) > 0;
                bool selectedRoot = false;
                CardSource selectedCard = null;
                if (hasInTrash || hasInSources)
                {
                    if (hasInTrash && hasInSources)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From This Digimon", value: true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From Trash", value: false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Choose where to get the digimon from";
                        string notSelectPlayerMessage = "The opponent is choosing effects.";

                        GManager.instance.userSelectionManager.SetBoolSelection(
                            selectionElements: selectionElements, selectPlayer: card.Owner,
                            selectPlayerMessage: selectPlayerMessage,
                            notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance
                            .userSelectionManager.WaitForEndSelect());

                        selectedRoot = GManager.instance.userSelectionManager.SelectedBoolValue;
                    }
                    else if (hasInSources)
                    {
                        selectedRoot = true;
                    }

                    if (selectedRoot)
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectLinkCard,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 card to link.",
                                    maxCount: 1,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to link", "The opponent is selecting 1 card to link");

                        yield return StartCoroutine(selectCardEffect.Activate());
                    }
                    else
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectLinkCard,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to link.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to link.", "The opponent is selecting 1 digivolution card to link.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Linked Card");

                            yield return StartCoroutine(selectCardEffect.Activate());
                    }
                }

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCard = cardSource;
                    yield return null;
                }

                if (selectedCard != null)
                {
                    bool CanSelectDigimon(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                            return selectedCard.CanLinkToTargetPermanent(permanent, false);

                        return false;
                    }

                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectDigimon))
                    {
                        Permanent selectedPermanent = null;
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectDigimon));
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to add link", "The opponent is selecting 1 digimon to add link");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddLinkCard(selectedCard, activateClass));
                        }
                    }
                }
            }

            #endregion

            #region Shared When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, true, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

            }

            #endregion

            #region Shared On Deletion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, true, SharedEffectDescription("On Deletion"));
                cardEffects.Add(activateClass);

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
            
            #region On Deletion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 lvl 4- [Appmon] from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() => "[On Deletion] You may play 1 level 4 or lower [Appmon] trait Digimon card from your trash without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card);
                }

                bool CanPlayCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel
                        && cardSource.Level <= 4
                        && cardSource.HasAppmonTraits
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int maxCount = Math.Min(1, card.Owner.TrashCards.Count(CanPlayCardCondition));

                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanPlayCardCondition,
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

            #endregion

            #region Link When Linking

            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 lowest DP Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsLinkedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Linking] Delete 1 of your opponent's Digimon with the lowest DP.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenLinking(hashtable, null, card);
                }

                bool DeletionTargetCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if(CardEffectCommons.HasMatchConditionOpponentsPermanent(card, DeletionTargetCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount = 1;

                        selectPermanentEffect.SetUp(

                            selectPlayer: card.Owner,
                            canTargetCondition: DeletionTargetCondition,
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

            #endregion

            return cardEffects;
        }
    }
}
