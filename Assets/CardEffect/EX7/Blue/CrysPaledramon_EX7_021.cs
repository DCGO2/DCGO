using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects
{
    public class CrysPaledramon_EX7_021 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Change Traits, Iceclad effect

            if (timing == EffectTiming.None)
            {
                ChangeTraitsClass changeTraitsClass = new ChangeTraitsClass();
                changeTraitsClass.SetUpICardEffect("Trait: Has [Ice-Snow] type", CanUseCondition, card);
                changeTraitsClass.SetUpChangeTraitsClass(changeeTraits: ChangeTraits);
                cardEffects.Add(changeTraitsClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                List<string> ChangeTraits(CardSource cardSource, List<string> cardTraits)
                {
                    if (cardSource == card)
                    {
                        cardTraits.Add("Ice-Snow");
                    }

                    return cardTraits;
                }
            }
            
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.IcecladSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 2 then unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Trash any 2 digivolution cards of your opponent's Digimon. Then, if your opponent has no Digimon with digivolution cards, unsuspend this Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return true;
                }

                bool CheckOpponentDigivolutionSources()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        int count = CardEffectCommons.MatchConditionOpponentsPermanentCount(card, (permanent) => permanent.IsDigimon && permanent.HasNoDigivolutionCards);

                        if (count >= 1)
                            return false;
                    }

                    return true;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Count >= 1)
                    {
                        int discardCount = 2;

                        if (card.Owner.HandCards.Count < discardCount)
                            discardCount = card.Owner.HandCards.Count;
                        
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
                    //unsuspend 
                    if (CheckOpponentDigivolutionSources())
                    {
                        Permanent selectedPermanent = card.PermanentOfThisCard();

                        yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());
                    }
                }
            }
            
            #endregion

            #region Inherited Effect

            if (timing == EffectTiming.None)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain Piercing and SecAtk+1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("CrysPaledramon_EX7_021_GainPiercing_SecAtk1");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] While your opponent has no Digimon with digivolution cards, this Digimon with the [Ice-Snow] trait gains <Piercing> and <Security A. +1>";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        int count = CardEffectCommons.MatchConditionOpponentsPermanentCount(card, (permanent) => permanent.IsDigimon && permanent.HasNoDigivolutionCards);

                        if (count >= 1)
                            return false;
                    }
                    return true;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.CardTraits.Contains("Ice-Snow"))
                    {
                        cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: true, card: card, condition: null));
                        cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: true, card: card, condition: null));
                    }

                    yield return null;
                }
            }

            #endregion

            return cardEffects;
        }
    }
}