using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT14
{
    public class BT14_013 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reduce digivolution cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] For the turn, when this Digimon would digivolve into a card with [Tyrannomon] in its name, or the [Dinosaur] or [Ceratopsian] trait, reduce the digivolution cost by 1.";
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
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        Permanent selectedPermanent = card.PermanentOfThisCard();

                        if (selectedPermanent != null)
                        {
                            bool Condition()
                            {
                                return CardEffectCommons.IsExistOnBattleArea(card);
                            }

                            bool PermanentCondition(Permanent targetPermanent)
                            {
                                return targetPermanent == card.PermanentOfThisCard();
                            }

                            bool CardSourceCondition(CardSource cardSource)
                            {
                                if (cardSource.ContainsCardName("Tyrannomon"))
                                {
                                    return true;
                                }

                                if (cardSource.CardTraits.Contains("Dinosaur"))
                                {
                                    return true;
                                }

                                if (cardSource.CardTraits.Contains("Ceratopsian"))
                                {
                                    return true;
                                }

                                return false;
                            }

                            ChangeCostClass changeCostClass = CardEffectFactory.ChangeDigivolutionCostStaticEffect(
                                changeValue: -1,
                                permanentCondition: PermanentCondition,
                                cardCondition: CardSourceCondition,
                                rootCondition: null,
                                isInheritedEffect: false,
                                card: card,
                                condition: Condition,
                                setFixedCost: false);

                            selectedPermanent.UntilEachTurnEndEffects.Add((_timing) => changeCostClass);

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(selectedPermanent));
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon attacks", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Attack_BT14_013");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn][Once Per Turn] If this Digimon has [Tyrannomon] in its name, or the [Dinosaur] or [Ceratopsian] trait, it may attack.";
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
                        if (card.PermanentOfThisCard().CanAttack(activateClass))
                        {
                            if (card.PermanentOfThisCard().TopCard.ContainsCardName("Tyrannomon"))
                            {
                                return true;
                            }

                            if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("Dinosaur"))
                            {
                                return true;
                            }

                            if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("Ceratopsian"))
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
                        if (card.PermanentOfThisCard().CanAttack(activateClass))
                        {
                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                            selectAttackEffect.SetUp(
                                attacker: card.PermanentOfThisCard(),
                                canAttackPlayerCondition: () => true,
                                defenderCondition: (permanent) => true,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}