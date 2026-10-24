using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

// ScourgeChiropmon — BT27-080
// Integration candidate; validate special hand-evolution cost in Unity.
namespace DCGO.CardEffects.BT27
{
    public class BT27_080 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            // [Digivolve] Lv.4 w/[Glowing Dawn] trait: Cost 3
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent permanent)
                    => permanent.TopCard.EqualsTraits("Glowing Dawn");
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false, card: card, condition: null, level: 4));
            }

            // [On Play] [When Digivolving] Delete 1 opposing level 4 or lower Digimon.
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 opposing level 4 or lower Digimon", CanUse, card);
                activateClass.SetUpActivateClass(CanActivate, Activate, -1, false,
                    "[On Play] [When Digivolving] Delete 1 of your opponent's level 4 or lower Digimon.");
                cardEffects.Add(activateClass);

                bool CanTarget(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasLevel && permanent.TopCard.Level <= 4;
                bool CanUse(Hashtable data)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && (CardEffectCommons.CanTriggerOnPlay(data, card)
                            || CardEffectCommons.CanTriggerWhenDigivolving(data, card));
                bool CanActivate(Hashtable data)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);
                IEnumerator Activate(Hashtable data)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanTarget))
                    {
                    SelectPermanentEffect selector = GManager.instance.GetComponent<SelectPermanentEffect>();
                    selector.SetUp(selectPlayer: card.Owner, canTargetCondition: CanTarget,
                        canTargetCondition_ByPreSelecetedList: null, canEndSelectCondition: null,
                        maxCount: 1, canNoSelect: false, canEndNotMax: false,
                        selectPermanentCoroutine: null, afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy, cardEffect: activateClass);
                    yield return ContinuousController.instance.StartCoroutine(selector.Activate());
                    }
                    yield return GrantKeywordsAfterTamerSource();
                }

                bool IsTamerWithSource(Permanent p) =>
                    CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(p, card)
                    && p.DigivolutionCards.Any(c => c.IsFaceDown);
                bool IsFaceDown(CardSource c) => c.IsFaceDown;
                IEnumerator GrantKeywordsAfterTamerSource()
                {
                    if (!CardEffectCommons.HasMatchConditionPermanent(IsTamerWithSource)) yield break;
                    Permanent tamer = null;
                    SelectPermanentEffect tamerSelector = GManager.instance.GetComponent<SelectPermanentEffect>();
                    tamerSelector.SetUp(selectPlayer: card.Owner, canTargetCondition: IsTamerWithSource,
                        canTargetCondition_ByPreSelecetedList: null, canEndSelectCondition: null,
                        maxCount: 1, canNoSelect: true, canEndNotMax: false,
                        selectPermanentCoroutine: OnTamer, afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom, cardEffect: activateClass);
                    IEnumerator OnTamer(Permanent p) { tamer = p; yield return null; }
                    yield return ContinuousController.instance.StartCoroutine(tamerSelector.Activate());
                    if (tamer == null) yield break;
                    int before = tamer.DigivolutionCards.Count;
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                            targetPermanent: tamer, trashCount: 1, isFromTop: false,
                            activateClass: activateClass, cardCondition: IsFaceDown));
                    if (tamer.DigivolutionCards.Count >= before || !CardEffectCommons.IsExistOnBattleAreaDigimon(card)) yield break;
                    Permanent self = card.PermanentOfThisCard();
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.GainRush(self, EffectDuration.UntilEachTurnEnd, activateClass));
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.GainPierce(self, EffectDuration.UntilEachTurnEnd, activateClass));
                }
            }

            // [Inherited] [On Deletion] Play 1 [Glowing Dawn] card costing <=5 from trash for free.
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 cost 5 or lower [Glowing Dawn] from trash", CanUse, card);
                activateClass.SetUpActivateClass(CanActivate, Activate, -1, false,
                    "[On Deletion] You may play 1 play cost 5 or lower [Glowing Dawn] trait card from your trash without paying the cost.");
                activateClass.SetIsSkippable(true);
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);
                bool Eligible(CardSource target)
                    => target.HasPlayCost && target.GetCostItself <= 5
                        && target.EqualsTraits("Glowing Dawn")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: target, payCost: false, cardEffect: activateClass);
                bool CanUse(Hashtable data)
                    => CardEffectCommons.CanTriggerOnDeletion(data, card, activateClass);
                bool CanActivate(Hashtable data)
                    => CardEffectCommons.CanActivateOnDeletion(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, Eligible);
                IEnumerator Activate(Hashtable data)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                        canTargetCondition: Eligible, root: SelectCardEffect.Root.Trash,
                        cardEffect: activateClass, payCost: false));
                }
            }

            // [Hand] [Main] — EX10_032 (Proganomon) selection/placement/evolution pattern.
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass handEffect = new ActivateClass();
                handEffect.SetUpICardEffect("Place NightChiropmon under Chiropmon to digivolve", CanUseHand, card);
                handEffect.SetUpActivateClass(null, ActivateHand, -1, false,
                    "[Hand] [Main] If you have a [Makoto Kuonji] Tamer, place [NightChiropmon] from trash under [Chiropmon] to digivolve into this card, ignoring level.");
                cardEffects.Add(handEffect);

                bool IsMakoto(Permanent p) => p.IsTamer && p.TopCard.ContainsCardName("Makoto Kuonji");
                bool IsChiropmon(Permanent p) => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(p, card)
                    && p.TopCard.EqualsCardName("Chiropmon");
                bool IsNightChiropmon(CardSource c) => c.IsDigimon && c.EqualsCardName("NightChiropmon");
                bool IsThisCard(CardSource c) => c == card && CardEffectCommons.IsExistOnHand(c);
                bool CanUseHand(Hashtable h) => CardEffectCommons.IsExistOnHand(card)
                    && CardEffectCommons.IsOwnerTurn(card)
                    && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsMakoto)
                    && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsChiropmon)
                    && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsNightChiropmon);

                IEnumerator ActivateHand(Hashtable h)
                {
                    CardSource source = null;
                    Permanent target = null;
                    SelectCardEffect sourceSelector = GManager.instance.GetComponent<SelectCardEffect>();
                    sourceSelector.SetUp(canTargetCondition: IsNightChiropmon,
                        canTargetCondition_ByPreSelecetedList: null, canEndSelectCondition: null,
                        canNoSelect: () => false, selectCardCoroutine: OnSource,
                        afterSelectCardCoroutine: null, message: "Select NightChiropmon from trash",
                        maxCount: 1, canEndNotMax: false, isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom, root: SelectCardEffect.Root.Trash,
                        customRootCardList: null, canLookReverseCard: true,
                        selectPlayer: card.Owner, cardEffect: handEffect);
                    IEnumerator OnSource(CardSource selected) { source = selected; yield return null; }
                    yield return StartCoroutine(sourceSelector.Activate());
                    if (source == null) yield break;

                    SelectPermanentEffect targetSelector = GManager.instance.GetComponent<SelectPermanentEffect>();
                    targetSelector.SetUp(selectPlayer: card.Owner, canTargetCondition: IsChiropmon,
                        canTargetCondition_ByPreSelecetedList: null, canEndSelectCondition: null,
                        maxCount: 1, canNoSelect: false, canEndNotMax: false,
                        selectPermanentCoroutine: OnTarget, afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom, cardEffect: handEffect);
                    IEnumerator OnTarget(Permanent selected) { target = selected; yield return null; }
                    yield return ContinuousController.instance.StartCoroutine(targetSelector.Activate());
                    if (target == null) yield break;
                    yield return ContinuousController.instance.StartCoroutine(
                        target.AddDigivolutionCardsBottom(new List<CardSource> { source }, handEffect));
                    if (!target.DigivolutionCards.Contains(source)) yield break;

                    // EX10 helper requires an ignore-requirements fixed cost.
                    // 3 is the printed alternate digivolution cost; verify intended hand effect cost.
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DigivolveIntoHandOrTrashCard(target, IsThisCard,
                            payCost: true, reduceCostTuple: null, fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: 3, isHand: true,
                            activateClass: handEffect, successProcess: null,
                            ignoreSelection: true, failedProcess: null, isOptional: false));
                }
            }
            return cardEffects;
        }
    }
}
