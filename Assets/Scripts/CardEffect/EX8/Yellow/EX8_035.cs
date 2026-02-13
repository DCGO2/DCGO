using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX8
{
    public class EX8_035 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Alternate Digivolution Conditions
            
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 5)
                        return targetPermanent.TopCard.EqualsTraits("DS");

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }
            
            #endregion
            
            #region Security Effect
            
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("2 Opponent's Digimon gain Security Attack -1 and add this card to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] At the end of the battle, give 2 of your opponent's Digimon <Security A. -1> for the turn. Then, add this card to the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnExecutingArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

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

                        selectPermanentEffect.SetUpCustomMessage("Select 2 Digimon that will get Security Attack -1.", "The opponent is selecting 2 Digimon that will get Security Attack -1.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(targetPermanent: permanent, changeValue: -1, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));

                            yield return null;
                        }
                    }

                    if (card.Owner.ExecutingCards.Contains(card))
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.brainStormObject.CloseBrainstrorm(card));

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { card }, false, activateClass));
                    }
                }
            }
            
            #endregion
            
            #region All Turns

            if (timing == EffectTiming.None)
            {
                DisableEffectClass invalidationClass = new DisableEffectClass();
                invalidationClass.SetUpICardEffect("Ignore [When Digivolving] effects of opponent's Digimon while memory is 1 or more", CanUseCondition, card);
                invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                cardEffects.Add(invalidationClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                bool InvalidateCondition(ICardEffect cardEffect)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.MemoryForPlayer >= 1)
                        {
                            if (cardEffect != null)
                            {
                                if (cardEffect is ActivateICardEffect)
                                {
                                    if (cardEffect.EffectSourceCard != null)
                                    {
                                        if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(cardEffect.EffectSourceCard.PermanentOfThisCard(), card))
                                        {
                                            if (!cardEffect.EffectSourceCard.PermanentOfThisCard().TopCard.CanNotBeAffected(invalidationClass))
                                            {
                                                if (cardEffect.IsWhenDigivolving)
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }
            }

            #endregion

            return cardEffects;
        }
    }
}