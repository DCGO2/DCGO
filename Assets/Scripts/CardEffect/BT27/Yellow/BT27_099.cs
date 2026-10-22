using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Surtr Brander
namespace DCGO.CardEffects.BT27
{
    public class BT27_099 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region When Trashed from Security
            if (timing == EffectTiming.OnDiscardSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Activate the security effect", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "When effects trash this card from the security stack, activate its [Security] effects.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnTrashSelfSecurity(hashtable, cardEffect => cardEffect != null, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    ActivateClass securityActivateClass = CardEffectCommons.OptionSecurityEffect(card);

                    if (securityActivateClass != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(securityActivateClass.Activate(CardEffectCommons.OptionMainCheckHashtable(card)));
                    }
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 opponent's Digimon gets -6K DP, add top security to hand, place this card as top security", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] 1 of your opponent's Digimon gets -6000 DP for the turn. Then, add your top security card to the hand and place this card as the top security card.";
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
                    #region -6000 DP
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to get -6000 DP.", "The opponent is selecting 1 Digimon to get -6000 DP.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(
                                    CardEffectCommons.ChangeDigimonDP(
                                        targetPermanent: permanent,
                                        changeValue: -6000,
                                        effectDuration: EffectDuration.UntilEachTurnEnd,
                                        activateClass: activateClass));
                            }
                        }
                    }
                    #endregion

                    #region Add top security to hand
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardObjectController.AddHandCards(new List<CardSource>() { card.Owner.SecurityCards[0] }, false, activateClass));

                        // Removing a card from the security stack triggers "security stack is removed from" effects.
                        yield return ContinuousController.instance.StartCoroutine(
                            new IReduceSecurity(player: card.Owner, refSkillInfos: ref ContinuousController.instance.nullSkillInfos, activateClass).ReduceSecurity());
                    }
                    #endregion

                    #region Place this card as top security
                    if (card.Owner.CanAddSecurity(activateClass))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(card, toTop: true));
                    }
                    #endregion
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 opponent's lowest DP Digimon as their top security card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Security] Place 1 of your opponent's Digimon with the lowest DP as their top security card.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.PutSecurityTop,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place as top security.", "The opponent is selecting 1 Digimon to place as top security.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
