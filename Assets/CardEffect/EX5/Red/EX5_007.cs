using System.Collections;
using System.Collections.Generic;

public class EX5_007 : CEntity_Effect
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
            activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Start of Your Main Phase] If you have a Tamer with the [Light Fang]/[Night Claw] trait, gain 1 memory.";
            }

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                {
                    if (permanent.IsTamer)
                    {
                        if (permanent.TopCard.CardTraits.Contains("Light Fang"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardTraits.Contains("LightFung"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardTraits.Contains("Night Claw"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardTraits.Contains("NightClaw"))
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
                        return true;
                    }
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, PermanentCondition))
                    {
                        if (card.Owner.CanAddMemory(activateClass))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
            }
        }

        if (timing == EffectTiming.OnDeclaration)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Place the top card of this Digimon at the bottom of digivolution cards to gain Memory +2 (Coronamon)", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, false, EffectDiscription());
            activateClass.SetIsInheritedEffect(true);
            activateClass.SetHashString("ReturnDigivolutionCards_EX50_007");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] [Once Per Turn] By placing the top card of this Digimon with the [Light Fang]/[Night Claw] trait as this Digimon's bottom digivolution card, gain 2 memory.";
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
