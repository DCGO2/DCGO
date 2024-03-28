using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class Sora_Takenouchi_BT15_082 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnStartTurn)
        {
            cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
        }

        if(timing == EffectTiming.WhenReturntoHandAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Return this Tamer to your hand to play a Digimon from your hand.", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[All Turns] When a red Digimon card returns from your trash to the hand, by returning this Tamer to the hand, you may play 1 13000 DP or less red Digimon card with [Avian], [Bird], [Beast], [Animal] or [Sovereign], other than [Sea Animal] in one of its traits from your hand without paying the cost. For each of your opponent's security cards, remove 2000 from this effect's playable card's DP maximum.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return permanent.TopCard == card;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (hashtable.ContainsKey("Permanents"))
                    {
                        List<Permanent> Permanents = (List<Permanent>)hashtable["Permanents"];

                        if (Permanents != null)
                        {
                            if (Permanents.Count((permanent) => permanent.TopCard.CardColors.Contains(CardColor.Red)) >= 1)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
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
                Permanent bounceTargetPermanent = null;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 tamer to return to hand.", "The opponent is selecting 1 tamer to return to hand.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    bounceTargetPermanent = permanent;

                    yield return null;
                }

                if (bounceTargetPermanent != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.BouncePeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { bounceTargetPermanent },
                        activateClass: activateClass,
                        successProcess: SuccessProcess(),
                        failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        Debug.Log("Successful Return: Select and Play out digimon");
                        yield return null;
                    }
                }
            }
        }
        
        if(timing == EffectTiming.SecuritySkill)
        {
            cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
        }
        return cardEffects;    
    }
}
