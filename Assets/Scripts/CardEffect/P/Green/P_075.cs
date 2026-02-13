using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class P_075 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.BeforePayCost)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Opponent's Digimons gain effect", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetHashString("Debuff_P_075");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Your Turn] When this Digimon digivolves into a card with [Insectoid] in its traits, all of your opponent's Digimon gain \"[All Turns] When this Digimon is suspended, lose 1 memory.\" until the end of your opponent's turn.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(hashtable, (permanent) => permanent == card.PermanentOfThisCard(), (cardSource) => cardSource.CardTraits.Contains("Insectoid")))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card);
                }

                foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaDigimons())
                {
                    if (PermanentCondition(permanent))
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(permanent));
                    }
                }

                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("Memory -1", CanUseCondition1, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                card.Owner.UntilOpponentTurnEndEffects.Add((_timing) => addSkillClass);

                bool CanUseCondition1(Hashtable hashtable)
                {
                    return true;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    if (PermanentCondition(cardSource.PermanentOfThisCard()))
                    {
                        if (cardSource == cardSource.PermanentOfThisCard().TopCard)
                        {
                            if (cardSource.PermanentOfThisCard().IsDigimon)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                {
                    if (_timing == EffectTiming.OnTappedAnyone)
                    {
                        ActivateClass activateClass1 = new ActivateClass();
                        activateClass1.SetUpICardEffect("Memory -1", CanUseCondition2, cardSource);
                        activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                        cardEffects.Add(activateClass1);

                        if (cardSource.PermanentOfThisCard() != null)
                        {
                            activateClass1.SetEffectSourcePermanent(cardSource.PermanentOfThisCard());
                        }

                        string EffectDiscription1()
                        {
                            return "[All Turns] When this Digimon is suspended, lose 1 memory.";
                        }

                        bool CanUseCondition2(Hashtable hashtable)
                        {
                            if (CardSourceCondition(cardSource))
                            {
                                if (CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, (permanent) => permanent == cardSource.PermanentOfThisCard()))
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        bool CanActivateCondition1(Hashtable hashtable)
                        {
                            if (CardEffectCommons.IsExistOnBattleArea(cardSource))
                            {
                                return true;
                            }

                            return false;
                        }

                        IEnumerator ActivateCoroutine1(Hashtable _hashtable)
                        {
                            yield return ContinuousController.instance.StartCoroutine(cardSource.Owner.AddMemory(-1, activateClass1));
                        }
                    }

                    return cardEffects;
                }
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
                        if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("Insectoid"))
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
