using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

//Chaosmon
namespace DCGO.CardEffects.P
{
    public class P_221 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

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
                        bool PermanentCondition1(Permanent permanent) => permanent.TopCard.CardColors.Contains(CardColor.Yellow) && permanent.Levels_ForJogress(card).Contains(6);

                        bool PermanentCondition2(Permanent permanent) => (permanent.TopCard.CardColors.Contains(CardColor.Purple) || permanent.TopCard.CardColors.Contains(CardColor.Black)) && permanent.Levels_ForJogress(card).Contains(6);

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "a level 6 yellow Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 6 purple/black Digimon"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Partition
            if (timing == EffectTiming.WhenRemoveField)
            {
                List<PartitionCondition> partitionConditions = new List<PartitionCondition>();
                partitionConditions.Add(new PartitionCondition(6, CardColor.Yellow));
                partitionConditions.Add(new PartitionCondition(6, CardColor.Purple, CardColor.Black));

                cardEffects.Add(CardEffectFactory.PartitionSelfEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null,
                    cardSourceConditions: partitionConditions));
            }
            #endregion

            #region Security Attack +1
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region When Digivolving 1

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Become immune.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If DNA digivolving, until your opponent's turn ends, their effects don't affect this Digimon";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsJogress(hashtable)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    if (selectedPermanent != null)
                    {
                        CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                        canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's effects", CanUseCondition1, card);
                        canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                        selectedPermanent.UntilOpponentTurnEndEffects.Add((_timing) => canNotAffectedClass);

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            return CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent);
                        }

                        bool CardCondition(CardSource cardSource)
                        {
                            return CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent)
                                && cardSource == selectedPermanent.TopCard;
                        }

                        bool SkillCondition(ICardEffect cardEffect)
                        {
                            return cardEffect != null
                                && cardEffect.EffectSourceCard != null
                                && cardEffect.EffectSourceCard.Owner == card.Owner.Enemy;
                        }
                    }

                    yield return null;
                }
            }

            #endregion

            #region OP/WD Shared

            string EffectNameShared()
               => "Give 1 opponent's Digimon -10000 DP";

            string EffectDescriptionShared(string tag)
            {
                return $"[{tag}] 1 of your opponent's Digimon gets -10000 DP until their turn ends";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanActivateConditionShared(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            IEnumerator ActivateCoroutineShared(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage(customMessageArray: CardEffectCommons.customPermanentMessageArray_ChangeDP(changeValue: -10000, maxCount: 1));

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            targetPermanent: permanent,
                            changeValue: -10000,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));
                    }
                }
            }

            #endregion

            #region When Digivolving 2

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(EffectNameShared(), CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, hash => ActivateCoroutineShared(hash, activateClass), -1, false, EffectDescriptionShared("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(EffectNameShared(), CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, hash => ActivateCoroutineShared(hash, activateClass), -1, false, EffectDescriptionShared("When Attacking"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
