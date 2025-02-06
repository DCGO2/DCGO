using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX1
{
    public class EX1_072 : CEntity_Effect
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
                    return "[Main] Your opponent can't use Option cards until the end of their next turn.";
                }
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().DebuffSE);

                    CanNotPlayClass canNotPlayClass = new CanNotPlayClass();
                    canNotPlayClass.SetUpICardEffect("Can't play option", (hashtable) => true, card);
                    canNotPlayClass.SetUpCanNotPlayClass(cardCondition: CardCondition);
                    CardEffectCommons.AddEffectToPlayer(
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        card: card,
                        cardEffect: canNotPlayClass,
                        timing: EffectTiming.None);

                    bool CardCondition(CardSource cardSource)
                    {
                        if (cardSource.Owner == card.Owner.Enemy)
                        {
                            if (cardSource.IsOption)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    yield return null;
                }
            }


            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Opponent can't play option and add this card to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] Your opponent can't use Option cards this turn. Then, add this card to its owner's hand.";
                }
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().DebuffSE);

                    CanNotPlayClass canNotPlayClass = new CanNotPlayClass();
                    canNotPlayClass.SetUpICardEffect("Can't play option", (hashtable) => true, card);
                    canNotPlayClass.SetUpCanNotPlayClass(cardCondition: CardCondition);
                    CardEffectCommons.AddEffectToPlayer(
                        effectDuration: EffectDuration.UntilEachTurnEnd,
                        card: card,
                        cardEffect: canNotPlayClass,
                        timing: EffectTiming.None);

                    bool CardCondition(CardSource cardSource)
                    {
                        if (cardSource.Owner == card.Owner.Enemy)
                        {
                            if (cardSource.IsOption)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
                }
            }

            return cardEffects;
        }
    }
}