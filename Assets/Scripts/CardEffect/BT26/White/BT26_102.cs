using System.Collections;
using System.Collections.Generic;

// Seven Code PAD
namespace DCGO.CardEffects.BT26
{
    public class BT26_102 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Use Req.
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource) => cardSource.EqualsTraits("Seven Code");
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 6 [Seven Code] cards as digivolution material, that Digimon may digivolve into [Dantemon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] By placing 6 [Seven Code] trait Digimon cards from your battle area, link cards or trash as 1 of your [Seven Code] trait Digimon's bottom digivolution cards, that Digimon may digivolve into [Dantemon] in the hand, ignoring digivolution requirements and without paying the cost.";

                bool CanSelectTargetCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsTraits("Seven Code");

                bool CanSelectBattleAreaSourceCondition(Permanent permanent)
                    => permanent != null && CanSelectTargetCondition(permanent);

                bool CanSelectDantemonCondition(CardSource cardSource) => cardSource.EqualsCardName("Dantemon");

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectTargetCondition);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedTarget = null;

                    SelectPermanentEffect selectTargetEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectTargetEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectTargetCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectTargetCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    IEnumerator SelectTargetCoroutine(Permanent permanent)
                    {
                        selectedTarget = permanent;
                        yield return null;
                    }

                    selectTargetEffect.SetUpCustomMessage("Select 1 [Seven Code] trait Digimon to gain digivolution material.", "The opponent is selecting 1 [Seven Code] trait Digimon.");

                    yield return ContinuousController.instance.StartCoroutine(selectTargetEffect.Activate());

                    if (selectedTarget == null) yield break;

                    List<CardSource> materialCards = new List<CardSource>();
                    int remaining = 6;

                    bool CanSelectOtherBattleAreaCondition(Permanent permanent)
                        => permanent != selectedTarget && CanSelectBattleAreaSourceCondition(permanent);

                    while (remaining > 0 && CardEffectCommons.HasMatchConditionPermanent(CanSelectOtherBattleAreaCondition))
                    {
                        Permanent selectedSource = null;

                        SelectPermanentEffect selectSourceEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectSourceEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOtherBattleAreaCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectSourceCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectSourceCoroutine(Permanent permanent)
                        {
                            selectedSource = permanent;
                            yield return null;
                        }

                        selectSourceEffect.SetUpCustomMessage($"Select 1 [Seven Code] trait Digimon from your battle area to place as material ({remaining} remaining). You may stop selecting.", "The opponent is selecting material.");

                        yield return ContinuousController.instance.StartCoroutine(selectSourceEffect.Activate());

                        if (selectedSource == null) break;

                        materialCards.Add(selectedSource.TopCard);
                        remaining--;

                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(new List<Permanent>() { selectedSource }, CardEffectCommons.CardEffectHashtable(activateClass), notShowCards: true).Destroy());
                    }

                    bool CanSelectLinkedOrTrashCondition(CardSource cardSource)
                        => cardSource.IsDigimon && cardSource.EqualsTraits("Seven Code");

                    while (remaining > 0)
                    {
                        List<CardSource> pool = new List<CardSource>();
                        pool.AddRange(selectedTarget.LinkedCards.Filter(CanSelectLinkedOrTrashCondition));
                        pool.AddRange(card.Owner.TrashCards.Filter(CanSelectLinkedOrTrashCondition));

                        if (pool.Count == 0) break;

                        CardSource selectedCard = null;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectLinkedOrTrashCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: $"Select 1 [Seven Code] trait Digimon card from your link cards or trash to place as material ({remaining} remaining). You may stop selecting.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: pool,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        if (selectedCard == null) break;

                        materialCards.Add(selectedCard);
                        remaining--;
                    }

                    if (materialCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(selectedTarget.AddDigivolutionCardsBottom(materialCards, activateClass));
                    }

                    if (materialCards.Count == 6)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            selectedTarget,
                            CanSelectDantemonCondition,
                            payCost: false,
                            reduceCostTuple: null,
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null,
                            ignoreRequirements: CardEffectCommons.IgnoreRequirement.All
                        ));
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 play cost 5 or lower [Appmon] card from hand or trash, then add this to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Security] You may play 1 play cost 5 or lower [Appmon] trait card from your hand or trash without paying the cost. Then, add this card to the hand.";

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.HasPlayCost && cardSource.GetCostItself <= 5 && cardSource.EqualsTraits("Appmon")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);

                bool CanUseCondition(Hashtable hashtable) => CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canPlayFromHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                    bool canPlayFromTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    if (canPlayFromHand || canPlayFromTrash)
                    {
                        SelectCardEffect.Root root = canPlayFromHand ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;

                        if (canPlayFromHand && canPlayFromTrash)
                        {
                            List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                            {
                                new(message: "Play from hand", value: 1, spriteIndex: 0),
                                new(message: "Play from trash", value: 2, spriteIndex: 1),
                                new(message: "Don't play a card", value: 3, spriteIndex: 2),
                            };

                            GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: "Will you play an [Appmon] card?", notSelectPlayerMessage: "The opponent is choosing whether to play an [Appmon] card.");
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            int selected = GManager.instance.userSelectionManager.SelectedIntValue;

                            if (selected != 3)
                            {
                                root = selected == 1 ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                                    canTargetCondition: CanSelectCardCondition,
                                    root: root,
                                    cardEffect: activateClass,
                                    payCost: false));
                            }
                        }
                        else
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                                canTargetCondition: CanSelectCardCondition,
                                root: root,
                                cardEffect: activateClass,
                                payCost: false));
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
