using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

// Millenniummon
namespace DCGO.CardEffects.P
{
    public class P_220 : CEntity_Effect
    {

         public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {

            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Kimeramon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 6,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region DNA Digivolution

            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect($"DNA Digivolution", CanUseCondition, card);
                addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
                addJogressConditionClass.SetNotShowUI(true);
                cardEffects.Add(addJogressConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                JogressCondition GetJogress(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        bool PermanentCondition1(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                            {
                                if (permanent.TopCard.EqualsCardName("Kimeramon"))
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        bool PermanentCondition2(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                            {
                                if (permanent.TopCard.EqualsCardName("Machinedramon"))
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "Kimeramon"),

                        new JogressConditionElement(PermanentCondition2, "Machinedramon"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }

            #endregion

            #region Assembly

            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect($"Assembly", CanUseCondition, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        AssemblyConditionElement element = new AssemblyConditionElement(CanSelectCardCondition);

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            return cardSource != null &&
                                cardSource.Owner == card.Owner &&
                                cardSource.IsDigimon &&
                                cardSource.HasLevel &&
                                (cardSource.EqualsTraits("Composite") ||
                                    cardSource.EqualsTraits("Ver.3") ||
                                    cardSource.EqualsTraits("Ver.5"));
                        }

                        bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                        {
                            List<int> cardLevels = new List<int>();

                            foreach (CardSource cardSource1 in cardSources)
                            {
                                if (!cardLevels.Contains(cardSource1.Level))
                                {
                                    cardLevels.Add(cardSource1.Level);
                                }
                                foreach (int level in cardSource1.Level_Assembly)
                                {
                                    if (!cardLevels.Contains(level))
                                    {
                                        cardLevels.Add(level);
                                    }
                                }
                            }

                            if (cardSource.Level_Assembly.Count((level) => cardLevels.Contains(level)) >= 1 || cardLevels.Contains(cardSource.Level))
                            {
                                return false;
                            }

                            return true;
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element:element,
                            CanTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                            selectMessage: "3 [Composite]/[Ver.3]/[Ver.5] Trait Digimon cards w/different levels",
                            elementCount: 3,
                            reduceCost: 6);

                        return assemblyCondition;
                    }

                    return null;
                }
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

            #region Shared OP / WD

            string SharedEffectName()
                => "<De-Digivolve 2>. Then Delete 1 Digimon";

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] <De-Digivolve 2> 1 of your opponent's Digimon. Then, you may delete 1 Digimon.";
            }

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card) &&
                    CardEffectCommons.HasMatchConditionPermanent(PermanentSelectCondition1);
            }

            bool PermanentSelectCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool PermanentSelectCondition1(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region De-digivolve 2
                if (CardEffectCommons.HasMatchConditionPermanent(PermanentSelectCondition))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(PermanentSelectCondition));
                    var selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: PermanentSelectCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 Digimon to De-Digivolve.",
                        "The opponent is selecting 1 Digimon to De-Digivolve.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        if (!permanent.TopCard.CanNotBeAffected(activateClass) && !permanent.ImmuneFromDeDigivolve()) {
                            yield return ContinuousController.instance.StartCoroutine(new IDegeneration(permanent, 2, activateClass).Degeneration());
                        }
                    }
                }
                #endregion

                #region Delete 1
                if (CardEffectCommons.HasMatchConditionPermanent(PermanentSelectCondition1))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(PermanentSelectCondition1));
                    var selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: PermanentSelectCondition1,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 Digimon to Delete.",
                        "The opponent is selecting 1 Digimon to Delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
                #endregion
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, (hashTable) => SharedActivateCoroutine(hashTable, activateClass), -1, true, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                        CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, (hashTable) => SharedActivateCoroutine(hashTable, activateClass), -1, true, SharedEffectDescription("when Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                        CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }

            #endregion

            #region On Deletion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 3 cards to play 2 digimon from Trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] By returning 3 [Composite], [Wicked God] or [DM] trait cards from your trash to the bottom of the deck, you may play 2 level 6 or lower [Composite], [Ver.3] or [Ver.5] trait Digimon cards from your trash without paying the costs. This effect can't play cards of the same level.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card) &&
                        card.Owner.TrashCards.Count(CardSelectCondition) >= 3;
                }

                bool CardSelectCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("Composite") ||
                        cardSource.EqualsTraits("Wicked God") ||
                        cardSource.HasDMTraits;
                }

                bool CardSelectCondition1(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                        cardSource.HasLevel &&
                        cardSource.Level <= 6 &&
                        (cardSource.EqualsTraits("Composite") ||
                            cardSource.EqualsTraits("Ver.3") ||
                            cardSource.EqualsTraits("Ver.5"));
                }

                bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                {
                    List<int> cardLevels = new List<int>();

                    foreach (CardSource cardSource1 in cardSources)
                    {
                        if (!cardLevels.Contains(cardSource1.Level))
                        {
                            cardLevels.Add(cardSource1.Level);
                        }
                    }

                    if (cardSource.Level_Assembly.Count((level) => cardLevels.Contains(level)) >= 1 || cardLevels.Contains(cardSource.Level))
                    {
                        return false;
                    }

                    return true;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.TrashCards.Count(CardSelectCondition) >= 3)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = Math.Min(3, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CardSelectCondition));

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CardSelectCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "3 [Composite]/[Wicked God]/[DM] Trait Digimon cards",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        selectCardEffect.SetUpCustomMessage("Select 3 [Composite]/[Wicked God]/[DM] Trait Digimon cards to bottom deck", "Your opponent is selecting 3 Digimon cards to bottom deck");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Selected Cards");
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        if (selectedCards.Count == 3)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ReturnRevealedCardsToLibraryBottom(
                                remainingCards: selectedCards,
                                activateClass: activateClass));

                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CardSelectCondition1)){
                                List<CardSource> selectedCards1 = new List<CardSource>();

                                int maxCount1 = Math.Min(2, card.Owner.TrashCards.Filter(CardSelectCondition1).Map(cardSource => cardSource.Level).Distinct().Count());

                                SelectCardEffect selectCardEffect1 = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect1.SetUp(
                                    canTargetCondition: CardSelectCondition1,
                                    canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine1,
                                    afterSelectCardCoroutine: null,
                                    message: "2 [Composite]/[Ver.3]/[Ver.5] Trait Digimon cards with different levels",
                                    maxCount: maxCount1,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                IEnumerator SelectCardCoroutine1(CardSource cardSource)
                                {
                                    selectedCards1.Add(cardSource);
                                    yield return null;
                                }

                                selectCardEffect1.SetUpCustomMessage("Select 2 [Composite]/[Ver.3]/[Ver.5] Trait Digimon cards to play", "Your opponent is selecting 2 Digimon cards to play");
                                selectCardEffect1.SetUpCustomMessage_ShowCard("Selected Cards");
                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect1.Activate());

                                if (selectedCards1.Count() > 0)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                        cardSources: selectedCards1, 
                                        activateClass: activateClass, 
                                        payCost: false, 
                                        isTapped: false, 
                                        root: SelectCardEffect.Root.Trash, 
                                        activateETB: true));
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            return cardEffects;     
        }
    }
}