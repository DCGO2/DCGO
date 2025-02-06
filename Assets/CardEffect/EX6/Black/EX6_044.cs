using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.EX6
{
    public class EX6_044 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 5)
                    {
                        return targetPermanent.TopCard.ContainsTraits("Legend-Arms");
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 4,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region Blocker/Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(
                                isInheritedEffect: false,
                                card: card,
                                condition: null));

                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(
                isInheritedEffect: true,
                card: card,
                condition: null));

                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #region Hand - Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<De-Digivolve 1> all of your opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsDigimonEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Hand][Main] By paying 3 cost and placing this card as the bottom digivolution card of 1 of your Digimon that's level 6 or has the [Legend-Arms] trait, <De-Digivolve 1> all of your opponent's Digimon with as much or less DP than that Digimon.";
                }

                bool IsLevel6OrHasLegendArmsTrait(Permanent targetPermanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(targetPermanent, card))
                    {
                        if (targetPermanent.TopCard.HasLevel && targetPermanent.Level == 6)
                            return true;

                        if (targetPermanent.TopCard.ContainsTraits("Legend-Arms"))
                            return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnHand(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsLevel6OrHasLegendArmsTrait))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;

                    if (CardEffectCommons.HasMatchConditionPermanent(IsLevel6OrHasLegendArmsTrait))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsLevel6OrHasLegendArmsTrait));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsLevel6OrHasLegendArmsTrait,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to add bottom digivolution source.", "The opponent is selecting 1 Digimon to add bottom digivolution source.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }
                    }

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(-3, activateClass));

                        yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(
                                new List<CardSource>() { card },
                                activateClass));

                        List<Permanent> enemyPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Where(x => x.DP <= selectedPermanent.DP).ToList();

                        foreach(Permanent permanent in enemyPermanents)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IDegeneration(permanent, 1, activateClass).Degeneration());
                        }
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unaffected by opponents Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] 1 of your Digimon isn't affected by your opponent's Digimon effects until the end of your opponent's turn.";
                }

                bool IsYourDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;

                    if (CardEffectCommons.HasMatchConditionPermanent(IsYourDigimon))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsYourDigimon));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsYourDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will not be affected by the effect's of your opponent's Digimon.", "The opponent is selecting 1 Digimon that will not be affected by the effect's of your Digimon.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }
                    }

                    if(selectedPermanent != null)
                    {
                        CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                        canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's Digimon's effects", CanUseConditionImmunity, card);
                        canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                        selectedPermanent.UntilOpponentTurnEndEffects.Add(GetCardEffect);

                        bool CanUseConditionImmunity(Hashtable hashtable)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(selectedPermanent, card);
                        }

                        bool CardCondition(CardSource cardSource)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                            {
                                if (cardSource == selectedPermanent.TopCard)
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        bool SkillCondition(ICardEffect cardEffect)
                        {
                            if (CardEffectCommons.IsOpponentEffect(cardEffect, card))
                            {
                                if (cardEffect.IsDigimonEffect)
                                {
                                    return true;
                                }

                                if (cardEffect.IsDigimonEffect && cardEffect.IsSecurityEffect)
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        ICardEffect GetCardEffect(EffectTiming _timing)
                        {
                            if (_timing == EffectTiming.None)
                            {
                                return canNotAffectedClass;
                            }

                            return null;
                        }
                    }
                }
            }
            #endregion

            #region Opponent's Turn - ESS
            if (timing == EffectTiming.None)
            {
                if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                {
                    if(card.PermanentOfThisCard().TopCard != card)
                    {
                        Permanent selectedPermanent = card.PermanentOfThisCard();

                        CanNotBeRemovedClass canNotBeRemovedClass = new CanNotBeRemovedClass();
                        canNotBeRemovedClass.SetUpICardEffect("Can't leave battle area except by deletion effect", CanUseProtectionCondition, card);
                        canNotBeRemovedClass.SetUpCanNotBeRemovedClass(permanentCondition: PermanentCondition);
                        canNotBeRemovedClass.SetIsInheritedEffect(true);

                        if(!cardEffects.Contains(canNotBeRemovedClass))
                            cardEffects.Add(canNotBeRemovedClass);

                        bool CanUseProtectionCondition(Hashtable hashtable)
                        {
                            if (CardEffectCommons.IsOpponentTurn(card))
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                {
                                    if (!CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOwnerEffect(cardEffect, card)))
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

                        bool PermanentCondition(Permanent permanent)
                        {

                            if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                            {
                                if (permanent == selectedPermanent) 
                                {
                                    if (selectedPermanent.TopCard != card)
                                    {
                                        if (selectedPermanent.TopCard.CardNames.Contains("RagnaLoardmon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}