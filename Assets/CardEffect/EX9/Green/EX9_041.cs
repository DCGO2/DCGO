using System;
using System.Collections;
using System.Collections.Generic;

// ExTyrannomon
namespace DCGO.CardEffects.EX9
{
    public class EX9_041 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Raremon Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Raremon");
                }
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region DM Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel4 &&
                        targetPermanent.TopCard.EqualsTraits("DM");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Ver.5 Digivolution Cost Reduction

            if (timing == EffectTiming.BeforePayCost)
            {
                Permanent selectedPermanent = null;
                bool Condition()
                {
                    return !CardEffectCommons.IsExistOnField(card);
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card) && targetPermanent.TopCard.EqualsTraits("Ver.5");
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                int ReduceCost()
                {
                    return card.PermanentOfThisCard().DigivolutionCards.Filter(x => x.IsFlipped).Count;
                }

                cardEffects.Add(CardEffectFactory.ChangeDigivolutionCostStaticEffect<Func<int>>(
                    changeValue: () => -ReduceCost(),
                    permanentCondition: PermanentCondition,
                    cardCondition: CardSourceCondition,
                    rootCondition: RootCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition,
                    setFixedCost: false));
            }

            #endregion

            #region Fortitude

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.FortitudeSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region On Play/When Digivolving Shared

            bool OpponentsDigimon(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool OpponentsTappedDigimon(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) && permanent.IsSuspended && CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy);
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 Digimon, trash bottom source to bounce suspended digimon with lowest DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Suspend 1 of your opponent's Digimon. Then, by trashing this Digimon's bottom face-down digivolution card, return 1 of your opponent's suspended Digimon with the lowest DP to the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimon))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, OpponentsDigimon));
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentsDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>
                        {
                            new(message: "Yes", value: true, spriteIndex: 0),
                            new(message: "No", value: false, spriteIndex: 1),
                        };
                        string selectPlayerMessage = "Use effect to bounce suspended digimon?";
                        string notSelectPlayerMessage = "The opponent is choosing to bounce suspended digimon.";
                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool isUsingEffect = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (isUsingEffect)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                                targetPermanent: card.PermanentOfThisCard(),
                                trashCount: 1,
                                isFromTop: false,
                                activateClass: activateClass
                            ));

                            int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, OpponentsTappedDigimon));
                            SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();
                            selectPermanentEffect1.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: OpponentsTappedDigimon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Bounce,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 Digimon, trash bottom source to bounce suspended digimon with lowest DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Suspend 1 of your opponent's Digimon. Then, by trashing this Digimon's bottom face-down digivolution card, return 1 of your opponent's suspended Digimon with the lowest DP to the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimon))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, OpponentsDigimon));
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentsDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>
                        {
                            new(message: "Yes", value: true, spriteIndex: 0),
                            new(message: "No", value: false, spriteIndex: 1),
                        };
                        string selectPlayerMessage = "Use effect to bounce suspended digimon?";
                        string notSelectPlayerMessage = "The opponent is choosing to bounce suspended digimon.";
                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool isUsingEffect = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (isUsingEffect)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                                targetPermanent: card.PermanentOfThisCard(),
                                trashCount: 1,
                                isFromTop: false,
                                activateClass: activateClass
                            ));

                            int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, OpponentsTappedDigimon));
                            SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();
                            selectPermanentEffect1.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: OpponentsTappedDigimon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Bounce,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }
                }
            }

            #endregion

            #region All Turns - ESS

            if (timing == EffectTiming.OnEndBattle)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash the top card of opponent's security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("EX9_041_TrashSec");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns] (Once Per Turn) When this Digimon deletes an opponent's Digimon in battle, trash their top security card.";
                }

                bool WinnerCondition(Permanent permanent) => permanent.cardSources.Contains(card);
                bool LoserCondition(Permanent permanent) => CardEffectCommons.IsOpponentPermanent(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDeleteOpponentDigimonByBattle(hashtable: hashtable,
                               winnerCondition: WinnerCondition, loserCondition: LoserCondition, isOnlyWinnerSurvive: false);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.Owner.Enemy.SecurityCards.Count >= 1;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}