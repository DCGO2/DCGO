using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT4_031 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

         if (timing == EffectTiming.None)
        {
            ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
            changeCardNamesClass.SetUpICardEffect("Also treated as having [Kimeramon] in its name", CanUseCondition, card);
            changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: changeCardNames);

            cardEffects.Add(changeCardNamesClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                return true;
            }

            List<string> changeCardNames(CardSource cardSource, List<string> CardNames)
            {
                if (cardSource == card)
                {
                    CardNames.Add("Kimeramon_BT4_031");
                }

                return CardNames;
            }
        }
        
        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Return Digimons to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] You may return 1 of your other Digimon to its owner's hand to return 1 of your opponent's Digimon with no digivolution cards to its owner's hand. Trash all of the digivolution cards of your Digimon.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    if (permanent != card.PermanentOfThisCard())
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanSelectPermanentCondition1(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.HasNoDigivolutionCards)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    Permanent bounceTargetPermanent = null;

                    int maxCount = Math.Min(1, card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

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

                    selectPermanentEffect.SetUpCustomMessage("Select 1 your Digimon to return to hand.", "The opponent is selecting 1 your Digimon to return to hand.");

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
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                            {
                                maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition1));

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentCondition1,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Bounce,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                            }
                        }
                    }
                }
            }
        }

        return cardEffects;
    }
}
