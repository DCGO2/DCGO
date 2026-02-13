using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT14
{
    public class BT14_094 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] Activate 1 of the effects below: - 1 of your opponent's Digimon gets -6000 DP for the turn. - By deleting 1 of your [Angemon], place 1 of your opponent's Digimon at the bottom of their security stack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return GManager.instance.photonWaitController.StartWait("Heavens_Select_BT14");

                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new SelectionElement<int>(message: $"DP-6000", value : 0, spriteIndex: 0),
                            new SelectionElement<int>(message: $"Delete 1 [Angemon]", value : 1, spriteIndex: 0),
                        };

                    string selectPlayerMessage = "Which effect will you activate?";
                    string notSelectPlayerMessage = "The opponent is choosing which effect to activate.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    int actionID = GManager.instance.userSelectionManager.SelectedIntValue;

                    switch (actionID)
                    {
                        case 0:
                            {
                                bool CanSelectPermanentCondition(Permanent permanent)
                                {
                                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
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
                                        selectPermanentCoroutine: SelectPermanentCoroutine,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage(customMessageArray: CardEffectCommons.customPermanentMessageArray_ChangeDP(changeValue: -6000, maxCount: maxCount));

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                            targetPermanent: permanent,
                                            changeValue: -6000,
                                            effectDuration: EffectDuration.UntilEachTurnEnd,
                                            activateClass: activateClass));
                                    }
                                }
                            }
                            break;

                        case 1:
                            {
                                bool CanSelectPermanentCondition(Permanent permanent)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                                    {
                                        if (permanent.TopCard.CardNames.Contains("Angemon"))
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                bool CanSelectPermanentCondition1(Permanent permanent)
                                {
                                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
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
                                        canNoSelect: true,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                            targetPermanents: new List<Permanent>() { permanent },
                                            activateClass: activateClass,
                                            successProcess: permanents => SuccessProcess(),
                                            failureProcess: null));

                                        IEnumerator SuccessProcess()
                                        {
                                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                                            {
                                                maxCount = 1;

                                                selectPermanentEffect.SetUp(
                                                    selectPlayer: card.Owner,
                                                    canTargetCondition: CanSelectPermanentCondition1,
                                                    canTargetCondition_ByPreSelecetedList: null,
                                                    canEndSelectCondition: null,
                                                    maxCount: maxCount,
                                                    canNoSelect: false,
                                                    canEndNotMax: false,
                                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                                    afterSelectPermanentCoroutine: null,
                                                    mode: SelectPermanentEffect.Mode.Custom,
                                                    cardEffect: activateClass);

                                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place to security.", "The opponent is selecting 1 Digimon to place to security.");

                                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                                {
                                                    Permanent selectedPermanent = permanent;

                                                    if (selectedPermanent != null)
                                                    {
                                                        yield return ContinuousController.instance.StartCoroutine(new IPutSecurityPermanent(selectedPermanent, CardEffectCommons.CardEffectHashtable(activateClass), toTop: false).PutSecurity());
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects, effectName: $"Select effects");
            }

            return cardEffects;
        }
    }
}