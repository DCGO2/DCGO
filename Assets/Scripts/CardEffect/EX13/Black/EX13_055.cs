using System.Collections;
using System.Collections.Generic;

// Raptordramon
namespace DCGO.CardEffects.EX13
{
    public class EX13_055 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - [Dorumon]
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Dorumon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Alternate Digivolution Requirement - Lv.3 w/[Chronicle] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasChronicleTraits;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null,
                    level: 3));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName = "DP -3000";

            string SharedEffectDescription(string tag)
                => $"[{tag}] 1 of your opponent's Digimon gets -3000 DP for the turn.";

            bool CanSelectPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -3000.", "The opponent is selecting 1 Digimon that will get DP -3000.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            targetPermanent: permanent,
                            changeValue: -3000,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass));
                    }
                }
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

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into a [Chronicle] trait Digimon card from hand or trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] This Digimon may digivolve into a Digimon card with the [Chronicle] trait in the hand or trash.";

                bool DigivolveCardCondition(CardSource cardSource)
                    => cardSource.IsDigimon
                        && cardSource.HasChronicleTraits;

                bool CanDigivolveFromHandCondition(CardSource cardSource)
                    => DigivolveCardCondition(cardSource)
                        && cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, true, activateClass, SelectCardEffect.Root.Hand);

                bool CanDigivolveFromTrashCondition(CardSource cardSource)
                    => DigivolveCardCondition(cardSource)
                        && cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, true, activateClass, SelectCardEffect.Root.Trash);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, CanDigivolveFromHandCondition)
                            || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanDigivolveFromTrashCondition));

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanDigivolveFromHandCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanDigivolveFromTrashCondition);

                    if (!canSelectHand && !canSelectTrash) yield break;

                    bool isHand = canSelectHand;

                    if (canSelectHand && canSelectTrash)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new(message: "From hand", value: 1, spriteIndex: 0),
                            new(message: "From trash", value: 2, spriteIndex: 0),
                            new(message: "Don't digivolve", value: 3, spriteIndex: 1),
                        };

                        GManager.instance.userSelectionManager.SetIntSelection(
                            selectionElements: selectionElements,
                            selectPlayer: card.Owner,
                            selectPlayerMessage: "From which area will you digivolve?",
                            notSelectPlayerMessage: "The opponent is choosing from which area to select a card.");

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedIntValue == 3) yield break;

                        isHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: DigivolveCardCondition,
                        payCost: true,
                        reduceCostTuple: null,
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: isHand,
                        activateClass: activateClass,
                        successProcess: null));
                }
            }
            #endregion

            #region Barrier - ESS
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
