using System.Collections;
using System.Collections.Generic;

// Bio Hybrid Digivolution
namespace DCGO.CardEffects.BT27
{
    public class BT27_102 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May play 1 [Kurata's Army] Tamer from hand or trash, then place this card in the battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] You may play 1 [Kurata's Army] trait Tamer card from your hand or trash without paying the cost. Then, place this card in the battle area.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanPlayFromHandCondition(CardSource cardSource)
                    => cardSource.IsTamer
                        && cardSource.EqualsTraits("Kurata's Army")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.Hand);

                bool CanPlayFromTrashCondition(CardSource cardSource)
                    => cardSource.IsTamer
                        && cardSource.EqualsTraits("Kurata's Army")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.Trash);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanPlayFromHandCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanPlayFromTrashCondition);

                    if (canSelectHand || canSelectTrash)
                    {
                        bool doPlay = true;
                        bool isHand = canSelectHand;

                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                            {
                                new(message: "From hand", value: 1, spriteIndex: 0),
                                new(message: "From trash", value: 2, spriteIndex: 0),
                                new(message: "Don't play", value: 3, spriteIndex: 1),
                            };

                            GManager.instance.userSelectionManager.SetIntSelection(
                                selectionElements: selectionElements,
                                selectPlayer: card.Owner,
                                selectPlayerMessage: "From which area will you play a card?",
                                notSelectPlayerMessage: "The opponent is choosing from which area to select a card.");

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            doPlay = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                            isHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                        }

                        if (doPlay)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                                canTargetCondition: isHand ? CanPlayFromHandCondition : CanPlayFromTrashCondition,
                                root: isHand ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash,
                                cardEffect: activateClass,
                                payCost: false));
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }
            #endregion

            #region All Turns - Delay
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<Delay> 1 of your Tamers may digivolve into a [Kurata's Army] Digimon from hand or trash for free", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] When any Digimon on the field are deleted, <Delay>. 1 of your Tamers may digivolve into a [Kurata's Army] trait Digimon card in the hand or trash without paying the cost.";

                bool AnyDigimonDeletedCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, AnyDigimonDeletedCondition, activateClass)
                        && CardEffectCommons.CanDeclareOptionDelayEffect(card);

                bool DigivolveCardCondition(CardSource cardSource)
                    => cardSource.IsDigimon
                        && cardSource.EqualsTraits("Kurata's Army");

                bool CanSelectTamerCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool delayPaid = false;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass,
                        successProcess: permanents => DelayPaid(),
                        failureProcess: null));

                    IEnumerator DelayPaid()
                    {
                        delayPaid = true;
                        yield return null;
                    }

                    if (!delayPaid || !CardEffectCommons.HasMatchConditionPermanent(CanSelectTamerCondition)) yield break;

                    Permanent selectedTamer = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectTamerCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer to digivolve.", "The opponent is selecting 1 Tamer to digivolve.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedTamer = permanent;
                        yield return null;
                    }

                    if (selectedTamer == null) yield break;

                    bool CanDigivolveFromHandCondition(CardSource cardSource)
                        => DigivolveCardCondition(cardSource)
                            && cardSource.CanPlayCardTargetFrame(selectedTamer.PermanentFrame, false, activateClass, SelectCardEffect.Root.Hand);

                    bool CanDigivolveFromTrashCondition(CardSource cardSource)
                        => DigivolveCardCondition(cardSource)
                            && cardSource.CanPlayCardTargetFrame(selectedTamer.PermanentFrame, false, activateClass, SelectCardEffect.Root.Trash);

                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanDigivolveFromHandCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanDigivolveFromTrashCondition);

                    if (!canSelectHand && !canSelectTrash) yield break;

                    bool isHand = canSelectHand;

                    if (canSelectHand && canSelectTrash)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new(message: "From hand", value: 1, spriteIndex: 0),
                            new(message: "From trash", value: 2, spriteIndex: 0),
                            new(message: "Don't digivolve", value: 3, spriteIndex: 1),
                        };

                        GManager.instance.userSelectionManager.SetIntSelection(
                            selectionElements: selectionElements,
                            selectPlayer: card.Owner,
                            selectPlayerMessage: "From which area will you digivolve?",
                            notSelectPlayerMessage: "The opponent is choosing from which area to select a card.");

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedIntValue == 3) yield break;

                        isHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: selectedTamer,
                        cardCondition: DigivolveCardCondition,
                        payCost: false,
                        reduceCostTuple: null,
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: isHand,
                        activateClass: activateClass,
                        successProcess: null));
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May play 1 play cost 5 or lower [Kurata's Army] card from hand or trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Security] You may play 1 play cost 5 or lower [Kurata's Army] trait card from your hand or trash without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);

                bool CanPlayFromHandCondition(CardSource cardSource)
                    => cardSource.EqualsTraits("Kurata's Army")
                        && cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 5
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.Hand);

                bool CanPlayFromTrashCondition(CardSource cardSource)
                    => cardSource.EqualsTraits("Kurata's Army")
                        && cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 5
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.Trash);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanPlayFromHandCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanPlayFromTrashCondition);

                    if (!canSelectHand && !canSelectTrash) yield break;

                    bool isHand = canSelectHand;

                    if (canSelectHand && canSelectTrash)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new(message: "From hand", value: 1, spriteIndex: 0),
                            new(message: "From trash", value: 2, spriteIndex: 0),
                            new(message: "Don't play", value: 3, spriteIndex: 1),
                        };

                        GManager.instance.userSelectionManager.SetIntSelection(
                            selectionElements: selectionElements,
                            selectPlayer: card.Owner,
                            selectPlayerMessage: "From which area will you play a card?",
                            notSelectPlayerMessage: "The opponent is choosing from which area to select a card.");

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedIntValue == 3) yield break;

                        isHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                        canTargetCondition: isHand ? CanPlayFromHandCondition : CanPlayFromTrashCondition,
                        root: isHand ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash,
                        cardEffect: activateClass,
                        payCost: false));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
