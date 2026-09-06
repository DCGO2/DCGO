using System.Collections;
using System.Collections.Generic;

// Hiroko Sagisaka
namespace DCGO.CardEffects.BT26
{
    public class BT26_088 : CEntity_Effect
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

            #region Cost reduction
            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspending this Tamer, reduce a [Boss]/[TS] Digimon's play cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Your Turn] When any [Boss] or [TS] trait Digimon cards would be played, by suspending this Tamer, reduce the cost by 1. If you have no Digimon, instead reduce the cost by 2.";

                bool PlayCardCondition(CardSource cardSource)
                    => cardSource.IsDigimon
                        && (cardSource.EqualsTraits("Boss") || cardSource.HasTSTraits)
                        && cardSource.Owner == card.Owner;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, PlayCardCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass,
                        successProcess: SuccessProcess,
                        failureProcess: null));

                    IEnumerator SuccessProcess(List<Permanent> _permanents)
                    {
                        int reduceAmount = card.Owner.GetBattleAreaDigimons().Count == 0 ? 2 : 1;

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect($"Play Cost -{reduceAmount}", _ => true, card);
                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: _ => true, isUpDown: () => true, isCheckAvailability: () => false, isChangePayingCost: () => true);
                        card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                        int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                            => CardSourceCondition(cardSource) ? cost - reduceAmount : cost;

                        bool CardSourceCondition(CardSource cardSource)
                            => cardSource != null && cardSource.Owner == card.Owner
                                && cardSource.IsDigimon && (cardSource.EqualsTraits("Boss") || cardSource.HasTSTraits);
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
