using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

//Wargreymon ACE
namespace DCGO.CardEffects.EX10
{
    public class EX10_010 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Blast Digivolve

            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }

            #endregion

            #region Static Effects

            //Raid
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }

            //Reboot/Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region On Play/When Digivolving Shared

            string EffectDescription(string tag)
            {
                return $"[{tag}] Delete 1 of your opponent's play cost 7 or lower Digimon or Tamers.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) || CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaTamer(permanent, card))
                       && permanent.TopCard.HasPlayCost && permanent.TopCard.GetCostItself <= 7;
            }

            bool CanActivateSharedCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card) &&
                       CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
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
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 Digimon/Tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, (hashTable) => SharedActivateCoroutine(hashTable, activateClass), -1, false, EffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 Digimon/Tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, (hashTable) => SharedActivateCoroutine(hashTable, activateClass), -1, false, EffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }

            #endregion

            #region All Turns

            #region All Turns - Immunity

            if (timing == EffectTiming.None)
            {
                CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's Digimon's effects", CanUseCondition, card);
                canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                cardEffects.Add(canNotAffectedClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(OpponentsPermanent);
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

                bool OpponentsPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           card.PermanentOfThisCard().Boosts.Exists(x => x.ID == "AT_EX10-010");
                }
            }

            #endregion

            #region All Turns - DP

            if (timing == EffectTiming.OnRemovedField || timing == EffectTiming.OnEnterFieldAnyone || timing == EffectTiming.WhenTopCardTrashed)
            {
                if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();

                    bool OpponentsPermanent(Permanent permanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                               permanent.DP >= 13000;
                    }

                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(OpponentsPermanent))
                            thisPermanent.AddBoost(new Permanent.DPBoost("AT_EX10-010", 3000, null));
                        else
                        {
                            if (thisPermanent.Boosts.Exists(x => x.ID == "AT_EX10-010"))
                                thisPermanent.RemoveBoost("AT_EX10-010");
                        }
                    }
                    else
                    {
                        if (thisPermanent.Boosts.Exists(x => x.ID == "AT_EX10-010"))
                            thisPermanent.RemoveBoost("AT_EX10-010");
                    }
                }
            }

            #endregion

            #endregion

            return cardEffects;
        }
    }
}