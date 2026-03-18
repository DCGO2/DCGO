using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Gallantmon
namespace DCGO.CardEffects.AD1
{
    public class AD1_008 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("WarGrowlmon")
                        || targetPermanent.TopCard.EqualsTraits("Hero");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    level: 5,
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }
            #endregion

            #region Rush
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RushSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Raid
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete opponent's Digimon whose total DP adds up to 10000 or less, Then may Attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() => "[When Digivolving] Delete up to 10000DP total worth of your opponent's Digimon. Then, this Digimon may attack.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.HasDP
                        && permanent.DP <= MaxDPForDeletion();
                }

                int MaxDPForDeletion() => card.Owner.MaxDP_DeleteEffect(10000, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(card.Owner.Enemy.GetBattleAreaDigimons().Count(CanSelectPermanentCondition),
                            CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: CanTargetConditionByPreSelectedList,
                            canEndSelectCondition: CanEndSelectCondition,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: true,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        bool CanEndSelectCondition(List<Permanent> permanents)
                        {
                            if (permanents.Count <= 0)
                            {
                                return false;
                            }

                            int sumDP = 0;

                            foreach (Permanent permanent in permanents)
                            {
                                sumDP += permanent.DP;
                            }

                            if (sumDP > MaxDPForDeletion())
                            {
                                return false;
                            }

                            return true;
                        }

                        bool CanTargetConditionByPreSelectedList(List<Permanent> permanents, Permanent permanent)
                        {
                            int sumDP = 0;

                            foreach (Permanent permanent1 in permanents)
                            {
                                sumDP += permanent1.DP;
                            }

                            sumDP += permanent.DP;

                            if (sumDP > MaxDPForDeletion())
                            {
                                return false;
                            }

                            return true;
                        }
                    }

                    Permanent thisPermanent = card.PermanentOfThisCard();

                    if (thisPermanent.CanAttack(activateClass))
                    {
                        SelectAttackEffect selectAttackEffect =
                                GManager.instance.GetComponent<SelectAttackEffect>();

                        selectAttackEffect.SetUp(
                            attacker: thisPermanent,
                            canAttackPlayerCondition: () => true,
                            defenderCondition: (permanent) => true,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                    }
                }
            }
            #endregion

            #region Shared WD / WA

            string SharedHashString = "AD1_008_WD_WA";

            string SharedEffectName = "Delete lowest DP";

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] Delete 1 of your opponent's lowest DP Digimon.";

            bool IsWeakestEnemyDigimon(Permanent permanent) =>  CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy);

            bool SharedCanActivateCondition(Hashtable hashtable) =>  CardEffectCommons.IsExistOnBattleArea(card);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(IsWeakestEnemyDigimon))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsWeakestEnemyDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }

            #endregion

            #region When Digivolving (OPT)
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), 1, false, SharedEffectDescription("When Digivolving"));
                activateClass.SetHashString(SharedHashString);
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
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), 1, false, SharedEffectDescription("When Attacking"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && card.PermanentOfThisCard().DigivolutionCards.Any(cardSource => cardSource.EqualsCardName("Takato Matsuki"));
                }

                #region Immunity
                CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's effects", CanUseCondition, card);
                canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                cardEffects.Add(canNotAffectedClass);

                bool CanUseCondition(Hashtable hashtable) => Condition();
                
                bool CardCondition(CardSource cardSource) => cardSource == card;

                bool SkillCondition(ICardEffect cardEffect) => CardEffectCommons.IsOpponentEffect(cardEffect, card);
                #endregion

                #region +5000 DP
                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect<int>(
                    changeValue: 5000,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition));
                #endregion
            }
            #endregion

            return cardEffects;
        }
    }
}
