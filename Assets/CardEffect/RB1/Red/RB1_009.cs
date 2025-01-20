using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.RB1
{
    public class RB1_009 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Gammamon") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return card.Owner.HandCards.Contains(card);
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(targetPermanent))
                    {
                        if (targetPermanent.TopCard.CardNames.Contains("Gammamon"))
                        {
                            if (targetPermanent.DigivolutionCards.Count(cardSource => cardSource.ContainsCardName("Gammamon")) >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: true, card: card, condition: Condition));
            }

            if (timing == EffectTiming.None)
            {
                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("This Digimon gains all effects of [Gammamon] in digivolution cards", CanUseCondition, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);

                cardEffects.Add(addSkillClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    if (PermanentCondition(cardSource.PermanentOfThisCard()))
                    {
                        if (cardSource == cardSource.PermanentOfThisCard().TopCard)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                {
                    if (CardSourceCondition(cardSource))
                    {
                        foreach (CardSource cardSource1 in cardSource.PermanentOfThisCard().DigivolutionCards)
                        {
                            if (cardSource1.ContainsCardName("Gammamon"))
                            {
                                foreach (ICardEffect cardEffect in cardSource1.cEntity_EffectController.GetCardEffects_ExceptAddedEffects(_timing, card))
                                {
                                    if (!cardEffect.IsSecurityEffect && !cardEffect.IsInheritedEffect)
                                    {
                                        cardEffects.Add(cardEffect);
                                    }
                                }
                            }
                        }
                    }

                    return cardEffects;
                }
            }

            if (timing == EffectTiming.None)
            {
                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("This Digimon gains all effects of [Gammamon] in digivolution cards", CanUseCondition, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                addSkillClass.SetIsInheritedEffect(true);
                cardEffects.Add(addSkillClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    if (PermanentCondition(cardSource.PermanentOfThisCard()))
                    {
                        if (cardSource == cardSource.PermanentOfThisCard().TopCard)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                {
                    if (CardSourceCondition(cardSource))
                    {
                        foreach (CardSource cardSource1 in cardSource.PermanentOfThisCard().DigivolutionCards)
                        {
                            if (cardSource1.ContainsCardName("Gammamon"))
                            {
                                foreach (ICardEffect cardEffect in cardSource1.cEntity_EffectController.GetCardEffects_ExceptAddedEffects(_timing, card))
                                {
                                    if (!cardEffect.IsSecurityEffect && !cardEffect.IsInheritedEffect)
                                    {
                                        cardEffects.Add(cardEffect);
                                    }
                                }
                            }
                        }
                    }

                    return cardEffects;
                }
            }

            return cardEffects;
        }
    }
}