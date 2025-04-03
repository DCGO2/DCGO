using System;
using System.Collections;
using System.Collections.Generic;

// Justimon: Critical Arm
namespace DCGO.CardEffects.P
{
    public class P_179 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.EqualsCardName("Justimon: Blitz Arm") || targetPermanent.TopCard.EqualsCardName("Justimon: Accel Arm"))
                    {
                        return true;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Reduce Digivolution Cost

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                    => (targetPermanent.TopCard.EqualsCardName("Justimon: Blitz Arm") || targetPermanent.TopCard.EqualsCardName("Justimon: Accel Arm"));
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 1, false, card, null));
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play [Device] option from hand or trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[When Digivolving] By placing 1 Option card with the [Device] trait from your hand or trash into the battle area, this Digimon gets +3000 DP until your opponent's turn ends.";

                bool CanSelectOption(CardSource cardSource)
                    => cardSource.IsOption && cardSource.HasDeviceTraits;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                       CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                    && (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectOption) || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectOption));

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedDevice = null;
                    bool optionInHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectOption);
                    bool optionInTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectOption);
                    SelectCardEffect.Root root = SelectCardEffect.Root.Hand;

                    if (optionInHand && optionInTrash)
                    {
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

                        root = GManager.instance.userSelectionManager.SelectedBoolValue ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                    }
                    else
                    {
                        root = optionInHand ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                    }

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

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedDevice = cardSource;
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    if (selectedDevice != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: new List<CardSource> { selectedDevice },
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Trash,
                            activateETB: true));

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

            bool CanSelectPermanentOptionCondition(Permanent permanent)
                => permanent.TopCard.IsOption && permanent.TopCard.HasDeviceTraits;

            bool CanSelectPermanentDigimonCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent)
                && permanent.TopCard.HasPlayCost
                && permanent.TopCard.GetCostItself <= 9;

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

                string SharedEffectDiscription()
                => "[When Digivolving] By trashing 1 [Device] Option on the field, you can select 1 of your opponent's Digimon with a level of 9 or less to delete.";

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

                string SharedEffectDiscription()
                => "[When Attacking] By trashing 1 [Device] Option on the field, you can select 1 of your opponent's Digimon with a level of 9 or less to delete.";

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