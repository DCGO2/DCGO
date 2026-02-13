using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX11
{
    // GrandGalemon
    public class EX11_032 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Hand Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [Galemon] from trash under 1 [Pteromon], to digivolve for 3", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() 
                    => "[Hand] [Main] If you have [Shoto Kazama], by placing 1 [Galemon] from your trash as your [Pteromon]'s bottom digivolution card, it digivolves into this card for a digivolution cost of 3, ignoring digivolution requirements.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnHand(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsShoto)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsPteromon)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsGalemon);
                }

                bool IsShoto(Permanent permanent)
                {
                    return permanent.TopCard.EqualsCardName("Shoto Kazama");
                }

                bool IsPteromon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.EqualsCardName("Pteromon");
                }

                bool IsGalemon(CardSource source)
                {
                    return source.IsDigimon && source.EqualsCardName("Galemon");
                }

                bool IsGrandGalemon(CardSource source)
                {
                    return CardEffectCommons.IsExistOnHand(source) && source == card;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsShoto))
                    {
                        Permanent pteromon = null;
                        CardSource galemon = null;

                        #region Select Galemon From Trash

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, IsGalemon));
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                        selectCardEffect.SetUp(
                                    canTargetCondition: IsGalemon,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 [Galemon] to add as digivolution source",
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
                            galemon = cardSource;
                            yield return null;
                        }

                        selectCardEffect.SetUpCustomMessage("Select 1 [Galemon] to add as digivolution source.", "The opponent is selecting 1 [Galemon] to add as digivolution source.");
                        yield return StartCoroutine(selectCardEffect.Activate());

                        #endregion

                        if (galemon != null)
                        {
                            #region Select Pteromon Permanent

                            int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, IsPteromon));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsPteromon,
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
                                pteromon = permanent;
                                yield return null;
                            }

                            selectPermanentEffect.SetUpCustomMessage("Select 1 [Pteromon] to add digivolution sources.", "The opponent is selecting 1 [Pteromon] to add digivolution sources.");
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            #endregion

                            if (pteromon != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(pteromon.AddDigivolutionCardsBottom(new List<CardSource>() { galemon }, activateClass));
                                if (pteromon.DigivolutionCards.Contains(galemon))
                                {
                                    #region Digivolve into GrandGalemon

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                        pteromon,
                                        IsGrandGalemon,
                                        payCost: true,
                                        reduceCostTuple: null,
                                        fixedCostTuple: null,
                                        ignoreDigivolutionRequirementFixedCost: 3,
                                        isHand: true,
                                        activateClass: activateClass,
                                        successProcess: null,
                                        ignoreSelection: true,
                                        failedProcess: FailureProcess(),
                                        isOptional: false));

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

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May Suspend 1 Digimon. May play 1 [Avian] or [Bird] in traits with 3k DP + 1k per suspended digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() 
                    => "[When Digivolving] You may suspend 1 Digimon. Then, you may play 1 3000 DP or lower green Digimon card with [Avian] or [Bird] in any of its traits from your hand without paying the cost. For each suspended Digimon, add 1000 to this effect's DP maximum.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleArea(card);

                bool CanSuspendPermanentCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);

                bool CanPlayBirdCondition(CardSource cardSource, int maxDP)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasBirdTraits
                        && cardSource.HasDP
                        && cardSource.CardDP <= maxDP;
                }

                bool IsSuspendedPermanent(Permanent permanent) => permanent.IsDigimon && permanent.IsSuspended;

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSuspendPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSuspendPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    int maxDP = 3000 + (1000 * CardEffectCommons.MatchConditionPermanentCount(IsSuspendedPermanent));

                    if (card.Owner.HandCards.Count(cardSource => CanPlayBirdCondition(cardSource, maxDP)) >= 1)
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: cardSource => CanPlayBirdCondition(cardSource, maxDP),
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                            yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

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
                                root: SelectCardEffect.Root.Hand,
                                activateETB: true));
                        }
                }
            }
            #endregion

            #region ESS - Win Battle
            if (timing == EffectTiming.OnEndBattle)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May Unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("EX11_032_ESS_Unsuspend");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Your Turn] [Once Per Turn] When this Digimon wins a battle, this [Vortex Warriors] trait Digimon may unsuspend.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            bool WinnerCondition(Permanent permanent) => permanent.cardSources.Contains(card);

                            if (CardEffectCommons.CanTriggerWhenWinBattle(
                                hashtable: hashtable,
                                winnerCondition: WinnerCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.PermanentOfThisCard().TopCard.EqualsTraits("Vortex Warriors")
                        && card.PermanentOfThisCard().IsSuspended;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
