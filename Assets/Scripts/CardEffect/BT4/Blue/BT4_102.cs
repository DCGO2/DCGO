using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT4_102 : CEntity_Effect
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
                return "[Main] Return 1 of your Digimon to its owner's hand to return up to 2 of your opponent's level 4 or lower Digimon to their owners' hands. Trash all of the digivolution cards of those Digimon.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
            }

            bool CanSelectPermanentCondition1(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.Level <= 4)
                    {
                        if (permanent.TopCard.HasLevel)
                        {
                            return true;
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
                                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                                            {
                                                maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition1));

                                                selectPermanentEffect.SetUp(
                                                    selectPlayer: card.Owner,
                                                    canTargetCondition: CanSelectPermanentCondition1,
                                                    canTargetCondition_ByPreSelecetedList: null,
                                                    canEndSelectCondition: CanEndSelectCondition,
                                                    maxCount: maxCount,
                                                    canNoSelect: false,
                                                    canEndNotMax: true,
                                                    selectPermanentCoroutine: null,
                                                    afterSelectPermanentCoroutine: null,
                                                    mode: SelectPermanentEffect.Mode.Bounce,
                                                    cardEffect: activateClass);

                                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                                bool CanEndSelectCondition(List<Permanent> permanents)
                                                {
                                                    if (permanents.Count <= 0)
                                                    {
                                                        return false;
                                                    }

                                                    return true;
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
