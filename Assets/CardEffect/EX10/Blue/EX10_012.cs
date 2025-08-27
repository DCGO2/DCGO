using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.EX10
{
    public class EX10_012 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Hand - Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play for reduced cost of 5, delete at end of turn", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsDigimonEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Hand] [Main] If you don't have any Digimon other than Digimon with [Dark Masters] in their texts, you may play this card with the play cost reduced by 5. At turn end, delete the Digimon this effect played.";
                }

                bool IsDarkMaster(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(targetPermanent, card) &&
                           !targetPermanent.TopCard.HasText("Dark Masters");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnHand(card) &&
                           CardEffectCommons.MatchConditionPermanentCount(IsDarkMaster) == 0;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    #region reduce play cost
                    if (card.Owner.CanReduceCost(null, card))
                        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect($"Play Cost -5", CanUseCondition1, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                    card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(hashtable));

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return true;
                    }

                    int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                    {
                        if (CardSourceCondition(cardSource))
                        {
                            if (RootCondition(root))
                            {
                                if (PermanentsCondition(targetPermanents))
                                {
                                    Cost -= 5;
                                }
                            }
                        }

                        return Cost;
                    }

                    bool PermanentsCondition(List<Permanent> targetPermanents)
                    {
                        if (targetPermanents == null)
                        {
                            return true;
                        }

                        else
                        {
                            if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    bool CardSourceCondition(CardSource cardSource)
                    {
                        return true;
                    }

                    bool RootCondition(SelectCardEffect.Root root)
                    {
                        return true;
                    }

                    bool isUpDown()
                    {
                        return true;
                    }
                    #endregion

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                            cardSources: new List<CardSource> { card },
                                            activateClass: activateClass,
                                            payCost: true,
                                            isTapped: false,
                                            root: SelectCardEffect.Root.Hand,
                                            activateETB: true));

                    #region Delete Digimon Played
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    ActivateClass activateClass1 = new ActivateClass();
                    activateClass1.SetUpICardEffect("Delete this Digimon", CanUseCondition2, selectedPermanent.TopCard);
                    activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                    activateClass1.SetEffectSourcePermanent(selectedPermanent);
                    selectedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                    if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                    }

                    string EffectDiscription1()
                    {
                        return "[End of Your Turn] Delete this Digimon.";
                    }

                    bool CanUseCondition2(Hashtable hashtable1)
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(selectedPermanent, selectedPermanent.TopCard))
                            {
                                if (CardEffectCommons.CanTriggerOnEndAttack(hashtable1, selectedPermanent.TopCard))
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }

                    bool CanActivateCondition1(Hashtable hashtable1)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                        {
                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                        {
                            yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                            new List<Permanent>() { selectedPermanent },
                            CardEffectCommons.CardEffectHashtable(activateClass1)).Destroy());
                        }
                    }

                    ICardEffect GetCardEffect(EffectTiming _timing)
                    {
                        if (_timing == EffectTiming.OnEndTurn)
                        {
                            return activateClass1;
                        }

                        return null;
                    }
                    #endregion
                }
            }
            #endregion

            #region On Play/When Attacking Shared

            bool CanSelectDigimonCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanSelectTamerCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                {
                    if (permanent.IsTamer)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanActivateSharedCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 Digimon and 1 Tamer cant suspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] 1 of your opponent's Digimon and 1 of their Tamers can't suspend until their turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDigimonCondition))
                    {
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDigimonCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get unable to suspend.", "The opponent is selecting 1 Digimon that will get unable to suspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTamerCondition))
                    {
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectTamerCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get unable to suspend.", "The opponent is selecting 1 Digimon that will get unable to suspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        Permanent selectedPermanent = permanent;

                        if (selectedPermanent != null)
                        {
                            CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                            canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCondition1, card);
                            canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCondition);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => canNotSuspendClass);

                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                            }

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                if (selectedPermanent.TopCard != null)
                                {
                                    if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            bool PermanentCondition(Permanent permanent)
                            {
                                if (permanent == selectedPermanent)
                                {
                                    return true;
                                }

                                return false;
                            }
                        }
                    }
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 Digimon and 1 Tamer cant suspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] 1 of your opponent's Digimon and 1 of their Tamers can't suspend until their turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDigimonCondition))
                    {
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDigimonCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get unable to suspend.", "The opponent is selecting 1 Digimon that will get unable to suspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTamerCondition))
                    {
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectTamerCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get unable to suspend.", "The opponent is selecting 1 Digimon that will get unable to suspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        Permanent selectedPermanent = permanent;

                        if (selectedPermanent != null)
                        {
                            CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                            canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCondition1, card);
                            canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCondition);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => canNotSuspendClass);

                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                            }

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                if (selectedPermanent.TopCard != null)
                                {
                                    if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            bool PermanentCondition(Permanent permanent)
                            {
                                if (permanent == selectedPermanent)
                                {
                                    return true;
                                }

                                return false;
                            }
                        }
                    }
                }
            }

            #endregion

            #region All Turns
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Apocalymon");
                }

                cardEffects.Add(CardEffectFactory.CanNotDigivolveStaticSelfEffect(
                cardCondition: CardCondition,
                isInheritedEffect: false,
                card: card,
                condition: Condition,
                effectName: "[All Turns] This Digimon can only digivolve into [Apocalymon].")
                );
            }
            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place this Digimon face up as bottom security, add top security to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] If you have no black face-up security cards, place this Digimon face up as the bottom security card.";
                }

                bool FaceUpGreen(CardSource card)
                {
                    return card.IsFlipped &&
                           card.CardColors.Contains(CardColor.Black);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card) &&
                           !CardEffectCommons.HasMatchConditionOwnersSecurity(card, FaceUpGreen);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(card, false, true));
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Play 1 Level 5 or lower", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] If this card was face-up, you may play 1 level 5 or lower card with [Dark Masters] in its text from your hand or trash without paying the cost.";
                }
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}