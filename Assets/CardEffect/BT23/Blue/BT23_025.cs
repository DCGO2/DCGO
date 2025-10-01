using System;
using System.Collections;
using System.Collections.Generic;

// MarineAngemon
namespace DCGO.CardEffects.BT23
{
    public class BT23_025 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();


            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel5 && targetPermanent.TopCard.HasCSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region Hand - Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Give 3 Digimon Sec-1", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Hand] [Main] If you have a Digimon or Tamer with the [CS] trait, by paying 5 cost, give 3 of your opponent's Digimon <Security A. -1> until their turn ends. Then, place this card as the top security card.";
                }

                bool IsCSDigimonOrTamer(Permanent permanent)
                {
                    bool isCSDigimon = CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                        permanent.TopCard.HasCSTraits;

                    bool isCSTamer = CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card) &&
                        permanent.TopCard.IsTamer &&
                        permanent.TopCard.HasCSTraits;

                    return isCSDigimon || isCSTamer;
                }

                bool IsOpponentDigimon(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    // Can use from your hand, if you have a CS digimon or tamer
                    return CardEffectCommons.IsExistOnHand(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsCSDigimonOrTamer);
                }   

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsCSDigimonOrTamer))
                    {
                        // First, select 3 opponent's digimon on the field
                        int maxCount = Math.Min(3, CardEffectCommons.MatchConditionPermanentCount(IsOpponentDigimon));

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

                        selectPermanentEffect.SetUpCustomMessage("Select 3 Digimon that will get Security Attack -1.", "The opponent is selecting 3 Digimon that will get Security Attack -1.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            // To the permanents selected, apply the Sec-1.
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(targetPermanent: permanent, changeValue: -1, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));

                            yield return null;
                        }

                        // Either way, pay the 5 cost, then place this card as the top security card.
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(-5, activateClass));

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(card, true, false));
                    }
                }
            }
            #endregion

            #region On Play/When Digivolving Shared

            bool IsLowestLevelOpponentDigimon(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy))
                    {
                        return true;
                    }
                }

                return false;
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 of your opponent's Digimon to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Play] Return 1 of your opponent's Digimon with the lowest level to the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(IsLowestLevelOpponentDigimon))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsLowestLevelOpponentDigimon))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsLowestLevelOpponentDigimon));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsLowestLevelOpponentDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
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

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 of your opponent's Digimon to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] Return 1 of your opponent's Digimon with the lowest level to the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(IsLowestLevelOpponentDigimon))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsLowestLevelOpponentDigimon))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsLowestLevelOpponentDigimon));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsLowestLevelOpponentDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
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

            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                // Ugh, I don't like doing this, but I think in order to delete the digimon played after the security battle, I have to be able to
                // activate the deletion effect from within the ActivateCoroutine of the function.  Instead of calling CardEffectFactory.PlaySelfDigimonAfterBattleSecurityEffect,
                // I need to copy the code from that function here, then modify the necessary sections...
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play this card at the end of the battle", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);

                string EffectDiscription()
                {
                    return "[Security] At the end of the battle, play this card without paying its memory cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnExecutingArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return null;

                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                    ActivateClass activateClass1 = new ActivateClass();
                    activateClass1.SetUpICardEffect("Play this card", CanUseCondition1, card);
                    activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                    card.Owner.UntilEndBattleEffects.Add(GetCardEffect1);

                    string EffectDiscription1()
                    {
                        return "Play this card without paying its memory cost.";
                    }

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return true;
                    }

                    bool CanActivateCondition1(Hashtable hashtable)
                    {
                        if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: card, payCost: false, cardEffect: activateClass1, root: SelectCardEffect.Root.Security))
                        {
                            if (!card.Owner.LibraryCards.Contains(card) && !card.Owner.SecurityCards.Contains(card))
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    ICardEffect GetCardEffect1(EffectTiming _timing)
                    {
                        if (_timing == EffectTiming.OnEndBattle)
                        {
                            return activateClass1;
                        }

                        return null;
                    }

                    IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                    {
                        if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: card, payCost: false, cardEffect: activateClass1, root: SelectCardEffect.Root.Security))
                        {
                            if (!card.Owner.LibraryCards.Contains(card) && !card.Owner.SecurityCards.Contains(card))
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: new List<CardSource>() { card },
                                    activateClass: activateClass1,
                                    payCost: false,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Security,
                                    activateETB: true));

                                #region Delete Played Digimon
                                Permanent selectedPermanent = card.PermanentOfThisCard();

                                ActivateClass activateClassDeleteSelf = new ActivateClass();
                                activateClassDeleteSelf.SetUpICardEffect("Delete this Digimon", CanUseDeleteSelfCondition, selectedPermanent.TopCard);
                                activateClassDeleteSelf.SetUpActivateClass(CanActivateDeleteSelfCondition, ActivateDeleteSelfCoroutine, -1, false, EffectDiscription2());
                                activateClassDeleteSelf.SetEffectSourcePermanent(selectedPermanent);
                                selectedPermanent.UntilOwnerTurnEndEffects.Add(GetDeleteSelfCardEffect);

                                if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass1))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                                }

                                string EffectDiscription2()
                                {
                                    return "[End of Your Turn] Delete this Digimon.";
                                }

                                bool CanUseDeleteSelfCondition(Hashtable hashtable2)
                                {
                                    if (CardEffectCommons.IsOpponentTurn(card))
                                    {
                                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(selectedPermanent, selectedPermanent.TopCard))
                                        {
                                            if (CardEffectCommons.CanTriggerOnEndAttack(hashtable2, selectedPermanent.TopCard))
                                            {
                                                return true;
                                            }
                                        }
                                    }

                                    return false;
                                }

                                bool CanActivateDeleteSelfCondition(Hashtable hashtable2)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                    {
                                        if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass1))
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                IEnumerator ActivateDeleteSelfCoroutine(Hashtable _hashtable2)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                                        new List<Permanent>() { selectedPermanent },
                                        CardEffectCommons.CardEffectHashtable(activateClassDeleteSelf)).Destroy());
                                    }
                                }

                                ICardEffect GetDeleteSelfCardEffect(EffectTiming _timing)
                                {
                                    if (_timing == EffectTiming.OnEndTurn)
                                    {
                                        return activateClassDeleteSelf;
                                    }

                                    return null;
                                }
                                #endregion
                            }
                        }
                    }                    
                }
            }
            #endregion

            return cardEffects;
        }
    }
}