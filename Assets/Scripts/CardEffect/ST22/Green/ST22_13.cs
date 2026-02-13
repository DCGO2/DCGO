using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

//GrandGalemon
namespace DCGO.CardEffects.ST22
{
    public class ST22_13 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static effects

                 #region Fortitude

                if (timing == EffectTiming.OnDestroyedAnyone)
                {
                    cardEffects.Add(CardEffectFactory.FortitudeSelfEffect(isInheritedEffect: false, card: card, condition: null));
                }

                #endregion

                #region Vortex

                if (timing == EffectTiming.OnEndTurn)
                {
                    cardEffects.Add(CardEffectFactory.VortexSelfEffect(isInheritedEffect: false, card: card,
                        condition: null));
                }

                #endregion

            #endregion

            #region Shared (On Play/When Digivolving/When Attacking)

            bool SelectDigimonToSuspend(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);
            }

            bool CanActivateSharedCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }
            
            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: SelectDigimonToSuspend,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to suspend.", "The opponent is selecting 1 Digimon to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                        targetPermanent: card.PermanentOfThisCard(),
                        changeValue: 3000,
                        effectDuration: EffectDuration.UntilEachTurnEnd,
                        activateClass: activateClass));
                }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] You may suspend 1 Digimon. Then, this Digimon get +3000 DP for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] You may suspend 1 Digimon. Then, this Digimon get +3000 DP for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] You may suspend 1 Digimon. Then, this Digimon get +3000 DP for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                     return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
            }

            #endregion

            #region When Attacking - ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("ST22_13_ESS");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return "[When Attacking] If your opponent has no unsuepended Digimon, this Digimon with the [Vortex Warriors] trait may unsuspend.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                     return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool OpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           !permanent.IsSuspended;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return (CardEffectCommons.MatchConditionOpponentsPermanentCount(card,OpponentsDigimon) == 0)
                        && card.PermanentOfThisCard().TopCard.EqualsTraits("Vortex Warriors")
                        && CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}