using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Kamemon — BT27-020
namespace DCGO.CardEffects.BT27
{
    public class BT27_020 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.IsLevel2
                        && permanent.TopCard.EqualsTraits("DATA SQUAD");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    level: 2,
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 0,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 eligible hand Digimon under a Digimon or DATA SQUAD Tamer, then Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Play] By placing 1 level 4 or lower [Aquatic] or [DATA SQUAD] trait Digimon card from your hand as any of your Digimon's bottom digivolution card, or face down under any of your [DATA SQUAD] trait Tamers, <Draw 1>.";

                bool EligibleHandCard(CardSource candidate)
                    => candidate.IsDigimon && candidate.HasLevel && candidate.Level <= 4
                        && (candidate.EqualsTraits("Aquatic") || candidate.EqualsTraits("DATA SQUAD"));

                bool OwnDigimon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool OwnDataSquadTamer(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.TopCard.EqualsTraits("DATA SQUAD");

                bool EligibleDestination(Permanent permanent)
                    => OwnDigimon(permanent) || OwnDataSquadTamer(permanent);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card)
                        && card.Owner.HandCards.Any(EligibleHandCard)
                        && CardEffectCommons.HasMatchConditionPermanent(EligibleDestination);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedCard = null;
                    SelectHandEffect handSelection = GManager.instance.GetComponent<SelectHandEffect>();
                    handSelection.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: EligibleHandCard,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: false,
                        selectCardCoroutine: OnSelectedHandCard,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Custom,
                        cardEffect: activateClass);
                    handSelection.SetUpCustomMessage("Select an eligible level 4 or lower Digimon from your hand.",
                        "The opponent is selecting a card from their hand.");
                    yield return ContinuousController.instance.StartCoroutine(handSelection.Activate());

                    IEnumerator OnSelectedHandCard(CardSource chosen)
                    {
                        selectedCard = chosen;
                        yield return null;
                    }

                    if (selectedCard == null)
                        yield break;

                    Permanent selectedDestination = null;
                    SelectPermanentEffect permanentSelection = GManager.instance.GetComponent<SelectPermanentEffect>();
                    permanentSelection.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: EligibleDestination,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: OnSelectedDestination,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);
                    permanentSelection.SetUpCustomMessage("Select your Digimon or a [DATA SQUAD] Tamer to place the card under.",
                        "The opponent is selecting a Digimon or Tamer.");
                    yield return ContinuousController.instance.StartCoroutine(permanentSelection.Activate());

                    IEnumerator OnSelectedDestination(Permanent chosen)
                    {
                        selectedDestination = chosen;
                        yield return null;
                    }

                    if (selectedDestination == null)
                        yield break;

                    bool faceDown = selectedDestination.IsTamer;
                    yield return ContinuousController.instance.StartCoroutine(
                        selectedDestination.AddDigivolutionCardsBottom(
                            new List<CardSource> { selectedCard }, activateClass, isFacedown: faceDown));
                    yield return ContinuousController.instance.StartCoroutine(
                        new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }
            #endregion

            #region Inherited — When Attacking, Once Per Turn
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash top or bottom opponent digivolution card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT27_020_Inherited");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] [Once Per Turn] Trash the top or bottom digivolution card of 1 of your opponent's Digimon.";

                bool EligibleOpponentDigimon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.DigivolutionCards.Any(source => !source.CanNotTrashFromDigivolutionCards(activateClass));

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionPermanent(EligibleOpponentDigimon);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedTarget = null;
                    SelectPermanentEffect permanentSelection = GManager.instance.GetComponent<SelectPermanentEffect>();
                    permanentSelection.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: EligibleOpponentDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: OnSelectedTarget,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);
                    permanentSelection.SetUpCustomMessage("Select 1 opposing Digimon with digivolution cards.",
                        "The opponent is selecting a Digimon to strip.");
                    yield return ContinuousController.instance.StartCoroutine(permanentSelection.Activate());

                    IEnumerator OnSelectedTarget(Permanent chosen)
                    {
                        selectedTarget = chosen;
                        yield return null;
                    }

                    if (selectedTarget == null || !EligibleOpponentDigimon(selectedTarget))
                        yield break;

                    List<SelectionElement<bool>> choices = new List<SelectionElement<bool>>
                    {
                        new SelectionElement<bool>(message: "Trash top digivolution card", value: true, spriteIndex: 0),
                        new SelectionElement<bool>(message: "Trash bottom digivolution card", value: false, spriteIndex: 1)
                    };
                    GManager.instance.userSelectionManager.SetBoolSelection(
                        selectionElements: choices,
                        selectPlayer: card.Owner,
                        selectPlayerMessage: "Trash the top or bottom digivolution card?",
                        notSelectPlayerMessage: "The opponent is choosing a digivolution card to trash.");
                    yield return ContinuousController.instance.StartCoroutine(
                        GManager.instance.userSelectionManager.WaitForEndSelect());
                    bool fromTop = GManager.instance.userSelectionManager.SelectedBoolValue;
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                            targetPermanent: selectedTarget,
                            trashCount: 1,
                            isFromTop: fromTop,
                            activateClass: activateClass));
                }
            }
            #endregion

            // [Rule] Trait: Has [Aquatic] Type is a card-data trait rule.
            // Confirm the card's metadata includes Aquatic; do not add an always-active effect here.
            return cardEffects;
        }
    }
}
