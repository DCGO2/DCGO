using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

// Imperialdramon: Fighter Mode
namespace DCGO.CardEffects.AD1
{
    public class AD1_024 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolve Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Hero");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 5, permanentCondition: PermanentCondition, digivolutionCost: 5, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Imperialdramon: Dragon Mode") || targetPermanent.TopCard.EqualsCardName("Imperialdramon Dragon Mode");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Security +1
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared WD / WA

            string SharedHashString = "AD1_024_WD_WA";

            string SharedEffectName = "Bottom deck 1 Opponent's lowest DP Digimon";

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] Return 1 of your opponent's lowest DP Digimon to the bottom of the deck.";

            bool IsLowestDPEnemy(Permanent permanent) => CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy);

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsLowestDPEnemy))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsLowestDPEnemy,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), 1, false, SharedEffectDescription("When Digivolving"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), 1, false, SharedEffectDescription("When Attacking"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May suspend 1 enemy Digimon and unsuspend. May bottom deck suspended enemy Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("AD1_024_AT");
                cardEffects.Add(activateClass);

                string EffectDescription() 
                    => "[All Turns] [Once Per Turn] When Digimon are played or digivolve, you may suspend 1 of your opponent's Digimon and unsuspend this Digimon. Then, if played or digivolved by effects, you may return 1 of your opponent's suspended Digimon to the bottom of the deck.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, IsDigimonCondition)
                            || CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, IsDigimonCondition));
                }

                bool IsDigimonCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);

                bool IsOpponentsDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool IsOpponentsSuspendedDigimon(Permanent permanent) => IsOpponentsDigimon(permanent) && permanent.IsSuspended;

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleArea(card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool isByEffect = CardEffectCommons.IsByEffect(hashtable, null);
                    bool executed = false;
                    string _Message;
                    if (isByEffect)
                    {
                        _Message = $"Card entered by effect, will you use \"{activateClass.EffectName}\"?";
                    }
                    else
                    {
                        _Message = "Card did not enter by effect, Will you use \"May suspend 1 enemy Digimon and unsuspend.\"?";
                    }

                    List<SelectionElement<bool>> selectionElements1 = new List<SelectionElement<bool>>()
                    {
                        new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                        new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                    };

                    string selectPlayerMessage1 = _Message;
                    string notSelectPlayerMessage1 = "Opponent is selecting an effect";

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage1, notSelectPlayerMessage: notSelectPlayerMessage1);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    var activate = GManager.instance.userSelectionManager.SelectedBoolValue;

                    if (activate)
                    {
                        if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsOpponentsDigimon))//first may includes both actions, so if suspend is possible
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsOpponentsDigimon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                                mode: SelectPermanentEffect.Mode.Tap,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                            {
                                if (permanents.Count > 0) yield return ContinuousController.instance.StartCoroutine(
                                    new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                                executed = true;
                                yield return null;
                            }
                        }
                        else //if nothing to suspend, may still choose to take the action to unsuspend
                        {
                            string selectPlayerMessage = "Will you unsuspend this card?";
                            string notSelectPlayerMessage = "The opponent is choosing if they will unsuspend.";

                            List<SelectionElement<bool>> command_SelectCommands = new List<SelectionElement<bool>>()
                            {
                                new SelectionElement<bool>(message: $"Yes", value: true, spriteIndex: 0),
                                new SelectionElement<bool>(message: $"No", value: false, spriteIndex: 1),
                            };

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: command_SelectCommands, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            bool unsuspend = GManager.instance.userSelectionManager.SelectedBoolValue;

                            if (unsuspend)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                                    new List<Permanent>() { card.PermanentOfThisCard() },
                                    activateClass).Unsuspend());
                                executed = true;
                            }
                        }

                        if (isByEffect && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsOpponentsSuspendedDigimon))
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsOpponentsSuspendedDigimon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            executed = true;
                        }
                    }

                    if (!executed) activateClass.RemoveUse();
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
