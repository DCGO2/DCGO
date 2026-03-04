using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.AD1
{
    public class AD1_022 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                cardEffects.Add(CardEffectFactory.Gain1MemoryTamerOpponentDigimonEffect(card));
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ();
                activateClass.SetUpICardEffect("By suspending this tamer, 1 of your [ADVENTURE] Digimon may digivolve for -1 per 2 colors on your Tamers.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Your Turn] When any of your other [ADVENTURE] trait Digimon orTamers are played, by suspending this Tamer, 1 of your Digimon may digivolve into an [ADVENTURE] trait Digimon in the hand. For every 2 of your Tamers' colors, reduce this effect's digivolution cost by 1.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent != card.PermanentOfThisCard()
                        && (permanent.IsDigimon || permanent.IsTamer)
                        && permanent.TopCard.EqualsTraits("ADVENTURE");
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool IsAdventureDigimon(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.EqualsTraits("ADVENTURE");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(
                        new List<Permanent>() { card.PermanentOfThisCard() },
                        CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    List<CardSource> tamerCards = new List<CardSource>();

                    foreach (Permanent permanent in card.Owner.GetBattleAreaPermanents())
                    {
                        if (permanent.IsTamer)
                        {
                            tamerCards.Add(permanent.TopCard);
                        }
                    }

                    int reduceCost = Combinations.GetUniqueColorCardCount(tamerCards) / 2;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        card.PermanentOfThisCard(),
                        IsAdventureDigimon,
                        payCost: true,
                        reduceCostTuple: (reduceCost, null),
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: true,
                        activateClass,
                        successProcess: null
                    ));
                }
            }
            #endregion

            #region Security 
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}