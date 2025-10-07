using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT9_044 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent.TopCard.CardNames.Contains("Magnamon");
            }

            cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
        }

        if (timing == EffectTiming.OnAllyAttack)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Switch attack target to this Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Opponentfs Turn] When an opponentfs Digimon attacks, if a card with [Armor Form] in its traits or [X Antibody] is in this Digimonfs digivolution cards, you may switch the target of attack to this Digimon.";
            }

            bool PermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsOpponentPermanent(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOpponentTurn(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, PermanentCondition))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardTraits.Contains("Armor Form") || cardSource.CardTraits.Contains("ArmorForm") || cardSource.CardNames.Contains("X Antibody") || cardSource.CardNames.Contains("XAntibody")) >= 1)
                    {
                        if (GManager.instance.attackProcess.AttackingPermanent.CanSwitchAttackTarget)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SwitchDefender(activateClass, false, card.PermanentOfThisCard()));
            }
        }

        if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Place top card of this Digimon to security to prevent deletion", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            activateClass.SetHashString("Substitute_BT9_044");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[All Turns] When this Digimon would be deleted, you may place the top card of this Digimon on top of your security stack face down to prevent that deletion.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card))
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
                    if (card.PermanentOfThisCard().DigivolutionCards.Count >= 1)
                    {
                        if (card.Owner.CanAddSecurity(activateClass))
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
                    if (card.PermanentOfThisCard().DigivolutionCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(card.PermanentOfThisCard()));

                        CardSource cardSource = card.PermanentOfThisCard().TopCard;
                        Permanent permanent = card.PermanentOfThisCard();

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(cardSource));

                        permanent.ShowingPermanentCard.ShowPermanentData(true);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().RemoveDigivolveRootEffect(cardSource, permanent));

                        if (!cardSource.IsToken)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(cardSource));

                            permanent.willBeRemoveField = false;

                            if (permanent.ShowingPermanentCard != null)
                            {
                                if (permanent.ShowingPermanentCard.WillBeDeletedObject != null)
                                {
                                    permanent.ShowingPermanentCard.WillBeDeletedObject.SetActive(false);
                                }
                            }
                        }
                    }
                }
            }
        }

        return cardEffects;
    }
}
