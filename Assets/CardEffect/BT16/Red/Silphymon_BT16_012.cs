using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.BT16
{
    public class Silphymon_BT16_012 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Partition
            if (timing == EffectTiming.WhenRemoveField)
            {
                bool CanSelectFirstSourceCondition(CardSource card)
                {
                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardColors.Contains(CardColor.Red) && cardSource.Level == 4) >= 1)
                    {
                        return true;
                    }

                    return false;
                }

                bool CanSelectSecondSourceCondition(CardSource card)
                {
                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardColors.Contains(CardColor.Yellow) && cardSource.Level == 4) >= 1)
                    {
                        return true;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.PartitionSelfEffect
                    (isInheritedEffect: false,
                    card: card,
                    condition: null,
                    canSelectFirstSourceCondition: CanSelectFirstSourceCondition, 
                    canSelectSecondSourceCondition: CanSelectSecondSourceCondition));
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.WhenRemoveField)
            {
                bool CanSelectFirstSourceCondition(CardSource card)
                {
                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardColors.Contains(CardColor.Red) && cardSource.Level == 4) >= 1)
                    {
                        return true;
                    }

                    return false;
                }

                bool CanSelectSecondSourceCondition(CardSource card)
                {
                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardColors.Contains(CardColor.Yellow) && cardSource.Level == 4) >= 1)
                    {
                        return true;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.PartitionSelfEffect
                    (isInheritedEffect: true,
                    card: card,
                    condition: null,
                    canSelectFirstSourceCondition: CanSelectFirstSourceCondition,
                    canSelectSecondSourceCondition: CanSelectSecondSourceCondition));
            }
            #endregion

            #region DNA
            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect($"DNA Digivolution", CanUseCondition, card);
                addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
                addJogressConditionClass.SetNotShowUI(true);
                cardEffects.Add(addJogressConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }



                JogressCondition GetJogress(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        bool PermanentCondition1(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.IsDigimon)
                                        {
                                            if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                            {
                                                if (permanent.TopCard.CardColors.Contains(CardColor.Red))
                                                {
                                                    if (permanent.Levels_ForJogress(card).Contains(4))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        bool PermanentCondition2(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.IsDigimon)
                                        {
                                            if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                            {
                                                if (permanent.TopCard.CardColors.Contains(CardColor.Yellow))
                                                {
                                                    if (permanent.Levels_ForJogress(card).Contains(4))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "a level 4 red Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 4 yellow Digimon"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Give 1 of your opponent's Digimon -7000 DP.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetHashString("Minus7000DP_BT16_012");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If DNA digivolving, 1 of your opponent's Digimon gets -7000 DP until the end of your opponent's turn.";
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
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {

                    bool isJogress = false;

                    if (_hashtable != null)
                    {
                        if (_hashtable.ContainsKey("isJogress"))
                        {
                            if (_hashtable["isJogress"] is bool)
                            {
                                isJogress = (bool)_hashtable["isJogress"];
                            }
                        }
                    }

                    if(isJogress)
                    {
                        if (isExistOnField(card))
                        {
                            if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
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

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -7000.", "The opponent is selecting 1 Digimon that will get DP -5000.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: -5000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region When Digivolving | When Attacking

            if (timing == EffectTiming.OnEnterFieldAnyone || timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 of your opponent's Digimon with 4000 DP or less.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("Delete4000DPorLess_BT16_012");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] [When Attacking] Delete 1 of your opponent's Digimon with 4000 DP or less.";
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

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card) || CardEffectCommons.CanTriggerOnAttack(hashtable,card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
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
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}