using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DCGO.CardEffects.EX10
{
    public class EX10_011 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Myotismon") &&
                           targetPermanent.TopCard.IsLevel5;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 5, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Trash - Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play for reduced cost of 11", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsDigimonEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Trash] [Main] By deleting 2 of your level 5 or higher Digimon with [Myotismon] in their texts, play this card with the play cost reduced by 11.";
                }

                bool IsLvl5Myotismon(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(targetPermanent, card) &&
                           targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level >= 5 &&
                           targetPermanent.TopCard.HasText("Myotismon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card) &&
                           CardEffectCommons.MatchConditionPermanentCount(IsLvl5Myotismon) >= 2 &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource: card, payCost: false, cardEffect: activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(IsLvl5Myotismon));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsLvl5Myotismon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectDigimon,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 2 Digimon to delete.", "The opponent is selecting 2 Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator AfterSelectDigimon(List<Permanent> permanents)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                            targetPermanents: permanents, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));
                    }                   

                    IEnumerator SuccessProcess()
                    {
                        #region reduce play cost
                        if (card.Owner.CanReduceCost(null, card))
                            ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect($"Play Cost -11", CanUseCondition1, card);
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
                                        Cost -= 11;
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
                                            root: SelectCardEffect.Root.Trash,
                                            activateETB: true));
                    }
                }
            }
            #endregion

            #region On Play/When Digivolving/When Attacking Shared

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent) &&
                       permanent != card.PermanentOfThisCard() &&
                       !permanent.IsSuspended;
            }

            bool CanActivateSharedCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 2 unsuspened Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, (hashTable) => SharedActivateCoroutine(hashTable, activateClass), 1, false, EffectDescription());
                activateClass.SetHashString("Delete2_EX10-011");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] [Once Per Turn] Delete 2 other unsuspended Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }

            #endregion

            #region When Digivlving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 2 unsuspened Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, (hashTable) => SharedActivateCoroutine(hashTable, activateClass), 1, false, EffectDescription());
                activateClass.SetHashString("Delete2_EX10-011");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] [Once Per Turn] Delete 2 other unsuspended Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 2 unsuspened Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, (hashTable) => SharedActivateCoroutine(hashTable, activateClass), 1, false, EffectDescription());
                activateClass.SetHashString("Delete2_EX10-011");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] [Once Per Turn] Delete 2 other unsuspended Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
            }

            #endregion

            #region All Turns
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash security, then return 1 to bottom deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("AllTurns_EX10-001");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When any of your other Digimon or Tamers are deleted, trash your opponent's top security card and return 1 of their Digimon with the lowest DP to the bottom of the deck.";
                }

                bool YourOtherPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsOwnerPermanent(permanent, card) &&
                           (permanent.IsDigimon || permanent.IsTamer);
                }

                bool OpponentsLowestDP(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           CardEffectCommons.IsMinDP(permanent, permanent.TopCard.Owner);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, YourOtherPermanent);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if(card.Owner.Enemy.SecurityCards.Count > 0)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                    player: card.Owner.Enemy,
                                    destroySecurityCount: 1,
                                    cardEffect: activateClass,
                                    fromTop: true).DestroySecurity());
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(OpponentsLowestDP))
                    {
                        SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect1.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentsLowestDP,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}