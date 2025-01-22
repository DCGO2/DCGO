using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX8
{
    public class EX8_056 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel2 && targetPermanent.TopCard.EqualsTraits("DS");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 0,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion
            
            #region On Deletion
            
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1 and trash 1 card from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Deletion] <Draw 1> and trash 1 card in your hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable,card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return true;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.LibraryCards.Count >= 1)
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                    if (card.Owner.HandCards.Count >= 1)
                    {
                        int discardCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: (cardSource) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: discardCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return StartCoroutine(selectHandEffect.Activate());
                    }
                }
            }
            
            #endregion
            
            #region When Attacking - ESS
            
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete opponents level 3 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("WhenAttacking_EX8_056");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] [Once Per Turn] Delete 1 of your opponent's level 3 Digimon.";
                }

                bool SelectOpponentsLevel3Digimon(Permanent permanent)
                {
                    if(CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if(permanent.Level == 3)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return true;
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: SelectOpponentsLevel3Digimon,
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
            
            return cardEffects;
        }
    }
}