using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

// Rebootmon
namespace DCGO.CardEffects.BT25
{
    public class BT25_060 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition - Ult.
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasUltimateAppTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region App Fusion (Bootmon, Shutmon)

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.AddAppfuseMethodByName(new List<string>() { "Bootmon", "Shutmon" }, card));
            }

            #endregion

            #region Security +1
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Link +1
            if (timing == EffectTiming.None) cardEffects.Add(CardEffectFactory.ChangeSelfLinkMaxStaticEffect(1, false, card, null));
            #endregion

            #region Shared WD / WA

            string SharedHashString = "BT25_060_WD_WA";

            string SharedEffectName = "Link from hand or digivolution cards to unsuspend";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] By linking 1 [Appmon] trait Digimon card from your hand or this Digimon's digivolution cards to this Digimon without paying the cost, 1 of your Digimon may unsuspend.";

            bool LinkableCard(CardSource cardSource)
            {
                return cardSource.HasAppmonTraits
                    && cardSource.CanLinkToTargetPermanent(card.PermanentOfThisCard(), false);
            }

            bool IsYourDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return CardEffectCommons.HasMatchConditionOwnersHand(card, LinkableCard)
                    || card.PermanentOfThisCard().DigivolutionCards.Any(LinkableCard);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool linked = false;

                Permanent thisPermanent = card.PermanentOfThisCard();

                bool validHandCard = CardEffectCommons.HasMatchConditionOwnersHand(card, LinkableCard);
                bool validDigivolutionCard = thisPermanent.DigivolutionCards.Any(LinkableCard);
                
                List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                if (validHandCard)
                {
                    selectionElements.Add(new (message: $"Link from Hand", value : 1, spriteIndex: 0));
                }
                if (validDigivolutionCard)
                {
                    selectionElements.Add(new (message: $"Link from Digivolution Cards", value : 2, spriteIndex: 0));
                }
                selectionElements.Add( new (message: $"Don't link", value: 3, spriteIndex: 1));

                string selectPlayerMessage = "From which area will you link a card?";
                string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                bool doLink = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                bool fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                if (doLink)
                {
                    if (fromHand)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: LinkableCard,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage($"Select a card to link.", "The opponent is selecting cards to link.");

                        yield return StartCoroutine(selectHandEffect.Activate());
                    }
                    else
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: LinkableCard,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select card to link.",
                                    maxCount: 1,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.DigivolutionCards,
                                    customRootCardList: thisPermanent.DigivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage($"Select a cards to link.", "The opponent is selecting cards to link.");

                        yield return StartCoroutine(selectCardEffect.Activate());
                    }

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        linked = true;

                        yield return ContinuousController.instance.StartCoroutine(thisPermanent.AddLinkCard(cardSource, activateClass));
                    }
                }

                if (linked) 
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsYourDigimon))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsYourDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.UnTap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    }
                }
                else activateClass.RemoveUse();
            }

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    additionalActivateCondition: AdditionalActivateCondition,
                    maxCountPerTurn: 1,
                    hashValue: SharedHashString,
                    optional: false,
                    isSkippable: true,
                    onPlay: true,
                    whenDigivolving: true);
            #endregion

            #region Shared All Turns

            string AllTurnsHashString = "BT25_060_AT_GAIN_EFFECTS";
            
            string AllTurnsEffectName = "Gain <Piercing>, <Blocker> and Digimon Effect Immunity";

            bool AllTurnsActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleArea(card);

            string AllTurnsEffectDescription()
                => "[All Turns] [Once Per Turn] When this Digimon gets linked or unsuspends, until your turn ends, this Digimon gains <Piercing> and <Blocker>, and your opponent's Digimon effects don't affect it.";

            IEnumerator AllTurnsActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent thisPermanent = card.PermanentOfThisCard();

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainPierce(
                            targetPermanent: thisPermanent,
                            effectDuration: EffectDuration.UntilOwnerTurnEnd,
                            activateClass: activateClass));

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(
                    targetPermanent: thisPermanent, 
                    effectDuration: EffectDuration.UntilOwnerTurnEnd, 
                    activateClass: activateClass));

                ICardEffect GetImmunity(EffectTiming timing)
                {
                    if (timing == EffectTiming.None)
                    {
                        return PermanentEffectFactory.DigimonEffectImmunity(thisPermanent);
                    }

                    return null;
                }

                thisPermanent.UntilOwnerTurnEndEffects.Add(GetImmunity);
            }

            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new();
                activateClass.SetUpICardEffect(AllTurnsEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(AllTurnsActivateCondition, hash => AllTurnsActivateCoroutine(hash, activateClass), 1, false, AllTurnsEffectDescription());
                activateClass.SetHashString(AllTurnsHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenLinking(hashtable, permanent => permanent == card.PermanentOfThisCard(), null);
                }
            }

            if (timing == EffectTiming.OnUnTappedAnyone)
            {
                ActivateClass activateClass = new();
                activateClass.SetUpICardEffect(AllTurnsEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(AllTurnsActivateCondition, hash => AllTurnsActivateCoroutine(hash, activateClass), 1, false, AllTurnsEffectDescription());
                activateClass.SetHashString(AllTurnsHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenSelfPermanentUnsuspends(hashtable, card);
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
