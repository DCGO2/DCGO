using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Matt Ishida & T.K. Takaishi
namespace DCGO.CardEffects.AD1
{
    public class AD1_019 : CEntity_Effect
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
                activateClass.SetUpICardEffect("By suspending this tamer, you may play 1[ADVENTURE] Digimon for -1 cost per 2 colors on your Tamers.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Your Turn] When any of your Digimon digivolve into an [ADVENTURE] trait Digimon, by suspending this Tamer, you may play 1 [ADVENTURE] trait card from your hand. For every 2 of your Tamers' colors, reduce this effect's play cost by 1.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsTraits("ADVENTURE");
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool IsAdventureDigimon(CardSource cardSource)
                {
                    return cardSource.HasPlayCost
                        && cardSource.EqualsTraits("ADVENTURE");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(
                        new List<Permanent>() { card.PermanentOfThisCard() },
                        CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, IsAdventureDigimon))
                    {
                        List<CardSource> tamerCards = new List<CardSource>();

                        foreach (Permanent permanent in card.Owner.GetBattleAreaPermanents())
                        {
                            if (permanent.IsTamer)
                            {
                                tamerCards.Add(permanent.TopCard);
                            }
                        }

                        int reduceCost = Combinations.GetUniqueColorCardCount(tamerCards) / 2;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsAdventureDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.PlayForCost,
                            cardEffect: activateClass);

                        selectHandEffect.SetReducedCostTuple((reduceCost, null));

                        selectHandEffect.SetUpCustomMessage("Select 1 digimon to play", "The opponent is selecting 1 digimon to play");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    }
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