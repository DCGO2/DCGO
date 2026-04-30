using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Firamon
namespace DCGO.CardEffects.BT25
{
    public class BT25_013 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 3, permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "May trash 1 card to recover 1 red or blue [Iliad] trait Digimon card from trash";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                isSkippable: true,
                onPlay: true,
                whenDigivolving: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] By trashing 1 card in your hand, you may return 1 red or blue [Iliad] trait Digimon card from your trash to the hand.";
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (card.Owner.HandCards.Count >= 1)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: _ => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            return cardSource.IsDigimon
                                && cardSource.HasTSTraits
                                && (cardSource.CardColors.Contains(CardColor.Red)
                                    || cardSource.CardColors.Contains(CardColor.Blue));
                        }

                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            int maxCount = Math.Min(1, card.Owner.TrashCards.Count(CanSelectCardCondition));

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to add to your hand.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.AddHand,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }
                    }
                }
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [Apollomon] in hand for 2 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() => "[Your Turn] When your Digimon are played or digivolve, if any of them are blue, this Digimon may digivolve into [Flaremon] in the hand with the cost reduced by 1.";
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition)
                            || CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Flaremon")
                        && cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> playedPermanents = new List<Permanent>();

                    foreach (Hashtable hash in CardEffectCommons.GetHashtablesFromHashtable(hashtable))
                    {
                        playedPermanents.Add(CardEffectCommons.GetPermanentFromHashtable(hash));
                    }

                    bool PermanentCondition1(Permanent permanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                            && permanent.TopCard.CardColors.Contains(CardColor.Blue);
                    }

                    List<Permanent> targetPermanents = playedPermanents.Filter(PermanentCondition1);

                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, targetPermanents.Contains))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: card.PermanentOfThisCard(),
                            cardCondition: CanSelectCardCondition,
                            payCost: true,
                            reduceCostTuple: (reduceCost: 1, reduceCostCardCondition: null),
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null));
                    }
                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 2000,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}
