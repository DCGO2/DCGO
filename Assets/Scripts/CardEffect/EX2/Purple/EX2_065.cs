using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX2
{
    public class EX2_065 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 card from deck top and your Digimon can digivolve", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When you attack with a Digimon, you may suspend this Tamer to trash the top card of your deck. Then, if the attacking Digimon is [Beelzemon], you may digivolve it into a [Beelzemon Blast Mode] in your trash for a digivolution cost of 3.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Beelzemon: Blast Mode");
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        return true;
                    }
                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
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
                        if (CardEffectCommons.CanActivateSuspendCostEffect(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(1, card.Owner, activateClass).AddTrashCardsFromLibraryTop());

                    if (GManager.instance.attackProcess.AttackingPermanent != null)
                    {
                        if (GManager.instance.attackProcess.AttackingPermanent.TopCard != null)
                        {
                            if (GManager.instance.attackProcess.AttackingPermanent.TopCard.CardNames.Contains("Beelzemon"))
                            {
                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                        targetPermanent: GManager.instance.attackProcess.AttackingPermanent,
                                        cardCondition: CanSelectCardCondition,
                                        payCost: true,
                                        reduceCostTuple: null,
                                        fixedCostTuple: (fixedCost: 3, fixedCostCardCondition: null),
                                        ignoreDigivolutionRequirementFixedCost: -1,
                                        isHand: false,
                                        activateClass: activateClass,
                                        successProcess: null));
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            return cardEffects;
        }
    }
}