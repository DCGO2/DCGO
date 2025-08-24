using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT8_097 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            int count()
            {
                int count = 0;

                count = card.Owner.Enemy.GetBattleAreaDigimons().Count();

                return count;
            }

            ChangeCostClass changeCostClass = new ChangeCostClass();
            changeCostClass.SetUpICardEffect($"Play Cost -", CanUseCondition, card);
            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => false);

            cardEffects.Add(changeCostClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (card.Owner.HandCards.Contains(card))
                {
                    if (count() >= 1)
                    {
                        changeCostClass.SetEffectName($"Play Cost -{count()}");

                        return true;
                    }
                }

                return false;
            }



            int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
            {
                if (CardSourceCondition(cardSource))
                {
                    if (RootCondition(root))
                    {
                        if (PermanentsCondition(targetPermanents))
                        {
                            Cost -= count();
                        }
                    }
                }

                return Cost;
            }

            bool PermanentsCondition(List<Permanent> targetPermanents)
            {
                return true;
            }

            bool CardSourceCondition(CardSource cardSource)
            {
                return cardSource == card;
            }

            bool RootCondition(SelectCardEffect.Root root)
            {
                return true;
            }

            bool isUpDown()
            {
                return true;
            }
        }

        if (timing == EffectTiming.OptionSkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] Your opponent can't play Digimon by effects until the end of their turn. Delete all of your opponent's Digimon with 6000 DP or less.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(6000, activateClass))
                    {
                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            if (permanent.CanBeDestroyedBySkill(activateClass))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                CanNotPutFieldClass canNotPutFieldClass = new CanNotPutFieldClass();
                canNotPutFieldClass.SetUpICardEffect("Can't play Digimon by effect", CanUseCondition1, card);
                canNotPutFieldClass.SetUpCanNotPutFieldClass(cardCondition: CardCondition, cardEffectCondition: CardEffectCondition);
                card.Owner.Enemy.UntilOwnerTurnEndEffects.Add((_timing) => canNotPutFieldClass);

                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().DebuffSE);

                bool CanUseCondition1(Hashtable hashtable)
                {
                    return true;
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon || cardSource.IsDigiEgg;
                }

                bool CardEffectCondition(ICardEffect cardEffect)
                {
                    return cardEffect != null &&
                           cardEffect.EffectSourceCard.Owner == card.Owner.Enemy;
                }

                List<Permanent> destroyTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(CanSelectPermanentCondition);
                yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(destroyTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects, effectName: $"Delete Digimons with 6000 DP or less and opponent can't play Digimon by effect");
        }

        return cardEffects;
    }
}
