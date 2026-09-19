using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT4_101 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OptionSkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] All of your Digimon gain \"[Your Turn] When attacking an opponent's Digimon with no digivolution cards, delete that Digimon\" for the turn.";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                foreach (Permanent permanent in card.Owner.GetBattleAreaPermanents())
                {
                    if (PermanentCondition(permanent))
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(permanent));
                    }
                }

                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("Gain Delete the Digimon", CanUseCondition1, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilEachTurnEnd, card: card, cardEffect: addSkillClass, timing: EffectTiming.None);

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
                            return true;
                        }
                    }

                    return false;
                }

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                {
                    if (_timing == EffectTiming.OnAllyAttack)
                    {
                        ActivateClass activateClass1 = new ActivateClass();
                        activateClass1.SetUpICardEffect("Delete the Digimon", CanUseCondition2, cardSource);
                        activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                        cardEffects.Add(activateClass1);

                        if (cardSource.PermanentOfThisCard() != null)
                        {
                            activateClass1.SetEffectSourcePermanent(cardSource.PermanentOfThisCard());
                        }

                        string EffectDiscription1()
                        {
                            return "[Your Turn] When attacking an opponent's Digimon with no digivolution cards, delete that Digimon";
                        }

                        bool CanUseCondition2(Hashtable hashtable)
                        {
                            if (CardSourceCondition(cardSource))
                            {
                                if (CardEffectCommons.CanTriggerOnAttack(hashtable, cardSource))
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(GManager.instance.attackProcess.DefendingPermanent))
                                    {
                                        if (GManager.instance.attackProcess.DefendingPermanent.HasNoDigivolutionCards)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        bool CanActivateCondition1(Hashtable hashtable)
                        {
                            if (CardEffectCommons.IsExistOnBattleArea(cardSource))
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(GManager.instance.attackProcess.DefendingPermanent))
                                {
                                    if (GManager.instance.attackProcess.DefendingPermanent.CanBeDestroyedBySkill(activateClass1))
                                    {
                                        if (!GManager.instance.attackProcess.DefendingPermanent.TopCard.CanNotBeAffected(activateClass1))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        IEnumerator ActivateCoroutine1(Hashtable _hashtable)
                        {
                            Permanent permanent = GManager.instance.attackProcess.DefendingPermanent;

                            yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                                new List<Permanent>() { permanent },
                                CardEffectCommons.CardEffectHashtable(activateClass1)).Destroy());
                        }
                    }

                    return cardEffects;
                }
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect($"Add this card to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] Add this card to its owner's hand.";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(
                    card,
                    activateClass));
            }
        }

        return cardEffects;
    }
}
