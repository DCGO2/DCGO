using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT1_112 : CEntity_Effect
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
                return "[Main] 1 of your Digimon gains the following effect for the turn: \"When this Digimon deletes an opponent's Digimon in battle and survivesÅCunsuspend it.\"";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
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
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get effects.", "The opponent is selecting 1 Digimon that will get effects.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    Permanent selectedPermanent = permanent;

                    if (selectedPermanent != null)
                    {
                        ActivateClass activateClass1 = new ActivateClass();
                        activateClass1.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition1, selectedPermanent.TopCard);
                        activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                        activateClass1.SetEffectSourcePermanent(selectedPermanent);
                        CardEffectCommons.AddEffectToPermanent(targetPermanent: selectedPermanent, effectDuration: EffectDuration.UntilEachTurnEnd, card: card, cardEffect: activateClass1, timing: EffectTiming.OnEndBattle);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(selectedPermanent));

                        string EffectDiscription1()
                        {
                            return "When this Digimon deletes an opponent's Digimon in battle and survivesÅCnsuspend it.";
                        }

                        bool CanUseCondition1(Hashtable hashtable1)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                            {
                                bool WinnerCondition(Permanent permanent) => permanent.cardSources.Contains(selectedPermanent.TopCard);
                                bool LoserCondition(Permanent permanent) => CardEffectCommons.IsOpponentPermanent(permanent, card);

                                if (CardEffectCommons.CanTriggerWhenDeleteOpponentDigimonByBattle(hashtable: hashtable1, winnerCondition: WinnerCondition, loserCondition: LoserCondition, isOnlyWinnerSurvive: true))
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        bool CanActivateCondition1(Hashtable hashtable)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                            {
                                if (CardEffectCommons.CanUnsuspend(selectedPermanent))
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass1).Unsuspend());
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
