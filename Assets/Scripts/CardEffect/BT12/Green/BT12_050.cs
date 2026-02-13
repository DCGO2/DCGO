using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT12
{
    public class BT12_050 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetHashString("Memory+1_BT12_050");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] If this Digimon would DNA digivolve into a blue Digimon card, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            bool CardCondition(CardSource cardSource)
                            {
                                if (cardSource.CardColors.Contains(CardColor.Blue))
                                {
                                    if (cardSource.IsDigimon)
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolveOfCard(hashtable, CardCondition, card))
                            {
                                if (CardEffectCommons.IsJogress(hashtable))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.CanAddMemory(activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (card.PermanentOfThisCard().TopCard.ContainsCardName("Imperialdramon"))
                            {
                                return true;
                            }

                            if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("Free"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: true, card: card, condition: Condition));
            }

            return cardEffects;
        }
    }
}