using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DCGO.CardEffects.EX6
{
    public class EX6_037 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Sakuttomon") || targetPermanent.TopCard.ContainsCardName("Kakkinmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, 
                    digivolutionCost: 0, 
                    ignoreDigivolutionRequirement: false, 
                    card: card, 
                    condition: null));
            }
            #endregion

            #region Hand - Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Pay 1 Memory, Place as bottom Digivolution source to Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Hand] [Main] By paying 1 cost and placing this card as the bottom digivolution card of 1 of your Digimon that's level 3 or has the [Legend-Arms] trait, <Draw 1>.";
                }

                bool IsLevel3OrHasLegendArmsTrait(Permanent targetPermanent)
                {
                    if(CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(targetPermanent, card))
                    {
                        if (targetPermanent.TopCard.HasLevel && targetPermanent.Level == 3)
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
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsLevel3OrHasLegendArmsTrait))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsLevel3OrHasLegendArmsTrait))
                    {
                        Permanent selectedPermanent = null;

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsLevel3OrHasLegendArmsTrait));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsLevel3OrHasLegendArmsTrait,
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

                        if(selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(
                                new List<CardSource>() { card },
                                activateClass));

                            yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(-1, activateClass));
                            yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                        }
                    }
                }
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 [Legend-Arms], <Draw 2>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By trashing 1 card with the [Legend-Arms] trait in your hand, <Draw 2>.";
                }

                bool HasLegendArmInTrait(CardSource cardSource)
                {
                    return cardSource.ContainsTraits("Legend-Arms");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Count(HasLegendArmInTrait) > 0)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            canTargetCondition: HasLegendArmInTrait,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectHandEffect.Mode.Discard,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 card with [Legend-Arms] trait to discard.", "The opponent is selecting 1 card with [Legend-Arms] trait to discard.");
                        
                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count > 0)
                                yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 2, activateClass).Draw());

                            yield return null;
                        }
                    }
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 3000 DP or less Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("WhenAttacking_EX6_037");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] Delete 1 of your opponent's Digimon with 3000 DP or less.";
                }

                bool DigimonTarget(Permanent permanent)
                {
                    if(CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if(permanent.HasDP && permanent.DP <= 3000)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(DigimonTarget))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(DigimonTarget));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: DigimonTarget,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}