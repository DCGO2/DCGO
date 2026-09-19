using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX5
{
    public class EX5_016 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 2)
                    {
                        if (targetPermanent.TopCard.CardTraits.Contains("Light Fang"))
                        {
                            return true;
                        }

                        if (targetPermanent.TopCard.CardTraits.Contains("LightFung"))
                        {
                            return true;
                        }

                        if (targetPermanent.TopCard.CardTraits.Contains("Night Claw"))
                        {
                            return true;
                        }

                        if (targetPermanent.TopCard.CardTraits.Contains("NightClaw"))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return your 1 Digimon to hand to gain Memory +2", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] By returning 1 of your Digimon to the hand, gain 2 memory.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        Permanent bounceTargetPermanent = null;

                        int maxCount = Math.Min(1, card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

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

                        selectPermanentEffect.SetUpCustomMessage("Select 1 your Digimon to return to hand.", "The opponent is selecting 1 your Digimon to return to hand.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            bounceTargetPermanent = permanent;

                            yield return null;
                        }

                        if (bounceTargetPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.BouncePeremanentAndProcessAccordingToResult(
                                targetPermanents: new List<Permanent>() { bounceTargetPermanent },
                                activateClass: activateClass,
                                successProcess: SuccessProcess(),
                                failureProcess: null));

                            IEnumerator SuccessProcess()
                            {
                                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(2, activateClass));
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place the top card of this Digimon at the bottom of digivolution cards to gain Memory +2 (Lunamon)", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("ReturnDigivolutionCards_EX50_016");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] [Once Per Turn] By placing the top card of this Digimon with the [Night Claw] or [Light Fang] trait as this Digimon's bottom digivolution card, gain 2 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count >= 1)
                        {
                            if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("Light Fang"))
                            {
                                return true;
                            }

                            if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("LightFung"))
                            {
                                return true;
                            }

                            if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("Night Claw"))
                            {
                                return true;
                            }

                            if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("NightClaw"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count >= 1)
                        {
                            CardSource topCard = card.PermanentOfThisCard().TopCard;

                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                                new List<CardSource>() { topCard },
                                activateClass));

                            yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(2, activateClass));
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}
