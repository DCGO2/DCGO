using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Grandracmon
namespace DCGO.CardEffects.BT23
{
    public class BT23_068 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.IsLevel5
                        && (targetPermanent.TopCard.HasCSTraits || targetPermanent.TopCard.HasUndeadTraits);
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 4,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region SoM/OD Shared

            const string SharedEffectDescription = "1 of your Digimon may digivolve into a level 6 or lower [Undead] or [Dark Animal] trait Digimon card in the trash without paying the cost.";

            bool SharedCanSelectDigiCardCondition(CardSource cardSource)
            {
                return cardSource.IsDigimon &&
                       cardSource.HasLevel && cardSource.Level <= 6 &&
                       (cardSource.HasUndeadTraits || cardSource.HasDarkAnimalTraits);
            }

            bool SharedCanSelectPermanentCondition(Permanent permanent, ActivateClass activateClass)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    // Loop thru the trash cards looking for a valid digi target
                    foreach (CardSource cardSource in card.Owner.TrashCards)
                    {
                        // does it have undead or dark animal trait?
                        if (SharedCanSelectDigiCardCondition(cardSource))
                        {
                            // can this digimon digivolve into it?
                            if (cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, true, activateClass))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent selectedPermanent = null;
                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: (permanent) => SharedCanSelectPermanentCondition(permanent, activateClass),
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to digivolve.", "The opponent is selecting 1 Digimon to digivolve.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    selectedPermanent = permanent;

                    yield return null;
                }

                bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, SharedCanSelectDigiCardCondition);

                if (canSelectTrash)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: selectedPermanent,
                                cardCondition: SharedCanSelectDigiCardCondition,
                                payCost: false,
                                reduceCostTuple: null,
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: false,
                                activateClass: activateClass,
                                successProcess: null));
                }
            }

            #endregion

            #region Start of Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into level 6 or lower", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, (hashtable) => SharedActivateCoroutine(hashtable, activateClass), -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of your Main Phase] " + SharedEffectDescription;
                }

                bool CanUseCondition(Hashtable hashtable)
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

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent((permanent) => SharedCanSelectPermanentCondition(permanent, activateClass)))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
            #endregion

            #region On Deletion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into level 6 or lower", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, (hashtable) => SharedActivateCoroutine(hashtable, activateClass), -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] " + SharedEffectDescription;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanActivateOnDeletion(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent((permanent) => SharedCanSelectPermanentCondition(permanent, activateClass)))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 level 4 or lower Digimon from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may play 1 level 4 or lower purple Digimon card from your trash without paying the cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.CardColors.Contains(CardColor.Purple))
                        {
                            if (cardSource.HasLevel && cardSource.Level <= 4)
                            {
                                if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 1;

                        if (card.Owner.TrashCards.Count(CanSelectCardCondition) <= maxCount)
                        {
                            maxCount = card.Owner.TrashCards.Count(CanSelectCardCondition);
                        }

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 card to play.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Trash,
                            activateETB: true));
                    }
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete opponent's lowest level Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("AT_BT23-068");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns][Once Per Turn] When any of your Digimon digivolve from the trash, delete all of your opponent's lowest level Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, DigivolveToPermanentCondition, TrashRootCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool TrashRootCondition(SelectCardEffect.Root root)
                {
                    return root == SelectCardEffect.Root.Trash;
                }

                bool DigivolveToPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool OpponentMinLevelPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> destroyTargetPermanents =
                            card.Owner.Enemy.GetBattleAreaDigimons().Filter(OpponentMinLevelPermanentCondition);

                    yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                        destroyTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}