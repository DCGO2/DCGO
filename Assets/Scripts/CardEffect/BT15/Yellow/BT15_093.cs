using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT15
{
    public class BT15_093 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] 1 of your opponent's Digimon gets -6000 DP for the turn. Then, by trashing the top or bottom card of your security stack, 1 of your opponent's Digimon gets -6000 DP for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
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

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -6000.", "The opponent is selecting 1 Digimon that will get DP -6000.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: -6000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                        }
                    }

                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                    {
                        new SelectionElement<int>(message: $"Security Top", value : 0, spriteIndex: 0),
                        new SelectionElement<int>(message: $"Security Bottom", value : 1, spriteIndex: 0),
                        new SelectionElement<int>(message: $"No Selection", value : 2, spriteIndex: 1),
                    };

                        string selectPlayerMessage = "Which will you trash the top or bottom card of the security?";
                        string notSelectPlayerMessage = "The opponent is selecting whether to trash the top or bottom card of security.";

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        int selectionValue = GManager.instance.userSelectionManager.SelectedIntValue;

                        if (selectionValue != 2)
                        {
                            bool fromTop = selectionValue.Equals(0);

                            yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                player: card.Owner,
                                destroySecurityCount: 1,
                                cardEffect: activateClass,
                                fromTop: fromTop).DestroySecurity());

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

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -6000.", "The opponent is selecting 1 Digimon that will get DP -6000.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: -6000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Security effect

            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(
                    card: card,
                    cardEffects: ref cardEffects,
                    effectName: $"Minus DP");
            }

            #endregion

            return cardEffects;
        }
    }
}