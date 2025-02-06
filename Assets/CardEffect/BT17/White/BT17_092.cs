using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

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
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By trashing 1 [Morphomon] or [Eosmon] in your hand, <Draw 2>.";
                }

                bool HasSpecifiedTrashTarget(CardSource source)
                {
                    return source.EqualsCardName("Morphomon") || source.EqualsCardName("Eosmon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                        return CardEffectCommons.HasMatchConditionOwnersHand(card, HasSpecifiedTrashTarget);

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, HasSpecifiedTrashTarget))
                    {
                        int maxCount = Math.Min(1, card.Owner.HandCards.Count(HasSpecifiedTrashTarget));

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: HasSpecifiedTrashTarget,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
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
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasEosmon))
                            return true;
                    }

                    return false;
                }

                bool HasEosmon(Permanent permanent)
                {
                    if (permanent.IsDigimon)
                        return permanent.TopCard.EqualsCardName("Eosmon");

                    return false;
                }

                bool InvalidateCondition(ICardEffect cardEffect)
                {
                    if (cardEffect != null)
                    {
                        if (cardEffect is ActivateICardEffect)
                        {
                            if (cardEffect.EffectSourceCard != null)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(cardEffect.EffectSourceCard.PermanentOfThisCard(), card))
                                {
                                    if (cardEffect.EffectSourceCard.PermanentOfThisCard().IsTamer)
                                    {
                                        if (!cardEffect.EffectSourceCard.PermanentOfThisCard().TopCard.CanNotBeAffected(invalidationClass))
                                        {
                                            if (cardEffect.IsOnPlay)
                                            {
                                                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasEosmon))
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
            }
            #endregion

            #region Opponents Turn - Protection
            if (timing == EffectTiming.WhenRemoveField)
            {
                List<Permanent> removedPermanents = new List<Permanent>();

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Prevent from leaving battle area, by opponents effect", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Prevent_BT17_092");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Opponent's Turn] [Once Per Turn] When one of your [Eosmon] would leave the battle area by an opponent's effect, by deleting 1 of your other [Eosmon], prevent it from leaving.";
                }

                bool isEosmon(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                        return permanent.TopCard.EqualsCardName("Eosmon");

                    return false;
                }

                bool HasOtherEosmon(Permanent permanent)
                {
                    if(CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        foreach (Permanent removed in removedPermanents)
                        {
                            if (removed != permanent)
                                return permanent.TopCard.EqualsCardName("Eosmon");
                        }
                    }
                    
                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsOpponentTurn(card))
                    {
                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            if (CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, isEosmon))
                            {
                                if (CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOpponentEffect(cardEffect, card)))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        removedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable).Filter(isEosmon);

                        return CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasOtherEosmon);
                    }
                        

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(HasOtherEosmon))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(HasOtherEosmon));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: HasOtherEosmon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
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

                                    removed.HideHandBounceEffect();
                                    removed.HideDeckBounceEffect();
                                    removed.HideWillRemoveFieldEffect();
                                }
                                    
                                yield return null;
                            }
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