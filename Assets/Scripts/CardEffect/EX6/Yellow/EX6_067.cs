using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DCGO.CardEffects.EX6
{
    public class EX6_067 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Main Effect
            
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[Main] Unsuspend 1 of your Digimon with the [Angel]/[Archangel]/[Three Great Angels] trait. If you have [Dominimon], unsuspend all of your Digimon with the [Angel]/[Archangel]/[Three Great Angels] trait instead.";
                }
                
                bool PermanentIsDominimon(Permanent permanent)
                {
                    return permanent.TopCard.ContainsCardName("Dominimon");
                }
                
                bool PermanentIsCorrectTarget(Permanent permanent)
                {
                    if (permanent.IsSuspended && permanent.CanUnsuspend)
                    {
                        return permanent.TopCard.HasAngelTraitRestrictive;
                    }
                    
                    return false;
                }
                
                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        return PermanentIsCorrectTarget(permanent);
                    }
                    
                    return false;
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Count(PermanentIsDominimon) >= 1)
                        {
                            List<Permanent> untappedPermanents = new List<Permanent>();
                            
                            foreach (Permanent permanent in card.Owner.GetBattleAreaDigimons())
                            {
                                if (PermanentIsCorrectTarget(permanent))
                                {
                                    untappedPermanents.Add(permanent);
                                }
                            }
                            
                            yield return ContinuousController.instance.StartCoroutine(
                                new IUnsuspendPermanents(untappedPermanents, activateClass).Unsuspend());
                        }
                        else
                        {
                            int maxCount = Math.Min(1,
                                CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));
                            
                            SelectPermanentEffect selectPermanentEffect =
                                GManager.instance.GetComponent<SelectPermanentEffect>();
                            
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
                                mode: SelectPermanentEffect.Mode.UnTap,
                                cardEffect: activateClass);
                            
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }
                }
            }
            
            #endregion
            
            #region Security Effect
            
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"[Recovery +1 <Deck>], then add this card to the hand", CanUseCondition,
                    card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);
                
                string EffectDiscription()
                {
                    return "[Security] [Recovery +1 <Deck>]. Then, add this card to the hand";
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.LibraryCards.Count >= 1)
                    {
                        if (card.Owner.CanAddSecurity(activateClass))
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                new IRecovery(card.Owner, 1, activateClass).Recovery());
                        }
                    }
                    
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.AddThisCardToHand(card, activateClass));
                }
            }
            
            #endregion
            
            return cardEffects;
        }
    }
}