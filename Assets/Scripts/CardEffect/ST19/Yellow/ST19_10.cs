using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.ST19
{
    public class ST19_10 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel4 &&
                           (targetPermanent.TopCard.ContainsCardName("Tyrannomon") || targetPermanent.TopCard.ContainsCardName("Raremon"));
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

            #region Armor Purge

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.ArmorPurgeEffect(card: card));
            }

            #endregion

            #region DigiXros

            if (timing == EffectTiming.None)
            {
                AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
                addDigiXrosConditionClass.SetUpICardEffect($"DigiXros -2", CanUseCondition, card);
                addDigiXrosConditionClass.SetUpAddDigiXrosConditionClass(getDigiXrosCondition: GetDigiXros);
                addDigiXrosConditionClass.SetNotShowUI(true);
                cardEffects.Add(addDigiXrosConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                DigiXrosCondition GetDigiXros(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        DigiXrosConditionElement elementTyrannomonRaremon =
                            new DigiXrosConditionElement(CanSelectCardConditionTyrannomonRaremon, "Lv.4 w/[Tyrannomon]/[Raremon] in name");

                        bool CanSelectCardConditionTyrannomonRaremon(CardSource conditionCardSource)
                        {
                            return conditionCardSource &&
                                   conditionCardSource.Owner == card.Owner &&
                                   conditionCardSource.IsDigimon &&
                                   conditionCardSource.HasLevel && conditionCardSource.IsLevel4 &&
                                   (conditionCardSource.ContainsCardName("Tyrannomon") ||
                                    conditionCardSource.ContainsCardName("Raremon"));
                        }

                        DigiXrosConditionElement elementPuppet =
                            new DigiXrosConditionElement(CanSelectCardConditionPuppet, "Lv.4 w/[Puppet] trait");

                        bool CanSelectCardConditionPuppet(CardSource conditionCardSource)
                        {
                            return conditionCardSource &&
                                   conditionCardSource.Owner == card.Owner &&
                                   conditionCardSource.IsDigimon &&
                                   conditionCardSource.HasLevel && conditionCardSource.IsLevel4 &&
                                   conditionCardSource.ContainsTraits("Puppet");
                        }

                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>()
                            { elementTyrannomonRaremon, elementPuppet };

                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, null, 2);

                        return digiXrosCondition;
                    }

                    return null;
                }
            }

            #endregion

            #region Barrier - ESS

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}