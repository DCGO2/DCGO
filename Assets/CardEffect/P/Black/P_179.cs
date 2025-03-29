using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects
{
    public class P_179 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Reduce Digivolution Cost

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Justimon: Blitz Arm") || targetPermanent.TopCard.EqualsCardName("Justimon: Accel Arm");
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.None)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play [Device] option from hand or trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "When Digivolving] By placing 1 Option card with the [Device] trait from your hand or trash into the battle area, this Digimon gets +3000 DP until your opponent's turn ends.";

                bool CanSelectOption(CardSource cardSource)
                    => cardSource.IsOption && cardSource.HasDeviceTraits;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                    && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedDevice = null;
                    SelectCardEffect SharedSelectCardEffect(SelectCardEffect.Root root)
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectOption,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select Device to play",
                            maxCount: 1,
                            canEndNotMax: true,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: root,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        return selectCardEffect;
                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: new List<CardSource> { cardSource },
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.Trash,
                                activateETB: true));
                            selectedDevice = cardSource;
                        }
                    }

                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                    {
                        new(message: "From hand", value: true, spriteIndex: 0),
                        new(message: "From trash", value: false, spriteIndex: 1)
                    };

                    string selectPlayerMessage = "Choose option location";
                    string notSelectPlayerMessage = "The opponent is choosing the option location.";

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner,
                        selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    SelectCardEffect.Root root = GManager.instance.userSelectionManager.SelectedBoolValue ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                    if (root == SelectCardEffect.Root.Hand && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectOption)) yield return ContinuousController.instance.StartCoroutine(SharedSelectCardEffect(root).Activate());
                    if (root == SelectCardEffect.Root.Trash && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectOption)) yield return ContinuousController.instance.StartCoroutine(SharedSelectCardEffect(root).Activate());
                    if (selectedDevice != null)
                    {
                        ChangeBaseDPClass changeDPClass = new ChangeBaseDPClass();
                        changeDPClass.SetUpICardEffect("+3000 DP", (hashtable) => true, card);
                        changeDPClass.SetUpChangeBaseDPClass(changeDPFunc: ChangeDP, permanentCondition: permanentCondition, isUpDownFunc: () => false, isMinusDPFunc: () => false);
                        selectedDevice.PermanentOfThisCard().UntilEachTurnEndEffects.Add((_timing) => changeDPClass);

                        int ChangeDP(Permanent permanent, int DP)
                            => DP + 3000;

                        bool permanentCondition(Permanent permanent)
                            => permanent != null && permanent.TopCard != null && permanent == selectedDevice.PermanentOfThisCard();
                    }
                }
            }

            #endregion

            #region When Digivolving/Attacking OPT Shared

            string SharedEffectDiscription()
                => "When Digivolving or attacking] By trashing 1 [Device] Option on the field, you can select 1 of your opponent's Digimon with a level of 9 or less to delete.";
            bool CanSelectPermanentOptionCondition(Permanent permanent)
                => permanent.TopCard.IsOption && permanent.TopCard.HasDeviceTraits;

            bool CanSelectPermanentDigimonCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent)
                && permanent.TopCard.HasLevel
                && permanent.TopCard.Level <= 9;

            IEnumerator SharedActivateCoroutine(Hashtable _hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanentOptionCondition))
                {
                    bool deviceTrashed = false;
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentOptionCondition));
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentOptionCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select [Device] option to trash", "The opponent is selecting 1 [Device] option to trash");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        deviceTrashed = true;
                        yield return null;
                    }

                    if (deviceTrashed && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentDigimonCondition))
                    {
                        int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentDigimonCondition));
                        SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect1.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentDigimonCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to delete", "The opponent is selecting 1 digimon to delete");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            #endregion

            #region When Digivolving OPT

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 [Device] Option on field", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, (hashtable) => SharedActivateCoroutine(hashtable, activateClass), 1, true, SharedEffectDiscription());
                activateClass.SetHashString("P_179_Trash&Delete");
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                    && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            #endregion

            #region When Attacking OPT

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 [Device] Option on field", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, (hashtable) => SharedActivateCoroutine(hashtable, activateClass), 1, true, SharedEffectDiscription());
                activateClass.SetHashString("P_179_Trash&Delete");
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                    && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
            }

            #endregion

            return cardEffects;
        }
    }
}