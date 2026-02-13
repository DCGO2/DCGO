using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class BT16_056 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place top card of one of your opponent's Digimon to the top/bottom of their security stack.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] You may place the top card of 1 of your opponent's Digimon with the [Vaccine] trait at the top of your opponent's security stack.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.CardTraits.Contains("Vaccine"))
                            return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition) >= 1)
                            return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
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
                                    Permanent securityPermanent = selectedPermanent;
                                    CardSource securityCard = securityPermanent.TopCard;

                                    if (securityPermanent.DigivolutionCards.Count >= 1)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(securityCard));

                                        permanent.ShowingPermanentCard.ShowPermanentData(true);

                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().RemoveDigivolveRootEffect(securityCard, permanent));

                                        if (!securityCard.IsToken)
                                        {
                                            Player owner = securityCard.Owner;
                                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(securityCard, true));

                                            permanent.willBeRemoveField = false;

                                            if (permanent.ShowingPermanentCard != null)
                                            {
                                                if (permanent.ShowingPermanentCard.WillBeDeletedObject != null)
                                                {
                                                    permanent.ShowingPermanentCard.WillBeDeletedObject.SetActive(false);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place top card of one of your opponent's Digimon to the top/bottom of their security stack.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may place the top card of 1 of your opponent's Digimon with the [Vaccine] trait at the top of your opponent's security stack.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.CardTraits.Contains("Vaccine"))
                            return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition) >= 1)
                            return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
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
                                    Permanent securityPermanent = selectedPermanent;
                                    CardSource securityCard = securityPermanent.TopCard;

                                    if (securityPermanent.DigivolutionCards.Count >= 1)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(securityCard));

                                        permanent.ShowingPermanentCard.ShowPermanentData(true);

                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().RemoveDigivolveRootEffect(securityCard, permanent));

                                        if (!securityCard.IsToken)
                                        {
                                            Player owner = securityCard.Owner;
                                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(securityCard, true));

                                            permanent.willBeRemoveField = false;

                                            if (permanent.ShowingPermanentCard != null)
                                            {
                                                if (permanent.ShowingPermanentCard.WillBeDeletedObject != null)
                                                {
                                                    permanent.ShowingPermanentCard.WillBeDeletedObject.SetActive(false);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnAddSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash top or bottom of opponents security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateAllTurnsCondition, ActivateCoroutine, 1, false, EffectAllTurnsDiscription());
                activateClass.SetHashString("Publimon_BT16_056_AllTurns");
                cardEffects.Add(activateClass);

                string EffectAllTurnsDiscription()
                {
                    return "[All Turns] [Once Per Turn] When a card is added to your opponent's security stack, if they have 3 or more security cards, trash the top or bottom card of their security stack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerWhenAddSecurity(hashtable, player => player == card.Owner.Enemy))
                    {
                        if (card.Owner.Enemy.SecurityCards.Count >= 3)
                            return true;
                    }

                    return false;
                }

                bool CanActivateAllTurnsCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.Enemy.SecurityCards.Count < 3)
                        yield break;

                    yield return GManager.instance.photonWaitController.StartWait("Publimon_Select_ETB");

                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                    {
                        new SelectionElement<bool>(message: $"Security Top", value : true, spriteIndex: 0),
                        new SelectionElement<bool>(message: $"Security Bottom", value : false, spriteIndex: 1),
                    };

                    string selectPlayerMessage = "Will you trash the card on the top or bottom of the security?";
                    string notSelectPlayerMessage = "The opponent is selecting whether to trash the card on the top or bottom of security.";

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool toTop = GManager.instance.userSelectionManager.SelectedBoolValue;

                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: toTop).DestroySecurity());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}