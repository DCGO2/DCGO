using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX8
{
    public class EX8_037 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Alternate Digivolution Conditions

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Sakuyamon")
                           && targetPermanent.TopCard.HasLevel
                           && targetPermanent.TopCard.Level == 6
                           && !targetPermanent.TopCard.HasXAntibodyTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            
            #endregion
            
            #region When Digivolving
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play a Token", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] If [Sakuyamon] or [X Antibody] is in this Digimon's digivolution cards, play 1 [Uka-no-Mitama] (Digimon/Yellow/9000 DP/<Rush>) Token.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if(CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanActivateTokenEffectCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.EqualsCardName("Sakuyamon") || cardSource.EqualsCardName("X Antibody") || cardSource.EqualsCardName("XAntibody")) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CanActivateTokenEffectCondition(hashtable))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayUkaNoMitama(activateClass));
                    }
                }
            }
            
            #endregion
            
            #region Your Turn

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 single-color option card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true,
                    EffectDescription());
                activateClass.SetHashString("PlayOption_EX8_037");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] [Once Per Turn] When any of your Digimon attack, you may use 1 single-color Option card with a cost of 5 or less from your hand without paying the cost. If you did, 1 of your Digimon unsuspends.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanSelectOptionCard(CardSource cardSource)
                {
                    if (cardSource.IsOption)
                    {
                        if (cardSource.CardColors.Count == 1 && cardSource.GetCostItself <= 5)
                        {
                            if (!cardSource.CanNotPlayThisOption)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, PermanentCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanSelectYourSuspendedDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.IsDigimon && permanent.IsSuspended;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.HandCards.Count(CanSelectOptionCard) >= 1)
                    {
                        bool played = false;
                        
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOptionCard,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage(
                            "Select 1 option card to use.",
                            "The opponent is selecting 1 option card to use.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Used Card");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.PlayOptionCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            root: SelectCardEffect.Root.Hand));

                        if (selectedCards.Any())
                        {
                            played = true;
                        }
                        
                        if (played)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectYourSuspendedDigimon))
                            {
                                SelectPermanentEffect selectPermanentEffect =
                                    GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canTargetCondition: CanSelectYourSuspendedDigimon,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.UnTap,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to unsuspend.",
                                    "The opponent is selecting 1 Digimon to unsuspend.");

                                yield return ContinuousController.instance.StartCoroutine(
                                    selectPermanentEffect.Activate());
                            }
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}