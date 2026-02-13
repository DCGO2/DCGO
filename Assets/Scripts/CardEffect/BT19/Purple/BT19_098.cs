using System.Collections;
using System.Collections.Generic;
using System;

namespace DCGO.CardEffects.BT19
{
    public class BT19_098 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Ignore Color Requirements
            
            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);

                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return !CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.TopCard.EqualsCardName("King Device"));
                }

                bool CardCondition(CardSource cardSource)
                {
                    return (cardSource == card);
                }
            }
            
            #endregion

            #region When Trashed from battle area
            
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("place 1 [Device] trait Option card from trash to battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "When this card is trashed in your battle area, place 1 [Device] trait Option card with cost of 3 from your trash to the battle area.";
                }

                bool IsPlayableDevice(CardSource cardSource)
                {
                    return cardSource.IsOption &&
                           cardSource.EqualsTraits("Device") &&
                           cardSource.GetCostItself == 3;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card) &&
                           CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsPlayableDevice);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedCard = null;
                    
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: IsPlayableDevice,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 card to place in battle area.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage(
                        "Select 1 card to place in battle area.",
                        "The opponent is selecting 1 card to place in battle area.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Place in battle area");
                    
                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    
                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;

                        yield return null;
                    }

                    if(selectedCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: selectedCard, cardEffect: activateClass));
                    }
                }
            }
            
            #endregion

            #region Main
            
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [Device] trait Option card from trash in battle area, then place this card in battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] Place 1 [Device] trait Option card with cost of 3 from your trash to the battle area. Then, place this card in the battle area.";
                }
                
                bool IsPlayableDevice(CardSource cardSource)
                {
                    return cardSource.IsOption &&
                           cardSource.EqualsTraits("Device") &&
                           cardSource.GetCostItself == 3;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedCard = null;
                    
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: IsPlayableDevice,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 card to place in battle area.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage(
                        "Select 1 card to place in battle area.",
                        "The opponent is selecting 1 card to place in battle area.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Place in battle area");
                    
                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    
                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;

                        yield return null;
                    }

                    if(selectedCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: selectedCard, cardEffect: activateClass));
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }
            
            #endregion

            #region Security Effect
            
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Place 1 Device trait option from your hand into the battle area, then add to hand.", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Security] You may place 1 [Device] trait Option card from your hand to the battle area. Then, add this card to the hand.";
                }
                
                bool IsPlayableDevice(CardSource cardSource)
                {
                    return cardSource.IsOption &&
                           cardSource.EqualsTraits("Device");
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedCard = null;
                    
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsPlayableDevice,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectHandEffect.SetUpCustomMessage(
                        "Select 1 card to place in battle area.",
                        "The opponent is selecting 1 card to place in battle area.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Place in battle area");

                    yield return StartCoroutine(selectHandEffect.Activate());
                    
                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;

                        yield return null;
                    }

                    if(selectedCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: selectedCard, cardEffect: activateClass));
                    }
                    
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
                }
            }
            
            #endregion

            return cardEffects;
        }
    }
}