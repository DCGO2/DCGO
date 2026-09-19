using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Cross Arts
namespace DCGO.CardEffects.BT26
{
    public class BT26_101 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Use Req.
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource) => cardSource.HasTSTraits;
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If you have [Dan Yuki]/[Kanan Yuki], TS Digimon gain Blocker and +3000 DP, then delete or unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] If you have a Tamer with [Dan Yuki] or [Kanan Yuki] in its name, all of your [TS] trait Digimon gain Blocker and +3000 DP until your opponent's turn ends. Then, activate 1 of the effects below: - Delete 1 of your opponent's Digimon with as much DP as 1 of your [TS] trait Digimon or less. - 1 of your [TS] trait Digimon unsuspends.";

                bool HasDanOrKananYuki(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && (permanent.TopCard.ContainsCardName("Dan Yuki") || permanent.TopCard.ContainsCardName("Kanan Yuki"));

                bool OwnTSDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasTSTraits;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(HasDanOrKananYuki))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlockerPlayerEffect(
                            permanentCondition: OwnTSDigimonCondition,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDPPlayerEffect(
                            permanentCondition: OwnTSDigimonCondition,
                            changeValue: 3000,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));
                    }

                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                    {
                        new(message: "Delete 1 of your opponent's Digimon with as much DP as 1 of your [TS] trait Digimon or less.", value: 1, spriteIndex: 0),
                        new(message: "1 of your [TS] trait Digimon unsuspends.", value: 2, spriteIndex: 0),
                    };

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: "Select 1 effect to activate.", notSelectPlayerMessage: "The opponent is selecting 1 effect to activate.");
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    int choice = GManager.instance.userSelectionManager.SelectedIntValue;

                    if (CardEffectCommons.HasMatchConditionPermanent(OwnTSDigimonCondition))
                    {
                        if (choice == 1)
                        {
                            int? compareDP = null;

                            bool CanSelectDeleteTargetCondition(Permanent permanent)
                                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                                    && compareDP.HasValue
                                    && permanent.DP <= compareDP.Value;

                            SelectPermanentEffect selectCompareEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectCompareEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: OwnTSDigimonCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectCompareEffect.SetUpCustomMessage("Select 1 [TS] Digimon to compare DP.", "The opponent is selecting 1 [TS] Digimon to compare DP.");

                            yield return ContinuousController.instance.StartCoroutine(selectCompareEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent selectedPermanent)
                            {
                                compareDP = selectedPermanent.DP;
                                yield return null;
                            }

                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteTargetCondition))
                            {
                                SelectPermanentEffect selectDeleteEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectDeleteEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectDeleteTargetCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Destroy,
                                    cardEffect: activateClass);

                                selectDeleteEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                                yield return ContinuousController.instance.StartCoroutine(selectDeleteEffect.Activate());
                            }
                        }
                        else if (choice == 2)
                        {
                            SelectPermanentEffect selectUnsuspendEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectUnsuspendEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: OwnTSDigimonCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.UnTap,
                                cardEffect: activateClass);

                            selectUnsuspendEffect.SetUpCustomMessage("Select 1 [TS] trait Digimon to unsuspend.", "The opponent is selecting 1 [TS] trait Digimon to unsuspend.");

                            yield return ContinuousController.instance.StartCoroutine(selectUnsuspendEffect.Activate());
                        }
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 play cost 4 or lower [TS] card from hand or trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Security] You may play 1 play cost 4 or lower [TS] trait card from your hand or trash without paying the cost.";

                bool CanPlayCondition(CardSource cardSource)
                    => cardSource.HasPlayCost && cardSource.GetCostItself <= 4 && cardSource.HasTSTraits
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanPlayCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanPlayCondition);

                    if (canSelectHand || canSelectTrash)
                    {
                        SelectCardEffect.Root root;

                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<int>> selectionElements1 = new List<SelectionElement<int>>()
                            {
                                new(message: "From hand", value: 1, spriteIndex: 0),
                                new(message: "From trash", value: 2, spriteIndex: 0),
                                new(message: "Don't play", value: 3, spriteIndex: 1),
                            };

                            GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: "From which area will you play a card?", notSelectPlayerMessage: "The opponent is choosing from which area to select a card.");
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            if (GManager.instance.userSelectionManager.SelectedIntValue == 3) yield break;

                            root = GManager.instance.userSelectionManager.SelectedIntValue == 1 ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                        }
                        else
                        {
                            root = canSelectHand ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                            canTargetCondition: CanPlayCondition,
                            root: root,
                            cardEffect: activateClass,
                            payCost: false));
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
