using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Chirinmon
namespace DCGO.CardEffects.BT22
{
    public class BT22_037 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel4 && targetPermanent.TopCard.HasCSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region When trashed from security

            if (timing == EffectTiming.OnDiscardSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("-8K DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When effects trash this card from the security stack, 1 of your opponent's Digimon gets -8000 DP for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnTrashSelfSecurity(hashtable, cardEffect => cardEffect != null, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card)
                        && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsOpponentDigimon);
                }

                bool IsOpponentDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermament = null;

                    #region Select Permament

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, IsOpponentDigimon));
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOpponentDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermament = permanent;
                        yield return null;
                    }

                    selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to -8K DP", "The opponent is selecting 1 digimon to -8K DP");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    #endregion

                    if (selectedPermament != null) yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.ChangeDigimonDP(selectedPermament, -8000, EffectDuration.UntilEachTurnEnd, activateClass));
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing top security, digivolve into card with [Kentaurosmon]/[Mitamamon] in name or [CS] trait ", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By trashing your top security card, this Digimon may digivolve into a Digimon card with [Kentaurosmon] or [Mitamamon] in its name or the [CS] trait in the hand with the digivolution cost reduced by 2.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CardCondition);
                }

                bool CardCondition(CardSource cardSource)
                {
                    return CardEffectCommons.IsExistOnHand(cardSource)
                        && cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, true, activateClass)
                        && (cardSource.ContainsCardName("Kentaurosmon") || cardSource.ContainsCardName("Mitamamon") || cardSource.HasCSTraits);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool isTrashingSecurity = false;

                    if (card.Owner.SecurityCards.Any())
                    {
                        #region Make Selection for Trashing Security

                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Trash top security to digivolve?";
                        string notSelectPlayerMessage = "The opponent is choosing to trash top secuirty.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                        isTrashingSecurity = GManager.instance.userSelectionManager.SelectedBoolValue;

                        #endregion
                    }

                    if (isTrashingSecurity)
                    {
                        #region Trash Top Security And Check

                        CardSource topSec = card.Owner.SecurityCards.FirstOrDefault();

                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());

                        List<CardSource> DiscardedCards = CardEffectCommons.GetDiscardedCardsFromHashtable(hashtable);

                        #endregion

                        if (DiscardedCards.Contains(topSec))
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                    targetPermanent: card.PermanentOfThisCard(),
                                    cardCondition: CardCondition,
                                    payCost: true,
                                    reduceCostTuple: (2, null),
                                    fixedCostTuple: null,
                                    ignoreDigivolutionRequirementFixedCost: -1,
                                    isHand: true,
                                    activateClass: activateClass,
                                    successProcess: null));
                        }
                    }
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("-4K DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("BT22_037_WA");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] 1 of your opponent's Digimon gets -4000 DP for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnField(card)
                        && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsOpponentDigimon);
                }

                bool IsOpponentDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsOpponentDigimon))
                    {
                        Permanent selectedPermament = null;

                        #region Select Permanent

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, IsOpponentDigimon));
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOpponentDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermament = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to -4K DP", "The opponent is selecting 1 Digimon to -4K DP");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        #endregion

                        if (selectedPermament != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            targetPermanent: selectedPermament,
                            changeValue: -4000,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass
                            ));
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}