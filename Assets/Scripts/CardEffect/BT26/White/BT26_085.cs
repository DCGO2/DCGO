using System.Collections;
using System.Collections.Generic;

// Giant Slayer
namespace DCGO.CardEffects.BT26
{
    public class BT26_085 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Collision
            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.CollisionSelfStaticEffect(false, card, null));
            }
            #endregion

            #region Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent's effects can't reduce this Digimon's DP or trash its stacked cards until their turn ends", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Play] Until your opponent's turn ends, your opponent's effects can't reduce this Digimon's DP or trash its stacked cards.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent permanent = card.PermanentOfThisCard();

                    bool EffectCondition(ICardEffect effect) => CardEffectCommons.IsOpponentEffect(effect, card);

                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.GainImmuneFromDPMinus(permanent, EffectCondition, EffectDuration.UntilOpponentTurnEnd, activateClass, "Immune from DP reduction"));

                    bool PermanentCondition(Permanent p) => p == permanent;

                    ImmuneStackTrashingClass immuneStackTrashingClass = new ImmuneStackTrashingClass();
                    immuneStackTrashingClass.SetUpICardEffect("Immune from stack trashing", _ => true, card);
                    immuneStackTrashingClass.SetUpImmuneFromStackTrashingClass(PermanentCondition: PermanentCondition, EffectCondition: EffectCondition);

                    CardEffectCommons.AddEffectToPermanent(
                        targetPermanent: permanent,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        card: card,
                        cardEffect: immuneStackTrashingClass,
                        timing: EffectTiming.None);
                }
            }
            #endregion

            #region All Turns - Prevent Leaving
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By digivolving into [Chronomon: Destroy Mode] from hand or trash, it doesn't leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] When this Digimon would leave the battle area, by digivolving it into [Chronomon: Destroy Mode] in the hand or trash without paying the cost, it doesn't leave.";

                bool CanSelectCardCondition(CardSource cardSource) => cardSource.EqualsCardName("Chronomon: Destroy Mode");

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition)
                            || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition));

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    if (canSelectHand && canSelectTrash)
                    {
                        List<SelectionElement<int>> selectionElements1 = new List<SelectionElement<int>>()
                    {
                        new (message: $"From hand", value : 1, spriteIndex: 0),
                        new (message: $"From trash", value : 2, spriteIndex: 0),
                        new (message: $"Don't Digivolve", value: 3, spriteIndex: 1)
                    };

                        string selectPlayerMessage1 = "From which area will you Digivolve?";
                        string notSelectPlayerMessage1 = "The opponent is choosing from which area to select a card.";

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage1, notSelectPlayerMessage: notSelectPlayerMessage1);
                    }
                    else
                    {
                        GManager.instance.userSelectionManager.SetInt(canSelectHand ? 1 : 2);
                    }

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool doSelect = GManager.instance.userSelectionManager.SelectedIntValue != 3;

                    if (doSelect)
                    {
                        Permanent thisPermanent = card.PermanentOfThisCard();

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            thisPermanent,
                            CanSelectCardCondition,
                            payCost: false,
                            reduceCostTuple: null,
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: canSelectHand,
                            activateClass: activateClass,
                            successProcess: SuccessProcess()
                        ));

                        IEnumerator SuccessProcess()
                        {
                            thisPermanent.willBeRemoveField = false;
                            thisPermanent.HideDeleteEffect();
                            thisPermanent.HideHandBounceEffect();
                            thisPermanent.HideDeckBounceEffect();
                            thisPermanent.HideWillRemoveFieldEffect();

                            yield return null;
                        }
                    }
                }
            }
            #endregion

            #region Assembly
            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect("Assembly", CanUseCondition, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool CanUseCondition(Hashtable hashtable) => true;

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        AssemblyConditionElement element = new AssemblyConditionElement(CanSelectCardCondition);

                        bool CanSelectCardCondition(CardSource cs)
                            => cs.HasText("Chronomon") || cs.EqualsTraits("Shaman");

                        bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                        {
                            if (!cardSource.HasLevel) return false;

                            foreach (CardSource selected in cardSources)
                            {
                                if (selected.HasLevel && selected.Level == cardSource.Level) return false;
                            }

                            return true;
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element: element,
                            CanTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                            selectMessage: "5 different-level cards w/[Chronomon] in text or w/[Shaman] trait",
                            elementCount: 5,
                            reduceCost: 5);

                        return assemblyCondition;
                    }

                    return null;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
