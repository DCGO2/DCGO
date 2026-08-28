using System;
using System.Collections;
using System.Collections.Generic;

// Aegiochusmon: Holy
namespace DCGO.CardEffects.BT26
{
    public class BT26_029 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Aegiomon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region Decode
            if (timing == EffectTiming.WhenRemoveField)
            {
                bool SourceCondition(CardSource source)
                {
                    return source.EqualsCardName("Aegiomon");
                }

                string[] decodeStrings = { "(Aegiomon)", "Aegiomon" };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(
                    card: card,
                    isInheritedEffect: false,
                    decodeStrings: decodeStrings,
                    sourceCondition: SourceCondition,
                    condition: null));
            }
            #endregion

            #region Ascension
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.AscensionSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP / WD
            string SharedEffectName = "By trashing your top security, grant DP-immunity, stack-trash-immunity, return-immunity on 1 Digimon";

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] By trashing your top security card, until your opponent's turn ends, their effects can't reduce the DP of 1 of your Digimon, trash any of its stacked cards, or return them to hands or decks.";
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Trash Top Security
                yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                    player: card.Owner,
                    destroySecurityCount: 1,
                    cardEffect: activateClass,
                    fromTop: true).DestroySecurity());
                #endregion

                #region Select Your Digimon
                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 of your Digimon to grant immunities.", "The opponent is selecting 1 Digimon to grant immunities.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainImmuneFromDPMinus(
                            targetPermanent: permanent,
                            cardEffectCondition: null,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass,
                            effectName: "Can't reduce DP"));

                        ImmuneStackTrashingClass immuneFromStackTrashingClass = new ImmuneStackTrashingClass();
                        immuneFromStackTrashingClass.SetUpICardEffect("Isn't affected by trashing any stacked card", CanUseCondition1, permanent.TopCard);
                        immuneFromStackTrashingClass.SetUpImmuneFromStackTrashingClass(PermanentCondition: PermanentCondition1, EffectCondition: EffectCondition1);          
                        permanent.UntilOpponentTurnEndEffects.Add((_timing) => immuneFromStackTrashingClass);
                        permanent.UntilOpponentTurnEndEffects.Add((_timing) => 
                            CardEffectFactory.CanNotBeReturnedToLibraryBySkillStaticEffect(PermanentCondition1, EffectCondition1, false, card, null, "Stacked cards may not be returned to hand or deck."));

                        bool CanUseCondition1(Hashtable hashtable1)
                        {
                            return permanent.TopCard != null;
                        }

                        bool EffectCondition1(ICardEffect effect)
                        {
                            return CardEffectCommons.IsOpponentEffect(effect, card);
                        }

                        bool PermanentCondition1(Permanent permanent1)
                        {
                            return permanent1 == permanent;
                        }
                    }
                }
                #endregion
            }

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: true,
                    onPlay: true,
                    whenDigivolving: true,
                    additionalActivateCondition: AdditionalActivateCondition);

            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return card.Owner.SecurityCards.Count > 0;
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("3 opponent's Digimon get -5000 DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT26_029_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When your security stack is removed from, 3 of your opponent's Digimon get -5000 DP for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, player => player == card.Owner);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int maxCount = Math.Min(3, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage($"Select {maxCount} Digimon to -5000 DP.", "The opponent is selecting Digimon to -5000 DP.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            targetPermanent: permanent,
                            changeValue: -5000,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass));
                    }
                }
            }
            #endregion

            #region Inherited Effect
            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("De-Digivolve 1 opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT26_029_INF");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When your security stack is removed from, <De-Digivolve 1> 1 of your opponent's Digimon. (Trash the top card. You can't trash past level 3 cards.)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, player => player == card.Owner);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Degenerate,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve.", "The opponent is selecting 1 Digimon to De-Digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}