using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Yuuki
namespace DCGO.CardEffects.EX11
{
    public class EX11_069 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Security Effect

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            #region SoYMP/OP Shared

            string SharedEffectName()
            {
                return "Trash a card to gain 1 memory.";
            }

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] By trashing 1 card in your hand, gain 1 memory.";
            }

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card)
                    && card.Owner.HandCards.Count >= 1;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool discarded = false;

                int discardCount = 1;

                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: (cardSource) => true,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: discardCount,
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
                    if (cardSources.Count >= 1)
                    {
                        discarded = true;

                        yield return null;
                    }
                }

                if (discarded)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            #endregion

            #region Start of Your Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("Start of Your Main Phase"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }

            #endregion

            #region Your Turn

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve attacking digimon to [Dark Dragon]/[Evil Dragon] trait for 1 less.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("EX11_069_YT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Your Turn] [Once Per Turn] When one of your Digimon attacks, if you have 4 or fewer cards in your hand, it may digivolve into a [Dark Dragon] or [Evil Dragon] trait Digimon card in the trash with the digivolution cost reduced by 1.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card)
                    && CardEffectCommons.IsOwnerTurn(card)
                    && CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, PermanentCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card)
                    && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition)
                    && card.Owner.HandCards.Count <= 4;

                bool PermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.IsDigimon
                    && (cardSource.EqualsTraits("Dark Dragon") || cardSource.EqualsTraits("Evil Dragon"));

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: GManager.instance.attackProcess.AttackingPermanent,
                            cardCondition: CanSelectCardCondition,
                            payCost: true,
                            reduceCostTuple: (reduceCost: 1, reduceCostCardCondition: null),
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: false,
                            activateClass: activateClass,
                            successProcess: null));
                }
            }

            #endregion

            #region End of All Turns

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend tamer to recover 1 [Evil], [Dark Dragon] or [Evil Dragon] trait card.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[End of All Turns] If you have 4 or fewer cards in your hand, by suspending this Tamer, you may return 1 [Evil], [Dark Dragon] or [Evil Dragon] trait card from your trash to the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && card.Owner.HandCards.Count <= 4;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("Evil")
                        || cardSource.EqualsTraits("Dark Dragon")
                        || cardSource.EqualsTraits("Evil Dragon");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() },
                            CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

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

            #endregion

            return cardEffects;
        }
    }
}
