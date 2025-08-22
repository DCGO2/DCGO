using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Reppamon
namespace DCGO.CardEffects.BT22
{
    public class BT22_034 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel3 && targetPermanent.TopCard.HasCSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region When trashed from security

            if (timing == EffectTiming.OnDiscardSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play Card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When effects trash this card from the security stack, you may play this card without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnTrashSelfSecurity(hashtable, cardEffect => cardEffect != null, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: new List<CardSource>() { card }, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Execution, activateETB: true));
                }
            }

            #endregion

            #region OP/WD Shared

            bool SharedIsOpponentDigimon(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool isTrashingSecurity = false;
                int dpChange = 3000;

                if (card.Owner.SecurityCards.Any())
                {
                    #region Make Selection for Trashing Security

                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                        };

                    string selectPlayerMessage = "Trash top security to -6K DP instead of 3K?";
                    string notSelectPlayerMessage = "The opponent is choosing to trash top secuirty.";

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                    isTrashingSecurity = GManager.instance.userSelectionManager.SelectedBoolValue;

                    #endregion
                }

                if (isTrashingSecurity)
                {
                    CardSource topSec = card.Owner.SecurityCards.FirstOrDefault();

                    #region Trash Top Security

                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                    player: card.Owner,
                    destroySecurityCount: 1,
                    cardEffect: activateClass,
                    fromTop: true).DestroySecurity());

                    #endregion

                    if (CardEffectCommons.IsExistOnTrash(topSec)) dpChange = 6000;
                }

                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, SharedIsOpponentDigimon))
                {
                    Permanent selectedPermament = null;

                    #region Select Permanent

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, SharedIsOpponentDigimon));
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: SharedIsOpponentDigimon,
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

                    selectPermanentEffect.SetUpCustomMessage($"Select 1 Digimon to -{dpChange} DP", $"The opponent is selecting 1 Digimon to -{dpChange} DP");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    #endregion

                    if (selectedPermament != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                        targetPermanent: selectedPermament,
                        changeValue: -dpChange,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass
                    ));
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash top sec for -6K DP, otherwise -3K DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] 1 of your opponent's Digimon gets -3000 DP until their turn ends. By trashing your top security card, instead 1 of your opponent's Digimon gets -6000 DP until their turn ends.";
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
                    yield return ContinuousController.instance.StartCoroutine(SharedActivateCoroutine(hashtable, activateClass));
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash top sec for -6K DP, otherwise -3K DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] 1 of your opponent's Digimon gets -3000 DP until their turn ends. By trashing your top security card, instead 1 of your opponent's Digimon gets -6000 DP until their turn ends.";
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
                    yield return ContinuousController.instance.StartCoroutine(SharedActivateCoroutine(hashtable, activateClass));
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("-2K DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("BT22_034_WA");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] 1 of your opponent's Digimon gets -2000 DP for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
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

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to -2K DP", "The opponent is selecting 1 Digimon to -2K DP");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        #endregion

                        if (selectedPermament != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            targetPermanent: selectedPermament,
                            changeValue: -2000,
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