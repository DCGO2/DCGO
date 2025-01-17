using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT19
{
    public class BT19_048 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel3 &&
                           targetPermanent.TopCard.EqualsTraits("Royal Base");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false,
                    card: card, condition: null));
            }

            #endregion

            #region Your Turn
            if (timing == EffectTiming.WhenRemoveField)
            {
                List<Permanent> removedPermanents = new List<Permanent>();

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place face up on bottom of security, to prevent other [Royal Base] trait digimon from leaving the battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When any of your other Digimon with the [Royal Base] trait would leave the battle area by effects, by placing this Digimon face up as the bottom security card, they don't leave.";
                }

                bool HasOtherRoyalBaseDigimon(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent != card.PermanentOfThisCard())
                            return permanent.TopCard.EqualsTraits("Royal Base");
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, HasOtherRoyalBaseDigimon))
                        {
                            if (CardEffectCommons.IsByEffect(hashtable, null))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    removedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable).Filter(HasOtherRoyalBaseDigimon);

                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IPutSecurityPermanent(
                        card.PermanentOfThisCard(), 
                        CardEffectCommons.CardEffectHashtable(activateClass), toTop: false, isFaceup: true).PutSecurity());

                    if (card.Owner.SecurityCards.Contains(card))
                    {
                        foreach (Permanent removed in removedPermanents)
                        {
                            removed.willBeRemoveField = false;

                            removed.HideHandBounceEffect();
                            removed.HideDeckBounceEffect();
                            removed.HideWillRemoveFieldEffect();
                        }
                    }
                }
            }
            #endregion

            #region All Turns - ESS
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 1000, isInheritedEffect: true, card: card, condition: Condition));
            }
            #endregion

            #region All Turns - Security
            if (timing == EffectTiming.None)
            {
                string EffectDiscription()
                {
                    return "(Security) [All Turns] All of your [Royal Base] trait Digimon get +1000 DP.";
                }

                bool Condition()
                {
                    return CardEffectCommons.IsExistInSecurity(card, false);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.EqualsTraits("Royal Base"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                permanentCondition: PermanentCondition,
                changeValue: 1000,
                isInheritedEffect: false,
                card: card,
                condition: Condition,
                effectName: EffectDiscription));
            }
            #endregion

            return cardEffects;
        }
    }
}