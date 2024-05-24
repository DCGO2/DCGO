using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class Zanmetsumon_BT16_062 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            # region Alternate Digivolution Requirement [Digivolve] Lv.4 w/[Gammamon] in text: Cost 3
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4)
                    {
                        if (targetPermanent.TopCard.HasText("Gammamon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region OnPlay

            #endregion

            #region When Digivolved

            #endregion

            #region All Turns
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
            #endregion

            #region All Turns - Inheritable
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
            #endregion

            return cardEffects;
        }
    }
}