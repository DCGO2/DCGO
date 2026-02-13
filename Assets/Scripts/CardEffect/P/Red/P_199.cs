using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Dan Yuki
namespace DCGO.CardEffects.P
{
    public class P_199 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start Of Your Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("+3K DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] If you have 4 or less memory, 1 of your Digimon gets +3000 DP for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition)
                        && card.Owner.MemoryForPlayer <= 4;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        Permanent selectedPermanent = null;
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

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will gain 3k DP.", "The opponent is selecting 1 Digimon that will gain 3k DP.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            targetPermanent: selectedPermanent,
                            changeValue: 3000,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass));
                    }
                }
            }

            #endregion

            #region Your Turn

            #region Reduce Cost Effect

            ActivateClass activateClass2 = new ActivateClass();
            activateClass2.SetUpICardEffect("Reduce play cost", CanUseCondition2, card);
            activateClass2.SetUpActivateClass(CanActivateCondition2, ActivateCoroutine2, -1, true, EffectDiscription2());

            string EffectDiscription2()
            {
                return "[Your Turn] When any of your Digimon cards with the [TS] trait would be played, by suspending this Tamer, reduce their play costs by 1.";
            }

            bool CardCondition(CardSource cardSource)
            {
                if (cardSource.Owner == card.Owner)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.HasTSTraits)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition2(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.CanActivateSuspendCostEffect(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            bool CanActivateCondition2(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    PlayCardClass playCardClass = CardEffectCommons.GetPlayCardClassFromHashtable(hashtable);

                    if (playCardClass != null)
                    {
                        if (playCardClass.PayCost)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine2(Hashtable _hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(
                        permanents: new List<Permanent> { card.PermanentOfThisCard() },
                        hashtable: _hashtable).Tap());

                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect($"Play Cost -1", CanUseCondition1, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                    card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                    yield return new WaitForSeconds(0.4f);
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return true;
                    }

                    int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                    {
                        if (CardSourceCondition(cardSource))
                        {
                            if (RootCondition(root))
                            {
                                if (PermanentsCondition(targetPermanents))
                                {
                                    Cost -= 1;
                                }
                            }
                        }

                        return Cost;
                    }

                    bool PermanentsCondition(List<Permanent> targetPermanents)
                    {
                        if (targetPermanents == null)
                        {
                            return true;
                        }
                        else
                        {
                            if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    bool CardSourceCondition(CardSource cardSource)
                    {
                        CardSource Card = CardEffectCommons.GetCardFromHashtable(_hashtable);

                        if (Card != null)
                        {
                            if (cardSource == Card)
                            {
                                return true;
                            }
                        }

                        return false;
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
            }

            #endregion

            #region Before Pay Cost

            if (timing == EffectTiming.BeforePayCost)
            {
                cardEffects.Add(activateClass2);
            }

            #endregion

            #region Before Pay Cost (Not Shown)

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -1", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => true, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (!card.Owner.isYou && GManager.instance.IsAI)
                    {
                        return false;
                    }

                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanActivateSuspendCostEffect(card))
                            {
                                if (activateClass2 != null)
                                {
                                    if (!card.cEntity_EffectController.isOverMaxCountPerTurn(activateClass2, activateClass2.MaxCountPerTurn))
                                    {
                                        return true;
                                    }
                                }
                            }
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
                                Cost -= 1;
                            }
                        }
                    }

                    return Cost;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    if (targetPermanents == null)
                    {
                        return true;
                    }
                    else
                    {
                        if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.HasTSTraits)
                        {
                            return true;
                        }
                    }

                    return false;
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

            #endregion

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}