using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Altea
namespace DCGO.CardEffects.EX11
{
    public class EX11_064 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Main
            if (timing == EffectTiming.OnStartMainPhase)
            {
                cardEffects.Add(CardEffectFactory.Gain1MemoryTamerOpponentDigimonEffect(card));
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Flip security card face up", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] Flip your opponent's top face-down security card face up.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           card.Owner.Enemy.SecurityCards.Count(source => source.IsFlipped) > 0;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    foreach (CardSource source in card.Owner.Enemy.SecurityCards)
                    {
                        if (!source.IsFlipped)
                            continue;

                        yield return ContinuousController.instance.StartCoroutine(new IFlipSecurity(source).FlipFaceUp());

                        break;
                    }

                    yield return null;
                }
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve attacker into [Cyborg] or [Machine] with cost reduced by # of face up security.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() 
                    => "[Your Turn] When one of your [Cyborg] or [Machine] trait Digimon attacks, by suspending this Tamer, that Digimon may digivolve into a [Cyborg] or [Machine] trait Digimon card in the hand. For each of your opponent's face-up security cards, reduce this effect's digivolution cost by 1.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, PermanentCondition);
                } 

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.EqualsTraits("Cyborg") || permanent.TopCard.EqualsTraits("Machine"));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && (cardSource.EqualsTraits("Cyborg") || cardSource.EqualsTraits("Machine"));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    int faceUpCards = card.Owner.Enemy.SecurityCards.Count(CardSource => !CardSource.IsFlipped);

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: GManager.instance.attackProcess.AttackingPermanent,
                            cardCondition: CanSelectCardCondition,
                            payCost: true,
                            reduceCostTuple: (reduceCost: faceUpCards, reduceCostCardCondition: null),
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null
                        ));
                }
            }
            #endregion

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}
