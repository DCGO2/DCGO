using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT5_110 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OptionSkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] You may return 1 of your Digimon with [Omnimon] in its name to its owner's hand to delete all Digimon and Tamers. Trash all of the digivolution cards of the Digimon you returned with this effect.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    if (permanent.TopCard.ContainsCardName("Omnimon"))
                    {
                        return true;
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
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    Permanent bouncePermanent = null;

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to return to hand.", "The opponent is selecting 1 Digimon to return to hand.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        bouncePermanent = permanent;

                        yield return null;
                    }

                    if (bouncePermanent != null)
                    {
                        if (bouncePermanent.TopCard != null)
                        {
                            if (!bouncePermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                if (!bouncePermanent.CannotReturnToHand(activateClass))
                                {
                                    Hashtable hashtable = new Hashtable();
                                    hashtable.Add("CardEffect", activateClass);

                                    CardSource bounceCard = bouncePermanent.TopCard;

                                    yield return ContinuousController.instance.StartCoroutine(new HandBounceClaass(new List<Permanent>() { bouncePermanent }, hashtable).Bounce());

                                    if (bouncePermanent.TopCard == null && bouncePermanent.HandBounceEffect == activateClass)
                                    {
                                        //if (bounceCard.Owner.HandCards.Contains(bounceCard) || bounceCard.isToken)
                                        {
                                            List<Permanent> destroyedPermanetns = new List<Permanent>();

                                            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                                            {
                                                foreach (Permanent permanent in player.Enemy.GetBattleAreaPermanents())
                                                {
                                                    if (permanent.IsDigimon || permanent.IsTamer)
                                                    {
                                                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                                        {
                                                            if (permanent.CanBeDestroyedBySkill(activateClass))
                                                            {
                                                                destroyedPermanetns.Add(permanent);
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            if (destroyedPermanetns.Count >= 1)
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(destroyedPermanetns, hashtable).Destroy());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect($"Add this card to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] Add this card to its owner's hand.";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
            }
        }

        return cardEffects;
    }
}
