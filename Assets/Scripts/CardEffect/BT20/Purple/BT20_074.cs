using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.BT20
{
    public class BT20_074 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
           
            #region DNA Digivolve
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
                                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))

                                {
                                            if (permanent.TopCard.CardColors.Contains(CardColor.Purple))
                                            {
                                                if (permanent.Levels_ForJogress(card).Contains(4))
                                                {
                                                    return true;
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
                                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
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

                        return false;
                    }

                    JogressConditionElement[] elements = new JogressConditionElement[]
                    {
                        new JogressConditionElement(PermanentCondition1, "a level 4 purple Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 4 red Digimon"),
                    };

                    JogressCondition jogressCondition = new JogressCondition(elements, 0);

                    return jogressCondition;
                }

                return null;
            }
            }
            
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return a card from your trash to the hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] You may return 1 Digimon card with [Imperialdramon] in its name or the [Free] trait from your trash to the hand.";
                }

                bool CanSelectCardCondition(CardSource cardSource){
                    if (cardSource.EqualsTraits("Free") || cardSource.ContainsCardName("Imperialdramon")){
                        return true;
                    }
                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable,card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        int maxCount = Math.Min(1, card.Owner.TrashCards.Count(CanSelectCardCondition));

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to add to your hand.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.AddHand,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return a card from your trash to the hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may return 1 Digimon card with [Imperialdramon] in its name or the [Free] trait from your trash to the hand.";
                }

                bool CanSelectCardCondition(CardSource cardSource){
                    if (cardSource.EqualsTraits("Free") || cardSource.ContainsCardName("Imperialdramon")){
                        return true;
                    }
                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable,card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        int maxCount = Math.Min(1, card.Owner.TrashCards.Count(CanSelectCardCondition));

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to add to your hand.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.AddHand,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }
                }
            }
            #endregion

            #region All Turns
            if(timing == EffectTiming.WhenReturntoLibraryAnywhere || timing == EffectTiming.WhenReturntoHandAnyone ){
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If returned, DNA Digivolve", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("DNA Digivolve into Imperialdramon");
                cardEffects.Add(activateClass);

                string EffectDiscription(){
                    return "[All Turns] When any of your [Dinobeemon]/[Paildramon] would be returned to hands or decks, 2 of your Digimon may DNA digivolve into [Imperialdramon: Dragon Mode] in the hand.";
                }

                bool isPaildramonOrDinoBeemon(Permanent permanent){
                    
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if(permanent.TopCard.EqualsCardName("Dinobeemon") || permanent.TopCard.EqualsCardName("Paildramon"))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                
               
                bool CanSelectDNACardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.CanPlayJogress(true))
                        {
                            if (cardSource.EqualsCardName("Imperialdramon: Dragon Mode"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanActivateCondition1(Hashtable hashtable)
                {  
                    if(CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, isPaildramonOrDinoBeemon)){                  
                        if (CardEffectCommons.MatchConditionPermanentCount(isPaildramonOrDinoBeemon)>=2){
                            return true;
                        }
                        return false;
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                                                        CardEffectCommons.DNADigivolvePermanentsIntoHandOrTrashCard(
                                                            CanSelectDNACardCondition,
                                                            payCost: true,
                                                            isHand: true,
                                                            activateClass
                                                        ));
                }
            }
            #endregion

            #region Inherited Effect
            if (timing == EffectTiming.None)
            {
            DisableEffectClass invalidationClass = new DisableEffectClass();
            invalidationClass.SetUpICardEffect("Ignore Security Effect", CanUseCondition, card);
            invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
            invalidationClass.SetIsInheritedEffect(true);
            cardEffects.Add(invalidationClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                bool InvalidateCondition(ICardEffect cardEffect)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card) && CardEffectCommons.IsOwnerTurn(card) && cardEffect.EffectSourceCard != null && cardEffect.EffectSourceCard.IsOption && cardEffect.IsSecurityEffect && GManager.instance.attackProcess.AttackingPermanent == card.PermanentOfThisCard()) 
                    {                                                           
                        return true;                                            
                    }
                    return false;
                }
            }

            #endregion    

        return cardEffects;

        }
    }
}