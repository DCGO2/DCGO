using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Imperialdramon: Paladin Mode
namespace DCGO.CardEffects.EX13
{
    public class EX13_076 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Free") 
                        || targetPermanent.TopCard.EqualsTraits("Royal Knight");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 5,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 6));
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Vortex
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.VortexSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Evade
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.EvadeSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP / WD / WA

            string SharedHashString = "EX13_076_OP_WD_WA";

            string SharedEffectName = "May Suspend Digimon, May strip digivolution cards from and battle a digimon, comparing digivolution cards";

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] You may suspend 1 of your opponent's Digimon. Then, you may return all digivolution cards of 1 of their Digimon to the bottom of the deck and have this Digimon battle it. Compare the number of digivolution cards instead of DP in this battle.";

            bool OpponentsDigimonCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            bool UnsuspendedDigimonCondition(Permanent permanent) => OpponentsDigimonCondition(permanent) && !permanent.IsSuspended;

            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass) => CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimonCondition);

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    additionalActivateCondition: AdditionalActivateCondition,
                    maxCountPerTurn: 1,
                    hashValue: SharedHashString,
                    optional: false,
                    isSkippable: true,
                    onPlay: true,
                    whenDigivolving: true,
                    whenAttacking: true);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool isUsed = false;

                #region May botdeck digimon
                if (CardEffectCommons.HasMatchConditionPermanent(UnsuspendedDigimonCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: UnsuspendedDigimonCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        if (permanents.Count > 0) { isUsed = true; }
                        yield return null;
                    }

                    selectPermanentEffect.SetUpCustomMessage("Select 1 opponent's Digimon to suspend.", "The opponent is selecting 1 Digimon to return to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
                #endregion

                #region May source strip and battle digimon
                if (CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimonCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: OpponentsDigimonCondition,
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
                        isUsed = true;

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                            targetPermanent: permanent, 
                            trashCount: permanent.DigivolutionCards.Count, 
                            isFromTop: true, 
                            activateClass: activateClass));

                        yield return ContinuousController.instance.StartCoroutine(new IBattle(card.PermanentOfThisCard(), permanent, null, true, CompareDigivolutionCards: true).Battle());
                    }

                    selectPermanentEffect.SetUpCustomMessage("Select 1 opponent's Digimon. You will remove all digivolution cards and then battle with this Digimon, comparing digivolution cards.", "The opponent is selecting 1 Digimon to remove sources and battle.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
                #endregion

                if (!isUsed) activateClass.RemoveUse();
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnEndBattle)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May botdeck 1 enemy Digimon, may unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("EX13_076_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When this Digimon wins a battle, you may return 1 of your opponent's Digimon to the bottom of the deck. Then, this Digimon may unsuspend.";

                bool WinnerCondition(Permanent permanent) => permanent.cardSources.Contains(card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenWinBattle(hashtable, WinnerCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                    && (CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimonCondition)
                        || card.PermanentOfThisCard().IsSuspended);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool isUsed = false;

                    #region May botdeck digimon
                    if (CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimonCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentsDigimonCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                            cardEffect: activateClass);

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            if (permanents.Count > 0) { isUsed = true; }
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to return to bottom of deck.", "The opponent is selecting 1 Digimon to return to bottom of deck.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                    #endregion
                    
                    #region May unsuspend
                    string selectPlayerMessage = "Will you unsuspend this card?";
                    string notSelectPlayerMessage = "The opponent is choosing if they will unsuspend.";

                    List<SelectionElement<bool>> command_SelectCommands = new List<SelectionElement<bool>>()
                    {
                        new SelectionElement<bool>(message: $"Yes", value: true, spriteIndex: 0),
                        new SelectionElement<bool>(message: $"No", value: false, spriteIndex: 1),
                    };

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: command_SelectCommands, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool unsuspend = GManager.instance.userSelectionManager.SelectedBoolValue;

                    if (unsuspend)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                            new List<Permanent>() { card.PermanentOfThisCard() },
                            activateClass).Unsuspend());
                        isUsed = true;
                    }
                    #endregion

                    if (!isUsed) activateClass.RemoveUse();
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

                        bool CanSelectCardCondition(CardSource cardSource)
                            => cardSource.EqualsTraits("Free") 
                            || cardSource.EqualsTraits("Royal Knight");

                        bool CanTargetCondition_ByPreSelecetedList(List<CardSource> selectedCards, CardSource cardSource)
                        {
                            List<CardSource> AllCards = selectedCards.Clone();
                            AllCards.Add(cardSource);
                            return Combinations.GetUniqueNameCardCount(AllCards) == AllCards.Count;
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element: element,
                            CanTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                            selectMessage: "6 [Free]/[Royal Knight] trait Digimon cards w/ different names",
                            elementCount: 6,
                            reduceCost: 8);

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
