using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

//Phantomon
namespace DCGO.CardEffects.BT23
{
    public class BT23_065 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Hand Main

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [Bakemon] from trash under 1 [Ghostmon], to digivolve for 3", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Hand] [Main] If you have [Violet Inboots], by placing 1 [Bakemon] from your trash as any of your [Ghostmon]'s bottom digivolution card, it digivolves into this card for a digivolution cost of 3, ignoring digivolution requirements.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnHand(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsVioletInboots)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsGhostmon)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsBakemon);
                }

                bool IsVioletInboots(Permanent permanent)
                {
                    return permanent.IsTamer && permanent.TopCard.EqualsCardName("Violet Inboots");
                }

                bool IsGhostmon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.EqualsCardName("Ghostmon");
                }

                bool IsBakemon(CardSource source)
                {
                    return source.IsDigimon && source.EqualsCardName("Bakemon");
                }

                bool IsProganomon(CardSource cardSource)
                {
                    return CardEffectCommons.IsExistOnHand(cardSource) && cardSource == card;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsVioletInboots))
                    {
                        Permanent sunarizamon = null;
                        CardSource landramon = null;

                        #region Select Bakemon From Trash

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, IsBakemon));
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                        selectCardEffect.SetUp(
                                    canTargetCondition: IsBakemon,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 [Bakemon] to add as digivolution source",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            landramon = cardSource;
                            yield return null;
                        }

                        selectCardEffect.SetUpCustomMessage("Select 1 [Bakemon] to add as digivolution source.", "The opponent is selecting 1 [Bakemon] to add as digivolution source.");
                        yield return StartCoroutine(selectCardEffect.Activate());

                        #endregion

                        if (landramon != null)
                        {
                            #region Select Ghostmon Permanent

                            int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, IsGhostmon));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsGhostmon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                sunarizamon = permanent;
                                yield return null;
                            }

                            selectPermanentEffect.SetUpCustomMessage("Select 1 [Ghostmon] to add digivolution sources.", "The opponent is selecting 1 [Ghostmon] to add digivolution sources.");
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            #endregion

                            if (sunarizamon != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(sunarizamon.AddDigivolutionCardsBottom(new List<CardSource>() { landramon }, activateClass));
                                if (sunarizamon.DigivolutionCards.Contains(landramon))
                                {
                                    #region Digivolve into MarineBullmon

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                        sunarizamon,
                                        IsProganomon,
                                        payCost: true,
                                        reduceCostTuple: null,
                                        fixedCostTuple: null,
                                        ignoreDigivolutionRequirementFixedCost: 3,
                                        isHand: true,
                                        activateClass: activateClass,
                                        successProcess: null,
                                        ignoreSelection: true,
                                        failedProcess: FailureProcess(),
                                        isOptional:false));

                                    #endregion
                                }

                                IEnumerator FailureProcess()
                                {
                                    List<IDiscardHand> discardHands = new List<IDiscardHand>() { new IDiscardHand(card, null) };
                                    yield return ContinuousController.instance.StartCoroutine(new IDiscardHands(discardHands, null).DiscardHands());
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region On Deletion Shared
            string SharedEffectDiscription()
            {
                return "[On Deletion] You may play 1 level 4 or lower Digimon card with the [Ghost] trait from your trash without paying the cost.";
            }

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanActivateOnDeletion(card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool CanSelectCardCondition1(CardSource source)
                {
                    return source.HasGhostTraits &&
                           source.HasLevel && source.Level <= 4 &&
                           source.IsDigimon &&
                           CardEffectCommons.CanPlayAsNewPermanent(source, false, activateClass, SelectCardEffect.Root.Trash);
            }

                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition1))
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition1,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to play.",
                                maxCount: 1,
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

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Trash, activateETB: true));
                }
            }
            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 level 4 [Ghost] trait from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDiscription());
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }
            }
            #endregion

            #region On Deletion - ESS
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 level 4 [Ghost] trait from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }
            }
            #endregion

            return cardEffects;
        }
    }
}