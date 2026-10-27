using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Monarchlizamon
namespace DCGO.CardEffects.BT27
{
    public class BT27_050 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - Lv.4 w/[Glowing Dawn] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasGlowingDawnTraits;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 4));
            }
            #endregion

            #region Hand Main Warp
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [Armalizamon] from trash under [Gekkomon] to digivolve into this card", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Hand] [Main] If you have a Tamer with [Tomoro Tenma] in its name, by placing 1 [Armalizamon] from your trash as any of your [Gekkomon]'s bottom digivolution card, that Digimon digivolves into this card, ignoring level.";

                bool IsMakotoTamer(Permanent permanent)
                    => permanent.IsTamer && permanent.TopCard.ContainsCardName("Tomoro Tenma");

                bool IsGekkomon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && !permanent.IsToken
                        && permanent.TopCard.EqualsCardName("Gekkomon");

                bool IsArmalizamon(CardSource cardSource)
                    => cardSource.EqualsCardName("Armalizamon");

                bool IsThisCard(CardSource cardSource)
                    => CardEffectCommons.IsExistOnHand(cardSource) && cardSource == card;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnHand(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsMakotoTamer)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsGekkomon)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsArmalizamon);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource nightGekkomon = null;
                    Permanent chiropmon = null;

                    #region Select Armalizamon from trash
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: IsArmalizamon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 [Armalizamon] to place as a digivolution card.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 [Armalizamon] to place as a digivolution card.", "The opponent is selecting 1 [Armalizamon] to place as a digivolution card.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        nightGekkomon = cardSource;
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    #endregion

                    if (nightGekkomon == null) yield break;

                    #region Select Gekkomon
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsGekkomon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 [Gekkomon] to digivolve.", "The opponent is selecting 1 [Gekkomon] to digivolve.");

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        chiropmon = permanent;
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    #endregion

                    if (chiropmon == null) yield break;

                    yield return ContinuousController.instance.StartCoroutine(
                        chiropmon.AddDigivolutionCardsBottom(new List<CardSource>() { nightGekkomon }, activateClass));

                    if (chiropmon.DigivolutionCards.Contains(nightGekkomon))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: chiropmon,
                            cardCondition: IsThisCard,
                            payCost: true,
                            reduceCostTuple: null,
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null,
                            ignoreSelection: true,
                            ignoreRequirements: CardEffectCommons.IgnoreRequirement.Level,
                            failedProcess: null,
                            isOptional: false));
                    }
                }
            }
            #endregion

            #region Shared Code
            bool IsTamerWithFaceDownCard(Permanent permanent, ActivateClass activateClass)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.HasFaceDownDigivolutionCards
                        && !permanent.ImmuneFromStackTrashing(activateClass);

            bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;
            #endregion

            #region Shared On Play / When Digivolving
            string SharedEffectName()
                => "May suspend 1 opponent's Digimon/Tamer, then 1 can't unsuspend";

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            string SharedEffectDescription(string tag)
                => $"[{tag}] You may suspend 1 of your opponent's Digimon or Tamers. Then, 1 of their Digimon or Tamers can't unsuspend until their turn ends.";

            bool CanSelectSuspendTargetCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer)
                    && !permanent.IsSuspended && permanent.CanSuspend;

            bool CanSelectCantUnsuspendCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendTargetCondition))
                {
                    SelectPermanentEffect selectSuspendEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectSuspendEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectSuspendTargetCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    selectSuspendEffect.SetUpCustomMessage("Select 1 Digimon or Tamer to suspend.", "The opponent is selecting 1 Digimon or Tamer to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectSuspendEffect.Activate());
                }

                Permanent selectedTamer = null;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: permanent => IsTamerWithFaceDownCard(permanent, activateClass),
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    selectedTamer = permanent;
                    yield return null;
                }

                selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer to trash its bottom face-down card.", "The opponent is selecting 1 Tamer to trash its bottom face-down card.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                if (selectedTamer != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedTamer, trashCount: 1, isFromTop: false, activateClass: activateClass, cardCondition: FaceDownCards));

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectCantUnsuspendCondition))
                    {
                        SelectPermanentEffect selectCantUnsuspendEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectCantUnsuspendEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCantUnsuspendCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine1,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectCantUnsuspendEffect.SetUpCustomMessage("Select 1 Digimon or Tamer that can't unsuspend.", "The opponent is selecting 1 Digimon or Tamer that can't unsuspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectCantUnsuspendEffect.Activate());

                        IEnumerator SelectPermanentCoroutine1(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotUnsuspend(permanent, EffectDuration.UntilOpponentTurnEnd, activateClass, null, "Can't unsuspend"));
                        }
                    }
                }
            }
            #endregion

            #region Inherit End of Attack OPT
            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing bottom face down of a tamer, this digimon with [Glowing Dawn] trait unsuspends", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT27_050_Inherited");
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription() => "[End of Attack] [Once Per Turn] By trashing the bottom face-down card from under any of your Tamers, this Digimon with the [Glowing Dawn] trait unsuspends.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnEndAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, permanent => IsTamerWithFaceDownCard(permanent, activateClass));
                }

                bool IsGlowingDawnDigimon(Permanent permanent) => permanent.TopCard.HasGlowingDawnTraits;

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool isUsed = false;
                    if (CardEffectCommons.HasMatchConditionPermanent(permanent => IsTamerWithFaceDownCard(permanent, activateClass)))
                    {
                        SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect1.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: permanent => IsTamerWithFaceDownCard(permanent, activateClass),
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect1.SetUpCustomMessage("Select 1 Tamer to trash 1 bottom face-down card from", "The opponent is selecting 1 Tamer to trash 1 bottom face-down card from");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanents[0], trashCount: 1, isFromTop: false, activateClass: activateClass, FaceDownCards));
                            isUsed = true;
                        }
                    }

                    if (isUsed && IsGlowingDawnDigimon(card.PermanentOfThisCard())) yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());

                    if (!isUsed) activateClass.RemoveUse();
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
