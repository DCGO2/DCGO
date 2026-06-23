using System;
using System.Collections;
using System.Collections.Generic;

// Kapurimon
namespace DCGO.CardEffects.EX12
{
    public class EX12_003 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Inherit
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May DNA 1 of the leaving Digimon and another Digimon into an [ME] in hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When any of your [ME] trait Digimon would leave the battle area other than by your effects, 1 of them and any of your other Digimon may DNA digivolve into an [ME] trait Digimon card in the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, PermanentCondition)
                        && !CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOwnerEffect(cardEffect, card));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectDNACondition)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent != card.PermanentOfThisCard());
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsTraits("ME");
                }

                bool CanSelectDNACondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("ME")
                        && CardEffectCommons.CanPlayJogress(true);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> allowedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable).Filter(PermanentCondition);

                    Func<Permanent, bool>[] permanentConditions = new Func<Permanent, bool>[] { allowedPermanents.Contains };//one of the 2 must be a to be removed digimon

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DNADigivolvePermanentsIntoHandOrTrashCard(
                        canSelectDNACardCondition: CanSelectDNACondition,
                        payCost: true,
                        isHand: true,
                        activateClass,
                        permanentConditions
                    ));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
