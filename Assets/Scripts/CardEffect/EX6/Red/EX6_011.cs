using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.EX6
{
    public class EX6_011 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Ace - Blast DNA Digivolve
            
            if (timing == EffectTiming.OnCounterTiming)
            {
                List<BlastDNACondition> blastDNAConditions = new List<BlastDNACondition>
                {
                    new("Durandamon"),
                    new("BryweLudramon")
                };
                cardEffects.Add(CardEffectFactory.BlastDNADigivolveEffect(card: card,
                    blastDNAConditions: blastDNAConditions, condition: null));
            }
            
            #endregion
            
            #region DNA Digivolution
            
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
                                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent,
                                                    card))
                                            {
                                                if (permanent.TopCard.CardColors.Contains(CardColor.Red))
                                                {
                                                    if (permanent.Levels_ForJogress(card).Contains(6))
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
                                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent,
                                                    card))
                                            {
                                                if (permanent.TopCard.CardColors.Contains(CardColor.Black))
                                                {
                                                    if (permanent.Levels_ForJogress(card).Contains(6))
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
                        
                        JogressConditionElement[] elements =
                        {
                            new(PermanentCondition1, "a level 6 Red Digimon"),
                            new(PermanentCondition2, "a level 6 Black Digimon")
                        };
                        
                        JogressCondition jogressCondition = new JogressCondition(elements, 0);
                        
                        return jogressCondition;
                    }
                    
                    return null;
                }
            }
            
            #endregion
            
            #region Raid
            
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card,
                    condition: null));
            }
            
            #endregion
            
            #region Reboot
            
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }
            
            #endregion
            
            #region Shared On Play / When Digivolving
                        
            bool CanActivateSharedCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }
            
            bool CanUseImmunitySharedCondition(Hashtable hashtableI)
            {
                return CardEffectCommons.IsPermanentExistsOnBattleArea(card.PermanentOfThisCard());
            }
            
            bool CardImmunitySharedCondition(CardSource cardSource)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (cardSource == card.PermanentOfThisCard().TopCard)
                    {
                        return true;
                    }
                }
                
                return false;
            }
            
            bool SkillImmunitySharedCondition(ICardEffect cardEffect)
            {
                if (cardEffect != null)
                {
                    if (cardEffect.EffectSourceCard)
                    {
                        if (CardEffectCommons.IsOpponentEffect(cardEffect, card))
                        {
                            return true;
                        }
                    }
                }
                
                return false;
            }
            
            bool IsEnemyPermanentShared(Permanent permanent)
            {           
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }
            
            #endregion
            
            #region On Play
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Trash the top card of your opponent's security stack and this Digimon isn't affected by your opponent's effects until the end of their turn.",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Play] Trash the top card of your opponent's security stack and this Digimon isn't affected by your opponent's effects until the end of their turn. Then, if DNA digivolving,  all of your opponent's Digimon (Trash the top card. You can't trash past level 3 cards) and delete 1 of their Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    // Trash Top of Opponent's Security Stack
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                    
                    // This Digimon isn't affected by your opponent's effects until the end of their turn.
                    CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                    canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's effects",
                        CanUseImmunitySharedCondition, card);
                    canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardImmunitySharedCondition,
                        SkillCondition: SkillImmunitySharedCondition);
                    card.PermanentOfThisCard().UntilOpponentTurnEndEffects.Add(_ => canNotAffectedClass);
                    
                    
                    // if DNA Digivolving
                    if (CardEffectCommons.IsJogress(hashtable))
                    {
                        // De-Digivolve all Opponent's Digimon
                        List<Permanent> enemyPermanents = card.Owner.Enemy.GetBattleAreaDigimons();
                        
                        foreach (Permanent permanent in enemyPermanents)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                new IDegeneration(permanent, 1, activateClass).Degeneration());
                        }
                        
                        // Delete 1 Opponent's Digimon
                        if (CardEffectCommons.HasMatchConditionPermanent(IsEnemyPermanentShared))
                        {
                            int enemyCount = Math.Min(1,
                                CardEffectCommons.MatchConditionPermanentCount(IsEnemyPermanentShared));
                            
                            SelectPermanentEffect selectEnemyEffect =
                                GManager.instance.GetComponent<SelectPermanentEffect>();
                            
                            selectEnemyEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsEnemyPermanentShared,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: enemyCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectEnemyEffect.Activate());
                        }
                    }
                }
            }
            
            #endregion
            
            #region When Digivolving
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Trash the top card of your opponent's security stack and this Digimon isn't affected by your opponent's effects until the end of their turn.",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] Trash the top card of your opponent's security stack and this Digimon isn't affected by your opponent's effects until the end of their turn. Then, if DNA digivolving,  all of your opponent's Digimon (Trash the top card. You can't trash past level 3 cards) and delete 1 of their Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    // Trash Top of Opponent's Security Stack
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                    
                    // This Digimon isn't affected by your opponent's effects until the end of their turn.
                    CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                    canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's effects",
                        CanUseImmunitySharedCondition, card);
                    canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardImmunitySharedCondition,
                        SkillCondition: SkillImmunitySharedCondition);
                    card.PermanentOfThisCard().UntilOpponentTurnEndEffects.Add(_ => canNotAffectedClass);


                    // if DNA Digivolving
                    if (CardEffectCommons.IsJogress(hashtable))
                    {
                        // De-Digivolve all Opponent's Digimon
                        List<Permanent> enemyPermanents = card.Owner.Enemy.GetBattleAreaDigimons().ToList();
                        
                        foreach (Permanent permanent in enemyPermanents)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                new IDegeneration(permanent, 1, activateClass).Degeneration());
                        }
                        
                        // Delete 1 Opponent's Digimon
                        if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsEnemyPermanentShared))
                        {                            
                            SelectPermanentEffect selectEnemyEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                            
                            selectEnemyEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsEnemyPermanentShared,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectEnemyEffect.Activate());
                        }
                    }
                }
            }
            
            #endregion
            
            return cardEffects;
        }
    }
}