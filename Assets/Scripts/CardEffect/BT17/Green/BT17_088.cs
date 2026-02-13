using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT17
{
    public class BT17_088 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start Of main Phase/ On Play
            bool CanSelectYourGreenDigimon(Permanent permanent)
            {
                if(CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    return permanent.TopCard.CardColors.Contains(CardColor.Green);
                
                return false;
            }
            #endregion

            #region Start of You Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("+2000 DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] 1 of your green Digimon gets +2000 DP until the end of your opponent's turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return isExistOnField(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if(CardEffectCommons.HasMatchConditionPermanent(CanSelectYourGreenDigimon))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectYourGreenDigimon));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectYourGreenDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP +2000.", "The opponent is selecting 1 Digimon that will get DP +2000.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: 2000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                        }
                    }
                }
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("+2000 DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] 1 of your green Digimon gets +2000 DP until the end of your opponent's turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPlay(hashtable, card))
                            return true;
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return isExistOnField(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectYourGreenDigimon))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectYourGreenDigimon));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectYourGreenDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP +2000.", "The opponent is selecting 1 Digimon that will get DP +2000.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: 2000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                        }
                    }
                }
            }
            #endregion

            #region Your Turn - When Digimon Played
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                Permanent selectedPermanent = null;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve for reduced cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When one your Digimon with [Terriermon] or [Lopmon] in its name is played, by suspending this Tamer, 1 of your Digimon with [Terriermon] or [Lopmon] in its name may digivolve into a Digimon card in the hand with the digivolution cost reduced by 2.";
                }

                bool PlayedPermanentCondition(Permanent permanent)
                {

                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.ContainsCardName("Terriermon") || permanent.TopCard.ContainsCardName("Lopmon"))
                        {
                                return true;
                        }
                    }

                    return false;
                }

                bool DigivolvePermanentCondition(CardSource source)
                {
                    if (source.Owner == card.Owner)
                    {
                        if (source.IsDigimon)
                        {
                            if (source.CanPlayCardTargetFrame(selectedPermanent.PermanentFrame, true, activateClass))
                            {
                                return true;
                            }
                        }
                    }
                        

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PlayedPermanentCondition))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (CardEffectCommons.CanActivateSuspendCostEffect(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                { 
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    if(CardEffectCommons.HasMatchConditionOwnersPermanent(card, PlayedPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(PlayedPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: PlayedPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    IEnumerator SelectPermanentCoroutine (Permanent permanent)
                    {
                        if(permanent != null)
                            selectedPermanent = permanent;

                        yield return null;
                    }

                    if(selectedPermanent != null)
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, DigivolvePermanentCondition))
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                        targetPermanent: selectedPermanent,
                                        cardCondition: DigivolvePermanentCondition,
                                        payCost: true,
                                        reduceCostTuple: (reduceCost: 2, reduceCostCardCondition: null),
                                        fixedCostTuple: null,
                                        ignoreDigivolutionRequirementFixedCost: -1,
                                        isHand: true,
                                        activateClass: activateClass,
                                        successProcess: null));

                        }
                    }
                }
            }
            #endregion

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}