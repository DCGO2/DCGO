using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX2
{
    public class EX2_054 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                DontBattleSecurityDigimonClass dontBattleSecurityDigimonClass = new DontBattleSecurityDigimonClass();
                dontBattleSecurityDigimonClass.SetUpICardEffect("Ignore Battle", CanUseCondition, card);
                dontBattleSecurityDigimonClass.SetUpDontBattleSecurityDigimonClass(CardSourceCondition: CardSourceCondition);
                dontBattleSecurityDigimonClass.SetIsSecurityEffect(true);
                dontBattleSecurityDigimonClass.SetNotShowUI(true);
                cardEffects.Add(dontBattleSecurityDigimonClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanUseIgnoreBattle(hashtable, card);
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play this card without battling", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] Play this card without battling and without paying its memory cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (card.Owner.ExecutingCards.Contains(card))
                    {
                        if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: card, payCost: false, cardEffect: activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: new List<CardSource>() { card },
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Execution,
                        activateETB: true));
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Recovery +1 (Deck)", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] If you have a [Mother D-Reaper] in play, <Recovery +1 (Deck)>. (Place the top card of your deck on top of your security stack.)";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.CardNames.Contains("Mother D-Reaper"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardNames.Contains("MotherD-Reaper"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            if (card.Owner.CanAddSecurity(activateClass))
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                }
            }

            if (timing == EffectTiming.None)
            {
                bool YourPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.DigivolutionCards.Count >= 6)
                        {
                            if (permanent.TopCard.CardNames.Contains("Mother D-Reaper"))
                            {
                                return true;
                            }

                            if (permanent.TopCard.CardNames.Contains("MotherD-Reaper"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(YourPermanentCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                cardEffects.Add(CardEffectFactory.ChangeSAttackStaticEffect(
                    permanentCondition: PermanentCondition,
                    changeValue: -1,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition));
            }

            return cardEffects;
        }
    }
}