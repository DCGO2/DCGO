using System.Collections;
using System.Collections.Generic;
using System.Linq;

// ScourgeChiropmon
namespace DCGO.CardEffects.BT27
{
    public class BT27_080 : CEntity_Effect
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

            #region Hand Main - Digivolve from Chiropmon
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [NightChiropmon] from trash under [Chiropmon] to digivolve into this card", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Hand] [Main] If you have a Tamer with [Makoto Kuonji] in its name, by placing 1 [NightChiropmon] from your trash as any of your [Chiropmon]'s bottom digivolution card, that Digimon digivolves into this card, ignoring level.";

                bool IsMakotoTamer(Permanent permanent)
                    => permanent.IsTamer && permanent.TopCard.ContainsCardName("Makoto Kuonji");

                bool IsChiropmon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && !permanent.IsToken
                        && permanent.TopCard.EqualsCardName("Chiropmon");

                bool IsNightChiropmon(CardSource cardSource)
                    => cardSource.EqualsCardName("NightChiropmon");

                bool IsThisCard(CardSource cardSource)
                    => CardEffectCommons.IsExistOnHand(cardSource) && cardSource == card;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnHand(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsMakotoTamer)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsChiropmon)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsNightChiropmon);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource nightChiropmon = null;
                    Permanent chiropmon = null;

                    #region Select NightChiropmon from trash
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: IsNightChiropmon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => false,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 [NightChiropmon] to place as a digivolution card.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 [NightChiropmon] to place as a digivolution card.", "The opponent is selecting 1 [NightChiropmon] to place as a digivolution card.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        nightChiropmon = cardSource;
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    #endregion

                    #region Select Chiropmon
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsChiropmon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 [Chiropmon] to digivolve.", "The opponent is selecting 1 [Chiropmon] to digivolve.");

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        chiropmon = permanent;
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    #endregion

                    if (nightChiropmon == null || chiropmon == null) yield break;

                    yield return ContinuousController.instance.StartCoroutine(
                        chiropmon.AddDigivolutionCardsBottom(new List<CardSource>() { nightChiropmon }, activateClass));

                    if (chiropmon.DigivolutionCards.Contains(nightChiropmon))
                    {
                        // Normal digivolution cost, ignoring level only.
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

            #region Shared On Play / When Digivolving

            string SharedEffectName = "Delete 1 opponent's level 4 or lower Digimon, then by trashing 1 Tamer face-down card, gain <Rush> and <Piercing>";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Delete 1 of your opponent's level 4 or lower Digimon. Then, by trashing the bottom face-down card from under any of your Tamers, this Digimon gains <Rush> and <Piercing> for the turn.";

            bool CanSelectDeleteCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.HasLevel
                    && permanent.Level <= 4;

            bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;

            bool TamerWithFaceDownSource(Permanent permanent, ICardEffect activateClass)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.DigivolutionCards.Any(FaceDownCards)
                    && !permanent.ImmuneFromStackTrashing(activateClass);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Delete
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectDeleteCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
                #endregion

                #region By trashing 1 Tamer face-down card, gain <Rush> and <Piercing>
                if (!CardEffectCommons.HasMatchConditionPermanent(permanent => TamerWithFaceDownSource(permanent, activateClass))) yield break;

                Permanent selectedTamer = null;

                SelectPermanentEffect selectTamerEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectTamerEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: permanent => TamerWithFaceDownSource(permanent, activateClass),
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectTamerCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectTamerEffect.SetUpCustomMessage("Select 1 Tamer to trash its bottom face-down card.", "The opponent is selecting 1 Tamer to trash its bottom face-down card.");

                yield return ContinuousController.instance.StartCoroutine(selectTamerEffect.Activate());

                IEnumerator SelectTamerCoroutine(Permanent permanent)
                {
                    selectedTamer = permanent;
                    yield return null;
                }

                if (selectedTamer == null) yield break;

                int countBefore = selectedTamer.DigivolutionCards.Count(FaceDownCards);

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                    targetPermanent: selectedTamer,
                    trashCount: 1,
                    isFromTop: false,
                    activateClass: activateClass,
                    cardCondition: FaceDownCards));

                Permanent thisPermanent = card.PermanentOfThisCard();

                if (selectedTamer.DigivolutionCards.Count(FaceDownCards) < countBefore && thisPermanent != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRush(
                        targetPermanent: thisPermanent,
                        effectDuration: EffectDuration.UntilEachTurnEnd,
                        activateClass: activateClass));

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainPierce(
                        targetPermanent: thisPermanent,
                        effectDuration: EffectDuration.UntilEachTurnEnd,
                        activateClass: activateClass));
                }
                #endregion
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            #endregion

            #region On Deletion - ESS
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 play cost 5 or lower [Glowing Dawn] trait card from trash for free", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Deletion] You may play 1 play cost 5 or lower [Glowing Dawn] trait card from your trash without paying the cost.";

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.HasGlowingDawnTraits
                        && cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 5
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.Trash);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.CanActivateOnDeletion(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                        canTargetCondition: CanSelectCardCondition,
                        root: SelectCardEffect.Root.Trash,
                        cardEffect: activateClass,
                        payCost: false));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
