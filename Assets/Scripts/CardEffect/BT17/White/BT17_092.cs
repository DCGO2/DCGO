using System.Collections;
using System.Collections.Generic;

// Menoa Bellucci
namespace DCGO.CardEffects.BT17
{
    public class BT17_092 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1, Draw 2", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] By trashing 1 [Morphomon] or [Eosmon] in your hand, <Draw 2>.";
                }

                bool HasSpecifiedTrashTarget(CardSource source)
                {
                    return source.EqualsCardName("Morphomon")
                            || source.EqualsCardName("Eosmon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, HasSpecifiedTrashTarget);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: HasSpecifiedTrashTarget,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: false,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: SelectCardCoroutine,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    selectHandEffect.SetUpCustomMessage("Select card to discard.", "The opponent is selecting card to discard.");

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectCardCoroutine(List<CardSource> selectedCards)
                    {
                        if (selectedCards.Count > 0)
                            yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 2, activateClass).Draw());
                    }
                }
            }
            #endregion

            #region All Turns - Turn off On Plays
            if (timing == EffectTiming.None)
            {
                DisableEffectClass invalidationClass = new DisableEffectClass();
                invalidationClass.SetUpICardEffect("Ignore [On Play] Effect of opponent's Tamers", CanUseCondition, card);
                invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                cardEffects.Add(invalidationClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasEosmon);
                }

                bool HasEosmon(Permanent permanent)
                {
                    return permanent.TopCard.EqualsCardName("Eosmon");
                }

                bool InvalidateCondition(ICardEffect cardEffect)
                {
                    return cardEffect != null
                        && cardEffect is ActivateICardEffect
                        && cardEffect.EffectSourceCard != null
                        && CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(cardEffect.EffectSourceCard.PermanentOfThisCard(), card)
                        && cardEffect.EffectSourceCard.PermanentOfThisCard().IsTamer
                        && !cardEffect.EffectSourceCard.PermanentOfThisCard().TopCard.CanNotBeAffected(invalidationClass)
                        && cardEffect.IsOnPlay
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasEosmon);
                }
            }
            #endregion

            #region Opponents Turn - Protection
            if (timing == EffectTiming.WhenRemoveField)
            {
                List<Permanent> removedPermanents = new List<Permanent>();

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 other [Eosmon] to prevent leaving battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("BT17_092_OT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Opponent's Turn] [Once Per Turn] When one of your [Eosmon] would leave the battle area by an opponent's effect, by deleting 1 of your other [Eosmon], prevent it from leaving.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOpponentTurn(card)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, isEosmon)
                        && CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOpponentEffect(cardEffect, card));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass))
                    {
                        removedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable).Filter(isEosmon);

                        return CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasOtherEosmon);
                    }
                        
                    return false;
                }

                bool isEosmon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && permanent.TopCard.EqualsCardName("Eosmon");
                }

                bool HasOtherEosmon(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        foreach (Permanent removed in removedPermanents)
                        {
                            if (removed != permanent)
                            {
                                return permanent.TopCard.EqualsCardName("Eosmon");
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: HasOtherEosmon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent>() { permanent },
                            activateClass: activateClass,
                            successProcess: permanents => SuccessProcess(),
                            failureProcess: null));

                        IEnumerator SuccessProcess()
                        {
                            foreach (Permanent removed in removedPermanents)
                            {
                                removed.willBeRemoveField = false;
                                removed.HideDeleteEffect();
                                removed.HideHandBounceEffect();
                                removed.HideDeckBounceEffect();
                                removed.HideWillRemoveFieldEffect();
                            }
                                    
                            yield return null;
                        }
                    }
                }
            }
            #endregion

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}