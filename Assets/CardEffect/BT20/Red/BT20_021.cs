using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT20
{
    public class BT20_021 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region DNA Digivolution
            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect($"DNA Digivolution", CanUseCondition, card);
                addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
                addJogressConditionClass.SetNotShowUI(true);
                cardEffects.Add(addJogressConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                JogressCondition GetJogress (CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        bool PermanentCondition1 (Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && permanent.TopCard.ContainsCardName("Jesmon")
                                && permanent.Levels_ForJogress(card).Contains(6);
                        }

                        bool PermanentCondition2 (Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && permanent.TopCard.ContainsCardName("Gankoomon");
                                && permanent.Levels_ForJogress(card).Contains(6);
                        }

                        JogressConditionElement[] elements =
                        {
                            new (PermanentCondition1, "a level 6 with [Jesmon] in name"),
                            new (PermanentCondition2, "a level 6 with [Gankoomon] in name")
                        };

                        JogressCondition jogress_condition = new JogressCondition(elements, 0);
                        return jogress_condition;
                    }

                    return null;
                }
      
            }
            #endregion

            #region BlastDigivolve
            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }
            #endregion

            #region OnPlay - WhenDigivolve - When Attacking
            if (timing == EffectTiming.OnEnterFieldAnyone || timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activate_class = new ActivateClass();
                activate_class.SetUpICardEffect ("Select 1 card, delete 1 card", CanUseCondition, card);
                activate_class.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                cardEffects.Add(activate_class);

                string EffectDescription()
                {
                    return "[OnPlay] [When Digivolving] [When Attacking] [Once Per Turn] Place 1 [Royal Knight] trait card from your hand or trash as this Digimon's bottom digivolution card, delete 1 of your opponent's Digimon with as much or less DP as this digimon.";
                }


            }

            return cardEffects;
        }
    }
}