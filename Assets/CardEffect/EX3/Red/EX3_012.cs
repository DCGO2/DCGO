using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX3
{
    public class EX3_012 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete oppponent's all Digimons with the lowest DP or opponent can't play Digimon with 5000 DP or less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Delete all of your opponent's Digimon with the lowest DP. If no Digimon is deleted by this effect, your opponent can't play Digimon with 5000 DP or less until the end of their turn.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<Permanent> destroyTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(PermanentCondition);
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: destroyTargetPermanents, activateClass: activateClass, successProcess: null, failureProcess: FailureProcess));

                    IEnumerator FailureProcess()
                    {
                        CanNotPutFieldClass canNotPutFieldClass = new CanNotPutFieldClass();
                        canNotPutFieldClass.SetUpICardEffect("Can't play Digimon with 5000 DP or less", CanUseCondition1, card);
                        canNotPutFieldClass.SetUpCanNotPutFieldClass(cardCondition: CardCondition, cardEffectCondition: CardEffectCondition);
                        card.Owner.Enemy.UntilOwnerTurnEndEffects.Add((_timing) => canNotPutFieldClass);

                        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().DebuffSE);

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            return true;
                        }

                        bool CardCondition(CardSource cardSource)
                        {
                            if (cardSource.IsDigimon || cardSource.IsDigiEgg)
                            {
                                if (cardSource.HasDP && cardSource.CardDP <= 5000)
                                {
                                    return true;
                                }
                            }

                            return cardSource.Owner == card.Owner.Enemy &&
                                   (cardSource.IsDigimon || cardSource.IsDigiEgg) &&
                                   (cardSource.HasDP && cardSource.CardDP <= 5000);
                        }

                        bool CardEffectCondition(ICardEffect cardEffect)
                        {
                            return true;
                        }

                        yield return null;
                    }
                }
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Trash the top card of opponent's security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] If you have a Tamer in play, trash the top card of your opponent's security stack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.Enemy.SecurityCards.Count >= 1)
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsTamer))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                }
            }

            return cardEffects;
        }
    }
}