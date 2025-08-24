using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Monzaemon
namespace DCGO.CardEffects.BT22
{
    public class BT22_038 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Numemon Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Numemon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel4 && targetPermanent.TopCard.HasDMTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Ver1 Digivolution Cost Reduction

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reduce the digivolution cost by 1 for each face-down source", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "When any of your Digimon with the [Ver.1] trait would digivolve into this card, for each of its face-down digivolution cards, reduce the digivolution cost by 1.";

                bool PermanentEvoCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.HasVer1Traits)
                        {
                            return card.CanPlayCardTargetFrame(permanent.PermanentFrame, true, activateClass);
                        }
                    }
                    return false;
                }

                bool CardCondition(CardSource source)
                {
                    return (source == card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, PermanentEvoCondition))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(hashtable, PermanentEvoCondition, CardCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Hashtable hashtable = new Hashtable();
                    hashtable.Add("CardEffect", activateClass);

                    Permanent targetPermanent = CardEffectCommons.GetPermanentsFromHashtable(_hashtable)[0];

                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect($"Digivolution Cost -{ReduceCost()}", CanUseCondition, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                    card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                    bool CanUseCondition(Hashtable hashtable)
                    {
                        return true;
                    }

                    int ReduceCost()
                    {
                        return targetPermanent.DigivolutionCards.Filter(x => x.IsFlipped).Count;
                    }

                    int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                    {
                        if (CardSourceCondition(cardSource))
                        {
                            if (RootCondition(root))
                            {
                                if (PermanentsCondition(targetPermanents))
                                {
                                    Cost -= ReduceCost();
                                }
                            }
                        }

                        return Cost;
                    }

                    bool PermanentsCondition(List<Permanent> targetPermanents)
                    {
                        if (targetPermanents != null)
                        {
                            if (targetPermanents.Exists(PermanentCondition))
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    bool PermanentCondition(Permanent targetPermanent)
                    {
                        if (targetPermanent.TopCard != null)
                        {
                            return PermanentEvoCondition(targetPermanent);
                        }

                        return false;
                    }

                    bool CardSourceCondition(CardSource cardSource)
                    {
                        if (cardSource != null)
                        {
                            return CardCondition(cardSource);
                        }

                        return false;
                    }

                    bool RootCondition(SelectCardEffect.Root root)
                    {
                        return true;
                    }

                    bool isUpDown()
                    {
                        return true;
                    }
                }
            }

            #endregion

            #region Armour Purge

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted) cardEffects.Add(CardEffectFactory.ArmorPurgeEffect(card: card));

            #endregion

            #endregion

            #region Shared WD/WA

            bool SharedIsOpponentDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            bool SharedHasFDSource() => card.PermanentOfThisCard().DigivolutionCards.Filter(x => x.IsFlipped).Any();

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool isTrashingSource = false;

                if (SharedHasFDSource())
                {
                    #region Make Selection for Trashing Bottom FD Source

                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                        };

                    string selectPlayerMessage = "Trash bottom FD source?";
                    string notSelectPlayerMessage = "The opponent is choosing to trash bottom FD source";

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                    isTrashingSource = GManager.instance.userSelectionManager.SelectedBoolValue;

                    #endregion

                    if (isTrashingSource)
                    {
                        CardSource bottomFDSource = card.PermanentOfThisCard().DigivolutionCards.FindLast(x => x.IsFlipped);

                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(card.PermanentOfThisCard(), 1, false, activateClass, cs => cs.IsFlipped));

                        if (!card.PermanentOfThisCard().DigivolutionCards.Contains(bottomFDSource))
                        {
                            Permanent selectedPermanent = null;

                            #region Select Permament

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: SharedIsOpponentDigimon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;
                                yield return null;
                            }

                            selectPermanentEffect.SetUpCustomMessage(
                                "Select 1 Digimon that will be unable to activate [When Digivolving] effects & -4K DP.",
                                "The opponent is selecting 1 Digimon that will be unable to activate [When Digivolving] effects & -4K DP.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            #endregion

                            if (selectedPermanent != null)
                            {
                                #region Setup Disable When Digivolving

                                bool CanUseConditionDebuff(Hashtable hashtableDebuff)
                                {
                                    return true;
                                }

                                bool InvalidateCondition(ICardEffect cardEffect)
                                {
                                    if (selectedPermanent.TopCard != null)
                                    {
                                        if (cardEffect != null)
                                        {
                                            if (cardEffect.EffectSourceCard != null)
                                            {
                                                if (isExistOnField(cardEffect.EffectSourceCard))
                                                {
                                                    if (cardEffect.EffectSourceCard.PermanentOfThisCard() == selectedPermanent)
                                                    {
                                                        if (cardEffect.IsWhenDigivolving)
                                                        {
                                                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                                            {
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

                                DisableEffectClass invalidationClass = new DisableEffectClass();
                                invalidationClass.SetUpICardEffect("Ignore [When Digivolving] Effect", CanUseConditionDebuff, card);
                                invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);

                                #endregion

                                selectedPermanent.UntilOwnerTurnEndEffects.Add(_ => invalidationClass);

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                    targetPermanent: selectedPermanent,
                                    changeValue: -4000,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                    activateClass: activateClass
                                ));
                            }
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("by trashing bottom face down source, 1 digimon cant use When digivolving effects and get -4K DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT22_038_WDWA");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] [Once Per Turn] By trashing this Digimon's bottom face-down digivolution card, until your opponent's turn ends, 1 of their Digimon can't activate [When Digivolving] effects and gets -4000 DP.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, SharedIsOpponentDigimon)
                        && SharedHasFDSource();
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(SharedActivateCoroutine(hashtable, activateClass));
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("by trashing bottom face down source, 1 digimon cant use When digivolving effects and get -4K DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT22_038_WDWA");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] By trashing this Digimon's bottom face-down digivolution card, until your opponent's turn ends, 1 of their Digimon can't activate [When Digivolving] effects and gets -4000 DP.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, SharedIsOpponentDigimon)
                        && SharedHasFDSource();
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
                activateClass.SetUpICardEffect("-4K DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("BT22_038_WA");
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