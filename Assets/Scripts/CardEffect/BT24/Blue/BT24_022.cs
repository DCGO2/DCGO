using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Ikkakumon
namespace DCGO.CardEffects.BT24
{
    public class BT24_022 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Condition

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel3 && targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Jamming
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(false, card, null));
            }
            #endregion

            #region Shared OP / WD

            string SharedEffectName()
            {
                return "Trash top 2 digivolution cards of 1 opponent's Digimon. 1 of their Digimon with as many of fewer sources as this Digimon can't suspend until their turn ends.";
            }

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] Trash the top 2 digivolution cards of 1 of your opponent's Digimon. Then, 1 of their Digimon with as many of fewer digivolution cards as this Digimon can't suspend until their turn ends.";
            }

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card) &&
                    card.Owner.Enemy.GetBattleAreaDigimons().Count() > 0;
            }

            bool CanStripCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanStunCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.DigivolutionCards.Count() <= card.PermanentOfThisCard().DigivolutionCards.Count();
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Strip 2 sources

                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanStripCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanStripCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will trash digivolution cards.", "The opponent is selecting 1 Digimon that will trash digivolution cards.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanent, trashCount: 2, isFromTop: true, activateClass: activateClass));
                    }
                }

                #endregion

                #region Stun 1 Digimon

                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanStunCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanStunCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that cannot suspend.", "The opponent is selecting 1 Digimon that cannot suspend.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                        canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCanNotSuspendCondition, card);
                        canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCanNotSuspendCondition);
                        permanent.UntilOwnerTurnEndEffects.Add(_ => canNotSuspendClass);

                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                                .CreateDebuffEffect(permanent));
                        }

                        bool CanUseCanNotSuspendCondition(Hashtable hashtableCanNotSuspend)
                        {
                            return permanent.TopCard != null && !permanent.TopCard.CanNotBeAffected(activateClass);
                        }

                        bool PermanentCanNotSuspendCondition(Permanent permanentCanNotSuspend)
                        {
                            return permanentCanNotSuspend == permanent;
                        }
                    }
                }

                #endregion
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition,(hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card)
                        && CardEffectCommons.IsExistOnBattleArea(card);
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition,(hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card)
                        && CardEffectCommons.IsExistOnBattleArea(card);
                }
            }
            #endregion

            #region ESS
            if (timing == EffectTiming.OnUnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If you have 7 or fewer cards in hand, <Draw 1>.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT24_022_YT_Draw1");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                 => "[Your Turn] [Once Per Turn] When this Digimon unsuspends, if you have 7 or fewer cards in your hand, <Draw 1>.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                        CardEffectCommons.IsOwnerTurn(card) &&
                        CardEffectCommons.CanTriggerWhenPermanentUnsuspends(hashtable, permanent => permanent == card.PermanentOfThisCard());
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                        card.Owner.HandCards.Count <= 7;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
