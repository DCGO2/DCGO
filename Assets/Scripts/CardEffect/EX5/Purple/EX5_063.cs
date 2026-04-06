using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Leviamon
public class EX5_063 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        #region Security A. +1
        if (timing == EffectTiming.None)
        {
            cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(
                changeValue: 1,
                isInheritedEffect: false,
                card: card,
                condition: null));
        }
        #endregion

        #region On Play
        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Delete 1 Digimon with the highest level and 1 Digimon with the lowest level", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] If your opponent has as many or more total Digimon and Tamers as you, delete 1 of your opponent's Digimon with the highest level. Then, delete 1 of your opponent's Digimon with the lowest level.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (CardEffectCommons.IsMaxLevel(permanent, card.Owner.Enemy))
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
                    if (CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy))
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
                    if (card.Owner.Enemy.GetBattleAreaPermanents().Count(permanent => permanent.IsDigimon || permanent.IsTamer)
                    >= card.Owner.GetBattleAreaPermanents().Count(permanent => permanent.IsDigimon || permanent.IsTamer))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
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
                if (card.Owner.Enemy.GetBattleAreaPermanents().Count(permanent => permanent.IsDigimon || permanent.IsTamer)
                    >= card.Owner.GetBattleAreaPermanents().Count(permanent => permanent.IsDigimon || permanent.IsTamer))
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
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
        }
        #endregion

        #region When Digivolving
        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Delete 1 Digimon with the highest level and 1 Digimon with the lowest level", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] If your opponent has as many or more total Digimon and Tamers as you, delete 1 of your opponent's Digimon with the highest level. Then, delete 1 of your opponent's Digimon with the lowest level.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (CardEffectCommons.IsMaxLevel(permanent, card.Owner.Enemy))
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
                    if (CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.Enemy.GetBattleAreaPermanents().Count(permanent => permanent.IsDigimon || permanent.IsTamer)
                    >= card.Owner.GetBattleAreaPermanents().Count(permanent => permanent.IsDigimon || permanent.IsTamer))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
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
                if (card.Owner.Enemy.GetBattleAreaPermanents().Count(permanent => permanent.IsDigimon || permanent.IsTamer)
                    >= card.Owner.GetBattleAreaPermanents().Count(permanent => permanent.IsDigimon || permanent.IsTamer))
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
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
        }
        #endregion

        #region All Turns
        if (timing == EffectTiming.OnDestroyedAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Gain Memory", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[All Turns] When an opponent's Digimon is deleted, gain 1 memory for each Digimon.";
            }

            bool PermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card)
                    && CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.CanAddMemory(activateClass))
                    {
                        List<Hashtable> hashtables = CardEffectCommons.GetHashtablesFromHashtable(hashtable);

                        if (hashtables != null)
                        {
                            if (hashtables.Count >= 1)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable hashtable)
            {
                List<CardSource> EnemyDigimon = new List<CardSource>();
                List<Hashtable> hashtables = CardEffectCommons.GetHashtablesFromHashtable(hashtable);

                if (hashtables != null)
                {
                    foreach (Hashtable hashtable1 in hashtables)
                    {
                        CardSource cardSource = CardEffectCommons.GetTopCardFromOneHashtable(hashtable1);
                        if (cardSource != null
                         && cardSource.IsDigimon
                         && cardSource.Owner == card.Owner.Enemy)
                        {
                            EnemyDigimon.Add(cardSource);
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(EnemyDigimon.Count, activateClass));
                }
            }
        }
        #endregion

        return cardEffects;
    }
}
