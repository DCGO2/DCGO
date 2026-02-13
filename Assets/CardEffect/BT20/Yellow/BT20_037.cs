using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace DCGO.CardEffects.BT20
{
    public class BT20_037 : CEntity_Effect
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

                        bool PermanentCondition2(Permanent permanent) => (permanent.TopCard.CardColors.Contains(CardColor.Green) || permanent.TopCard.CardColors.Contains(CardColor.Black)) && permanent.Levels_ForJogress(card).Contains(6);

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "a level 6 yellow Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 6 green/black Digimon"),
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
                partitionConditions.Add(new PartitionCondition(6, CardColor.Green, CardColor.Black));

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

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("For each level 6 in sources, suspend 1 Digimon and memory +1, Then Opponent's Digimon/Tamers can't activate [On Play] or unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] For each of this Digimon's level 6 digivolution cards, suspend 1 of your opponent's Digimon or Tamers and gain 1 memory. Then, none of their Digimon or Tamers can activate [On Play] effects or unsuspend until the end of their turn.";
                }

                bool IsLevel6(CardSource source)
                {
                    return source.HasLevel && source.IsLevel6 && !source.IsFlipped;
                }

                bool OpponentsDigimonOrTamer(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card) &&
                           (permanent.IsDigimon || permanent.IsTamer);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if(card.PermanentOfThisCard().DigivolutionCards.Count(IsLevel6) > 0)
                    {
                        int maxCount = Mathf.Min(CardEffectCommons.MatchConditionPermanentCount(OpponentsDigimonOrTamer), card.PermanentOfThisCard().DigivolutionCards.Count(IsLevel6));
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentsDigimonOrTamer,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage($"Select {maxCount} Digimon/Tamers to suspend.", $"The opponent is selecting {maxCount} Digimon/Tamers to suspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (card.Owner.CanAddMemory(activateClass))
                            yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(card.PermanentOfThisCard().DigivolutionCards.Count(IsLevel6), activateClass));
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotUnsuspendPlayerEffect(
                        permanentCondition: OpponentsDigimonOrTamer,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass,
                        isOnlyActivePhase: false,
                        effectName: "Your card can't unsuspend"));

                    DisableEffectClass invalidationClass = new DisableEffectClass();
                    invalidationClass.SetUpICardEffect("Ignore [On Play] Effect of opponent's Digimon/Tamers", CanUseCondition, card);
                    invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                    card.Owner.UntilOpponentTurnEndEffects.Add((_timing) => invalidationClass);

                    bool CanUseCondition(Hashtable hashtable)
                    {
                        return true;
                    }

                    bool InvalidateCondition(ICardEffect cardEffect)
                    {
                        if (cardEffect != null)
                        {
                            if (cardEffect is ActivateICardEffect)
                            {
                                if (cardEffect.EffectSourceCard != null)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(cardEffect.EffectSourceCard.PermanentOfThisCard(), card))
                                    {
                                        if (cardEffect.EffectSourceCard.PermanentOfThisCard().IsTamer || cardEffect.EffectSourceCard.PermanentOfThisCard().IsDigimon)
                                        {
                                            if (!cardEffect.EffectSourceCard.PermanentOfThisCard().TopCard.CanNotBeAffected(invalidationClass))
                                            {
                                                if (cardEffect.IsOnPlay)
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
                }
            }
            #endregion

            return cardEffects;
        }
    }
}