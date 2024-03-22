using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class HerculesKabuterimon_BT15_053 : CEntity_Effect
{
  public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
  {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        #region Start of Your Main Phase/ When Digivolving
        if (timing == EffectTiming.OnStartMainPhase || timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Suspend 1 Digimon and your 1 Digimon gains Pierce", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Start of Your Main Phase] [When Digivolving] Suspend 1 of your opponent's Digimon. 1 of your Digimon gains <Piercing> for the turn.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanSelectPermanentCondition1(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        return true;
                    }
                }

                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        return true;
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
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
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition1));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition1,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 Digimon that will gain Pierce.",
                        "The opponent is selecting 1 Digimon that will gain Pierce.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainPierce(
                            targetPermanent: permanent,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass));
                    }
                }
            }
        }
        #endregion

        #region All Turns
        if (timing == EffectTiming.None)
        {
            CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
            canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's Digimon's effects", CanUseCondition, card);
            canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
            cardEffects.Add(canNotAffectedClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.PermanentOfThisCard().IsSuspended)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CardCondition(CardSource cardSource)
            {
                if (cardSource == card)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (cardSource == card.PermanentOfThisCard().TopCard)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool SkillCondition(ICardEffect cardEffect)
            {
                if (CardEffectCommons.IsOpponentEffect(cardEffect, card))
                {
                    if (cardEffect.IsDigimonEffect)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        #endregion

        return cardEffects;
  }
}
