using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class P_066 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Delete 1 Digimon with 4000 DP or less or Draw 1, and add this card to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] At the end of the battle, delete 1 of your opponent's Digimon with 4000 DP or less. If no Digimon was deleted by this effect, <Draw 1>. (Draw 1 card from your deck.) Then, add this card to its owner's hand.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnExecutingArea(card))
                {
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return null;

                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                ActivateClass activateClass1 = new ActivateClass();
                activateClass1.SetUpICardEffect("Delete 1 Digimon with 4000 DP or less or Draw 1, and add this card to hand", CanUseCondition1, card);
                activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                card.Owner.UntilEndBattleEffects.Add(GetCardEffect1);

                string EffectDiscription1()
                {
                    return "Delete 1 of your opponent's Digimon with 4000 DP or less. If no Digimon was deleted by this effect, <Draw 1>. (Draw 1 card from your deck.) Then, add this card to its owner's hand.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(4000, activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition1(Hashtable hashtable)
                {
                    return true;
                }

                bool CanActivateCondition1(Hashtable hashtable)
                {
                    return true;
                }

                IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                {
                    List<Permanent> deleteTargetPermanents = new List<Permanent>();

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            foreach (Permanent permanent in permanents)
                            {
                                deleteTargetPermanents.Add(permanent);
                            }

                            yield return null;
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: deleteTargetPermanents, activateClass: activateClass, successProcess: null, failureProcess: FailureProcess));

                    IEnumerator FailureProcess()
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    }

                    if (card.Owner.ExecutingCards.Contains(card))
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.brainStormObject.CloseBrainstrorm(card));

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { card }, false, activateClass));
                    }
                }

                ICardEffect GetCardEffect1(EffectTiming _timing)
                {
                    if (_timing == EffectTiming.OnEndBattle)
                    {
                        return activateClass1;
                    }

                    return null;
                }
            }
        }

        return cardEffects;
    }
}
