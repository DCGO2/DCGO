using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.EX6
{
    public class Witchmon_EX6_048 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 card in your hand to give effects.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By trashing 1 card in your hand, until the end of your opponent's turn, 1 of your opponent's Digimon gains [End of Attack] Delete this Digimon.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }                

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.HandCards.Count >= 1)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Count >= 1)
                    {
                        bool discarded = false;

                        int discardCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: (cardSource) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: discardCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                discarded = true;

                                yield return null;
                            }
                        }

                        if (discarded)
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
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage(
                                    "Select 1 Digimon that will get [End of Attack] Delete this Digimon.",
                                    "The opponent is selecting 1 Digimon that will get [End of Attack] Delete this Digimon.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    Permanent selectedPermanent = permanent;

                                    if (selectedPermanent != null)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                                    }

                                    if (selectedPermanent != null)
                                    {
                                        ActivateClass activateClass1 = new ActivateClass();
                                        activateClass1.SetUpICardEffect("Delete this Digimon", CanUseCondition1, selectedPermanent.TopCard);
                                        activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                                        activateClass1.SetEffectSourcePermanent(selectedPermanent);
                                        selectedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                                        }

                                        string EffectDiscription1()
                                        {
                                            return "[End of Attack] Delete this Digimon.";
                                        }

                                        bool CanUseCondition1(Hashtable hashtable1)
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                            {
                                                if (CardEffectCommons.CanTriggerOnAttack(hashtable, selectedPermanent.TopCard))
                                                {
                                                    if (selectedPermanent.TopCard.Owner.GetBattleAreaDigimons().Contains(selectedPermanent))
                                                    {
                                                        if (GManager.instance.turnStateMachine.gameContext.TurnPlayer == selectedPermanent.TopCard.Owner)
                                                        {
                                                            return true;
                                                        }
                                                    }
                                                }
                                            }

                                            return false;
                                        }

                                        bool CanActivateCondition1(Hashtable hashtable1)
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                            {
                                                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                                {
                                                    return true;
                                                }
                                            }

                                            return false;
                                        }

                                        IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                                                new List<Permanent>() { selectedPermanent },
                                                CardEffectCommons.CardEffectHashtable(activateClass1)).Destroy());
                                            }
                                        }

                                        ICardEffect GetCardEffect(EffectTiming _timing)
                                        {
                                            if (_timing == EffectTiming.OnEndAttack)
                                            {
                                                return activateClass1;
                                            }

                                            return null;
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 card in your hand to give effects.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By trashing 1 card in your hand, until the end of your opponent's turn, 1 of your opponent's Digimon gains [End of Attack] Delete this Digimon.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.HandCards.Count >= 1)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Count >= 1)
                    {
                        bool discarded = false;

                        int discardCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: (cardSource) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: discardCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                discarded = true;

                                yield return null;
                            }
                        }

                        if (discarded)
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
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage(
                                    "Select 1 Digimon that will get [End of Attack] Delete this Digimon.",
                                    "The opponent is selecting 1 Digimon that will get [End of Attack] Delete this Digimon.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    Permanent selectedPermanent = permanent;

                                    if (selectedPermanent != null)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                                    }

                                    if (selectedPermanent != null)
                                    {
                                        ActivateClass activateClass1 = new ActivateClass();
                                        activateClass1.SetUpICardEffect("Delete this Digimon", CanUseCondition1, selectedPermanent.TopCard);
                                        activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                                        activateClass1.SetEffectSourcePermanent(selectedPermanent);
                                        selectedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                                        }

                                        string EffectDiscription1()
                                        {
                                            return "[End of Attack] Delete this Digimon.";
                                        }

                                        bool CanUseCondition1(Hashtable hashtable1)
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                            {
                                                if (CardEffectCommons.CanTriggerOnAttack(hashtable, selectedPermanent.TopCard))
                                                {
                                                    if (selectedPermanent.TopCard.Owner.GetBattleAreaDigimons().Contains(selectedPermanent))
                                                    {
                                                        if (GManager.instance.turnStateMachine.gameContext.TurnPlayer == selectedPermanent.TopCard.Owner)
                                                        {
                                                            return true;
                                                        }
                                                    }
                                                }
                                            }

                                            return false;
                                        }

                                        bool CanActivateCondition1(Hashtable hashtable1)
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                            {
                                                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                                {
                                                    return true;
                                                }
                                            }

                                            return false;
                                        }

                                        IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                                                new List<Permanent>() { selectedPermanent },
                                                CardEffectCommons.CardEffectHashtable(activateClass1)).Destroy());
                                            }
                                        }

                                        ICardEffect GetCardEffect(EffectTiming _timing)
                                        {
                                            if (_timing == EffectTiming.OnEndAttack)
                                            {
                                                return activateClass1;
                                            }

                                            return null;
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.OnCounterTiming)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("End the attack by deleting 1 of your Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("StopAttack_EX6-048");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Opponent's Turn][Once Per Turn] When an opponent's Digimon attacks, by deleting 1 of your other Digimon, end the attack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, permanent => true))
                        {
                            if (CardEffectCommons.IsOpponentTurn(card))
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
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanentCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
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

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        int maxCount = 1;

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

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { permanent }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                            IEnumerator SuccessProcess()
                            {
                                GManager.instance.attackProcess.IsEndAttack = true;

                                yield return null;
                            }
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}