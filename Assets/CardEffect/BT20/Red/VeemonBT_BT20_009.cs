using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects
{
    public class VeemonBT_20_003 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            #region On Your Turn
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [Free] digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When any of your purple Digimon are played, this Digimon may digivolve into a Digimon card with the [Free] trait in the hand with the digivolution cost reduced by 1.";
                }

                bool CardCondition(CardSource cardSource){
                    bool matchColorRequirement = cardSource.CardColors.Every(cardColor =>
                    cardSource.Owner.GetFieldPermanents().Some(permanent =>
                        !permanent.TopCard.IsOption && permanent.TopCard.CardColors.Contains(CardColor.Purple)));
                    if(cardSource.Owner == card.Owner){
                        if(cardSource.IsDigimon){
                            if(matchColorRequirement){
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool canEvolveIntoFree(CardSource cardSource){
                    if (cardSource.CardTraits.Contains("Free")){
                        return true;
                    }
                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition)){
                        return true;
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return true;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int maxCount = Math.Min(1, card.Owner.SecurityCards.Count);
                        
                        CardSource selectedCard = null;
                        
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                        
                        selectCardEffect.SetUp(
                            canTargetCondition: canEvolveIntoFree,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to digivolve.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Security,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);
                        
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        
                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            
                            yield return null;
                        }

                        PlayCardClass playCardClass = new PlayCardClass(
                                        cardSources: new List<CardSource>() { selectedCard },
                                        hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                        payCost: true,
                                        targetPermanent: card.PermanentOfThisCard(),
                                        isTapped: false,
                                        root: SelectCardEffect.Root.Security,
                                        activateETB: true);

                                    playCardClass.SetReducedCost(1);
                }
            }
            #endregion

            #region InheritedEffect
            if (timing == EffectTiming.None)
            {
                bool InheritedEffectCondition()
                {
                    return CardEffectCommons.IsOwnerTurn(card);
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 1000,
                    isInheritedEffect: true,
                    card: card,
                    condition: InheritedEffectCondition));
            }
            #endregion

            return cardEffects;
        }
    }
}