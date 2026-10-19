using System.Collections;
using System.Collections.Generic;
using System.Linq;

// VictoryGreymon // Dramon Breaker
namespace DCGO.CardEffects.EX14
{
    public class EX14_012 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Effects
            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("TERMINAL");
                }

                cardEffects.Add(
                    CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                        permanentCondition: PermanentCondition, 
                        digivolutionCost: 3, 
                        ignoreDigivolutionRequirement: false, 
                        card: card, 
                        condition: null, 
                        level: 5));
            }
            #endregion

            #region Security A. +1
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Engage
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.EngageSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared When Digivolving / When Attacking
            string SharedEffectName()
                => "1 Digimon +3000 DP, then by trashing a Tamer's bottom face-down card, delete 1 opponent's Digimon with DP up to this Digimon's";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] 1 of your Digimon gets +3000 DP until the end of your opponent's turn. Then, by trashing the bottom face-down card from under any of your Tamers, delete 1 of your opponent's Digimon with as much DP as this Digimon or less.";

            bool IsOwnerDigimon(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

            bool IsTamerWithFaceDownCard(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.HasFaceDownDigivolutionCards;

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool CanTrashFaceDownCard(CardSource cardSource)
                    => cardSource.IsFaceDown
                        && !cardSource.CanNotTrashFromDigivolutionCards(activateClass);

                bool CanSelectDeleteTargetCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && card.PermanentOfThisCard() != null
                        && permanent.HasDP
                        && card.PermanentOfThisCard().HasDP
                        && permanent.DP <= card.PermanentOfThisCard().DP;

                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsOwnerDigimon))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOwnerDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: 3000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                    }

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP +3000.", "The opponent is selecting 1 Digimon that will get DP +3000.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsTamerWithFaceDownCard))
                {
                    bool trashed = false;

                    SelectPermanentEffect selectTamerEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectTamerEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsTamerWithFaceDownCard,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        if (permanents.Count == 1)
                        {
                            CardSource bottomFaceDownCard = permanents[0].DigivolutionCards.LastOrDefault(CanTrashFaceDownCard);

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                                targetPermanent: permanents[0],
                                trashCount: 1,
                                isFromTop: false,
                                activateClass: activateClass,
                                cardCondition: CanTrashFaceDownCard));

                            trashed = bottomFaceDownCard != null && !permanents[0].DigivolutionCards.Contains(bottomFaceDownCard);
                        }
                    }

                    selectTamerEffect.SetUpCustomMessage("Select 1 Tamer to trash the bottom face-down card from.", "The opponent is selecting 1 Tamer to trash the bottom face-down card from.");

                    yield return ContinuousController.instance.StartCoroutine(selectTamerEffect.Activate());

                    if (trashed && CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteTargetCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDeleteTargetCondition,
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
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: 1,
                hashValue: "EX14_012_WD_WA",
                whenDigivolving: true,
                whenAttacking: true);
            #endregion

            #region Shared Security / On Deletion
            string UseSelfEffectName()
                => "Use this card without paying the cost";

            string UseSelfEffectDescription(string tag)
                => $"[{tag}] You may use this card without paying the cost.";

            bool CanUseSelf(Hashtable hashtable, ActivateClass activateClass)
                => (CardEffectCommons.IsExistOnTrash(card) || card.Owner.ExecutingCards.Contains(card))
                    && !card.CanNotPlayThisOption;

            IEnumerator UseSelfActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                    cardSources: new List<CardSource>() { card },
                    activateClass: activateClass,
                    payCost: false,
                    root: CardEffectCommons.IsExistOnTrash(card) ? SelectCardEffect.Root.Trash : SelectCardEffect.Root.Execution));
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                UseSelfEffectName(),
                UseSelfActivateCoroutine,
                UseSelfEffectDescription,
                optional: true,
                additionalActivateCondition: CanUseSelf,
                onDeletion: true,
                security: true);
            #endregion
            #endregion

            #region Option Effects
            #region Use Req.
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource)
                    => cardSource.EqualsTraits("TERMINAL");
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete up to 15000 DP total of opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] Delete up to 15000 DP total worth of your opponent's Digimon.";

                int MaxTotalDP()
                    => card.Owner.MaxDP_DeleteEffect(15000, activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.DP <= MaxTotalDP();

                int SumDP(List<Permanent> permanents)
                {
                    int sumDP = 0;

                    foreach (Permanent permanent in permanents)
                    {
                        sumDP += permanent.DP;
                    }

                    return sumDP;
                }

                bool CanTargetConditionByPreSelectedList(List<Permanent> permanents, Permanent permanent)
                    => SumDP(permanents) + permanent.DP <= MaxTotalDP();

                bool CanEndSelectCondition(List<Permanent> permanents)
                    => permanents.Count >= 1
                        && SumDP(permanents) <= MaxTotalDP();

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: CanTargetConditionByPreSelectedList,
                            canEndSelectCondition: CanEndSelectCondition,
                            maxCount: CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition),
                            canNoSelect: false,
                            canEndNotMax: true,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select Digimon to delete.", "The opponent is selecting Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            #region Arts Digivolution
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }
            #endregion
            #endregion

            return cardEffects;
        }
    }
}
