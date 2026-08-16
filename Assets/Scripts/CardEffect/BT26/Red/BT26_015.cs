using System;
using System.Collections;
using System.Collections.Generic;

// Butenmon
namespace DCGO.CardEffects.BT26
{
    public class BT26_015 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 4));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName()
                => "Opponent's Digimon -4000 DP, then return a trash card to deck bottom to delete 1 opponent's Digimon";

            string SharedEffectDescription(string tag)
                => $"[{tag}] 1 of your opponent's Digimon gets -4000 DP until their turn ends. Then, by returning 1 card in your trash to the bottom of the deck, delete 1 of your opponent's Digimon with 5000 DP or less.";

            bool CanSelectDebuffTargetCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            bool CanSelectDeleteTargetCondition(Permanent permanent, ICardEffect activateClass)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.DP <= card.Owner.MaxDP_DeleteEffect(5000, activateClass);

            bool SharedCanActivateCondition(Hashtable hashtable, ICardEffect activateClass)
                => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                    && CardEffectCommons.HasMatchConditionPermanent(CanSelectDebuffTargetCondition);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDebuffTargetCondition))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectDebuffTargetCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectDebuffTargetCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get -4000 DP.", "The opponent is selecting 1 Digimon that will get -4000 DP.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: -4000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                    }
                }

                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, _ => true))
                {
                    CardSource selectedCardToReturn = null;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: _ => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 card in your trash to return to the bottom of your deck.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCardToReturn = cardSource;
                        yield return null;
                    }

                    selectCardEffect.SetUpCustomMessage("Select 1 card in your trash to return to the bottom of your deck.", "The opponent is selecting 1 card in their trash to return to the bottom of their deck.");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    if (selectedCardToReturn != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(new List<CardSource>() { selectedCardToReturn }));

                        bool CanSelectDeleteTargetConditionBound(Permanent permanent) => CanSelectDeleteTargetCondition(permanent, activateClass);

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteTargetConditionBound))
                        {
                            int deleteMaxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectDeleteTargetConditionBound));

                            SelectPermanentEffect selectDeleteEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectDeleteEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectDeleteTargetConditionBound,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: deleteMaxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            selectDeleteEffect.SetUpCustomMessage("Select 1 Digimon with 5000 DP or less to delete.", "The opponent is selecting 1 Digimon with 5000 DP or less to delete.");

                            yield return ContinuousController.instance.StartCoroutine(selectDeleteEffect.Activate());
                        }
                    }
                }
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateCondition(hash, activateClass), (hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateCondition(hash, activateClass), (hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnAddLibraryAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 of your Digimon gets +3000 DP and attacks", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT26_015_YourTurn");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Your Turn] [Once Per Turn] When your effects add to decks, 1 of your Digimon may get +3000 DP until your opponent's turn ends and attack.";

                bool CardSourceCondition(CardSource cardSource)
                    => cardSource.Owner == card.Owner;

                bool CanSelectPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerOnAddLibrary(hashtable, CardSourceCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get +3000 DP and attack.", "The opponent is selecting 1 Digimon that will get +3000 DP and attack.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: 3000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));

                        if (permanent.CanAttack(activateClass))
                        {
                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                            selectAttackEffect.SetUp(
                                attacker: permanent,
                                canAttackPlayerCondition: () => true,
                                defenderCondition: (defender) => true,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                        }
                    }
                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.OnAddLibraryAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT26_015_Inherit");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When your effects add to decks, this Digimon with [Chronomon] in its text may unsuspend.";

                bool CardSourceCondition(CardSource cardSource)
                    => cardSource.Owner == card.Owner;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAddLibrary(hashtable, CardSourceCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.HasText("Chronomon")
                        && CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard());

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
