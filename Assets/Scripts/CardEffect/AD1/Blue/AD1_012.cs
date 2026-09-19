using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// CresGarurumon
namespace DCGO.CardEffects.AD1
{
    public class AD1_012 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Garurumon")
                        || targetPermanent.TopCard.EqualsTraits("ADVENTURE");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    level: 5,
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }
            #endregion

            #region Alliance and Evade
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.EvadeSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP / WD / WA

            string SharedHashString = "AD1_012_OP_WD_WA";

            string SharedEffectName = "You may return 1 lowest level opponent to hand. This and 1 [Greymon] may unsuspend";

            string SharedEffectDescription(string tag) 
                => $"[{tag}] [Once Per Turn] You may return 1 of your opponent's lowest level Digimon to the hand. Then, this Digimon and 1 of your Digimon with [Greymon] in its name may unsuspend.";

            bool SharedCanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleArea(card);

            bool IsOpponentsLowestLevel(Permanent permanent) => CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy);

            bool IsOwnGreymon(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.ContainsCardName("Greymon")
                    && permanent.IsSuspended;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsOpponentsLowestLevel))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOpponentsLowestLevel,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Bounce,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 opponent's Digimon to return to hand", "The opponent is selecting 1 Digimon to return to hand");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

                string selectPlayerMessage = "Will you unsuspend your digimon?";
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
                    List<Permanent> selectedPermanents = new List<Permanent>() { card.PermanentOfThisCard() };

                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsOwnGreymon))
                    {
                        SelectPermanentEffect selectPermanentEffect =
                                GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOwnGreymon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Greymon to unsuspend with this card.", "The opponent is selecting 1 Digimon to unsuspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanents.Add(permanent);
                            yield return null;
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(selectedPermanents, activateClass).Unsuspend());
                }
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition,(hash) => SharedActivateCoroutine(hash, activateClass), 1, true, SharedEffectDescription("On Play"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition,(hash) => SharedActivateCoroutine(hash, activateClass), 1, true, SharedEffectDescription("When Digivolving"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition,(hash) => SharedActivateCoroutine(hash, activateClass), 1, true, SharedEffectDescription("When Attacking"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
            }
            #endregion

            #region Opponent's Turn
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("You may DNA digivolve. Then you may change the attack target to 1 Digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("Redirect_AD1_012");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Opponent's Turn] [Once Per Turn] When one of your opponent's Digimon attacks, 2 of your Digimon may DNA digivolve into [Omnimon Alter-S] in the hand. Then, you may change the attack target to 1 of your Digimon.";

                bool IsOpponentDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOpponentTurn(card)
                        && CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, IsOpponentDigimon);
                }

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

                bool CanSelectDNACardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.CanPlayJogress(true)
                        && cardSource.EqualsCardName("Omnimon Alter-S");
                }

                bool IsOwnDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    #region DNA digivolve
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DNADigivolvePermanentsIntoHandOrTrashCard(
                            CanSelectDNACardCondition,
                            payCost: true,
                            isHand: true,
                            activateClass
                        ));
                    #endregion
                    #region redirect attack
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect =
                                GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOwnDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to change the attack target to.", "The opponent is selecting 1 Digimon to change the attack target to.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SwitchDefender(
                                activateClass,
                                false,
                                selectedPermanent));
                    }
                    #endregion
                }
            }
            #endregion

            #region Assembly
            if (timing == EffectTiming.None)
            {
                string cardName1 = "WereGarurumon: Sagittarius Mode";
                string cardName2 = "Garurumon";
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect($"Assembly", CanUseCondition, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        AssemblyConditionElement element1 = new AssemblyConditionElement(CanSelectCardCondition1, selectMessage: $"[{cardName1}]", elementCount: 1);
                        AssemblyConditionElement element2 = new AssemblyConditionElement(CanSelectCardCondition2, selectMessage: $"[{cardName2}]", elementCount: 1);

                        bool CanSelectCardCondition1(CardSource cardSource)
                        {
                            return cardSource != null && 
                                cardSource.Owner == card.Owner && 
                                cardSource.IsDigimon && 
                                cardSource.EqualsCardName(cardName1);
                        }

                        bool CanSelectCardCondition2(CardSource cardSource)
                        {
                            return cardSource != null && 
                                cardSource.Owner == card.Owner && 
                                cardSource.IsDigimon && 
                                cardSource.EqualsCardName(cardName2);
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            elements:new List<AssemblyConditionElement>() { element1, element2 },
                            reduceCost: 4);

                        return assemblyCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Your Turn - ESS
            if (timing == EffectTiming.None)
            {
                CanNotSwitchAttackTargetClass canNotSwitchAttackTargetClass = new CanNotSwitchAttackTargetClass();
                canNotSwitchAttackTargetClass.SetUpICardEffect("This Digimon's attack target can't be changed.", CanUseCondition, card);
                canNotSwitchAttackTargetClass.SetUpCanNotSwitchAttackTargetClass(PermanentCondition: PermanentCondition);
                canNotSwitchAttackTargetClass.SetIsInheritedEffect(true);
                cardEffects.Add(canNotSwitchAttackTargetClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent != null && permanent.TopCard && permanent == card.PermanentOfThisCard();
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
