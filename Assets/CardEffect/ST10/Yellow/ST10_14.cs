using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class ST10_14 : CEntity_Effect
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
                return "[Main] Place 1 of your opponent's Digimon face down at the top or bottom of your opponent's security stack. If you do, trash the top card of your opponent's security stack.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
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

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place to security.", "The opponent is selecting 1 Digimon to place to security.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        Permanent selectedPermanent = permanent;

                        if (selectedPermanent != null)
                        {
                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                                {
                                    new SelectionElement<bool>(message: $"Security Top", value : true, spriteIndex: 0),
                                    new SelectionElement<bool>(message: $"Security Bottom", value : false, spriteIndex: 1),
                                };

                                string selectPlayerMessage = "Will you place the card on the top or bottom of the security?";
                                string notSelectPlayerMessage = "The opponent is selecting whether to place the card on the top or bottom of security.";

                                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                                bool toTop = GManager.instance.userSelectionManager.SelectedBoolValue;

                                if (toTop)
                                {
                                    GManager.instance.commandText.OpenCommandText("\"Place to the top of security\" was selected.");

                                    PlayLog.OnAddLog?.Invoke($"\n{card.BaseENGCardNameFromEntity}({card.CardID}):Place the Digimon to the top of security\n");
                                }

                                else
                                {
                                    GManager.instance.commandText.OpenCommandText("\"Place to the bottom of security\" was selected.");

                                    PlayLog.OnAddLog?.Invoke($"\n{card.BaseENGCardNameFromEntity}({card.CardID}):Place the Digimon to the bottom of security\n");
                                }

                                yield return new WaitForSeconds(0.4f);
                                GManager.instance.commandText.CloseCommandText();
                                yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                                Permanent securityPermanent = selectedPermanent;
                                CardSource securityCard = securityPermanent.TopCard;

                                yield return ContinuousController.instance.StartCoroutine(new IPutSecurityPermanent(
                                    securityPermanent,
                                    CardEffectCommons.CardEffectHashtable(activateClass),
                                    toTop).PutSecurity());

                                if (securityPermanent.TopCard == null)
                                {
                                    if (securityCard.Owner.SecurityCards.Contains(securityCard) || securityCard.IsToken)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                    player: card.Owner.Enemy,
                    destroySecurityCount: 1,
                    cardEffect: activateClass,
                    fromTop: true).DestroySecurity());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect($"Place an opponent's Digimon to security", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] You may place 1 of your opponent's Digimon face down at the top or bottom of its owner's security stack.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
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
                        canNoSelect: true,
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
                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                                {
                                    new SelectionElement<bool>(message: $"Security Top", value : true, spriteIndex: 0),
                                    new SelectionElement<bool>(message: $"Security Bottom", value : false, spriteIndex: 1),
                                };

                                string selectPlayerMessage = "Will you place the card on the top or bottom of the security?";
                                string notSelectPlayerMessage = "The opponent is selecting whether to place the card on the top or bottom of security.";

                                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                                bool toTop = GManager.instance.userSelectionManager.SelectedBoolValue;

                                if (toTop)
                                {
                                    GManager.instance.commandText.OpenCommandText("\"Place to the top of security\" was selected.");

                                    PlayLog.OnAddLog?.Invoke($"\n{card.BaseENGCardNameFromEntity}({card.CardID}):Place the Digimon to the top of security\n");
                                }

                                else
                                {
                                    GManager.instance.commandText.OpenCommandText("\"Place to the bottom of security\" was selected.");

                                    PlayLog.OnAddLog?.Invoke($"\n{card.BaseENGCardNameFromEntity}({card.CardID}):Place the Digimon to the bottom of security\n");
                                }

                                yield return new WaitForSeconds(0.4f);
                                GManager.instance.commandText.CloseCommandText();
                                yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                                Permanent securityPermanent = selectedPermanent;
                                CardSource securityCard = securityPermanent.TopCard;

                                yield return ContinuousController.instance.StartCoroutine(new IPutSecurityPermanent(
                                    securityPermanent,
                                    CardEffectCommons.CardEffectHashtable(activateClass),
                                    toTop).PutSecurity());
                            }
                        }
                    }
                }
            }
        }

        return cardEffects;
    }
}
