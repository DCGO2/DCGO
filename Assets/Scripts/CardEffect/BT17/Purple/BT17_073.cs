using System.Collections;
using System.Collections.Generic;
using System;

namespace DCGO.CardEffects.BT17
{
    public class BT17_073 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Dorugoramon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false,
                    card: card, condition: null));
            }

            #endregion

            #region Trash - All Turns

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Digivolve into [Dorugoramon] into [DexDorugoramon] from your trash to prevent deletion",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetHashString("DigivolveIntoDex_BT17_073");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns] When one of your [Dorugoramon] would be deleted, by digivolving it into this card without paying the cost, prevent that deletion.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card) &&
                           CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, IsOwnerPermanentCondition);
                }

                bool IsOwnerPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.EqualsCardName("Dorugoramon") &&
                           card.CanPlayCardTargetFrame(permanent.PermanentFrame, false, activateClass, root: SelectCardEffect.Root.Trash);
                }

                bool IsOwnerPermanentToBeDeletedCondition(Permanent permanent)
                {
                    return IsOwnerPermanentCondition(permanent) && permanent.willBeRemoveField;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(IsOwnerPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsOwnerPermanentToBeDeletedCondition))
                    {
                        Permanent selectedPermanent = null;

                        int maxCount = Math.Min(1,
                            CardEffectCommons.MatchConditionPermanentCount(IsOwnerPermanentToBeDeletedCondition));

                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOwnerPermanentToBeDeletedCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage(
                            "Select 1 Digimon to digivolve.",
                            "The opponent is selecting 1 Digimon to digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: selectedPermanent,
                                cardCondition: null,
                                payCost: false,
                                reduceCostTuple: null,
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: false,
                                activateClass: activateClass,
                                successProcess: null,
                                ignoreSelection: true));

                            selectedPermanent.willBeRemoveField = false;
                            selectedPermanent.HideDeleteEffect();
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<De-Digivolve 3> and delete all of your opponent's Digimon with the lowest level",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] <De-Digivolve 3> 1 of your opponent's Digimon (Trash up to 3 cards from the top. You can't trash past level 3 cards). Then, if [Dorugoramon] is in this Digimon's digivolution cards or this card is digivolving from the trash, delete all of your opponent's Digimon with the lowest level.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool IsDoruCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           cardSource.EqualsCardName("Dorugoramon");
                }

                bool TrashRootCondition(SelectCardEffect.Root root)
                {
                    return root == SelectCardEffect.Root.Trash;
                }

                bool CanSelectOpponentPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool OpponentMinLevelPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectOpponentPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOpponentPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve.",
                            "The opponent is selecting 1 Digimon to De-Digivolve.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IDegeneration(permanent, 3, activateClass)
                                .Degeneration());
                        }
                    }

                    if (card.PermanentOfThisCard().DigivolutionCards.Some(IsDoruCardCondition) ||
                        CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card, TrashRootCondition))
                    {
                        List<Permanent> destroyTargetPermanents =
                            card.Owner.Enemy.GetBattleAreaDigimons().Filter(OpponentMinLevelPermanentCondition);

                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                            destroyTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                    }
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT17_073_Unsuspend");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] (Once Per Turn) When another Digimon is deleted, you may unsuspend this Digimon.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleArea(permanent) &&
                           permanent.IsDigimon &&
                           permanent != card.PermanentOfThisCard();
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard()) &&
                           CardEffectCommons.GetHashtablesFromHashtable(hashtable) is { Count: >= 1 };
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}