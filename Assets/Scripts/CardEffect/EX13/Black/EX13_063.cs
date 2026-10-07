using System.Collections;
using System.Collections.Generic;

// PrinceMamemon
namespace DCGO.CardEffects.EX13
{
    public class EX13_063 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Assembly
            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect("Assembly", CanUseCondition, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                    => true;

                bool IsAssemblyCard(CardSource assemblyCard)
                    => assemblyCard != null
                        && assemblyCard.Owner == card.Owner
                        && assemblyCard.HasLevel
                        && assemblyCard.Level <= 5
                        && assemblyCard.HasText("Mamemon");

                bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                {
                    List<CardSource> allCards = cardSources.Clone();

                    allCards.Add(cardSource);

                    return allCards.Count == Combinations.GetUniqueNameCardCount(allCards);
                }

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource != card) return null;

                    return new AssemblyCondition(
                        element: new AssemblyConditionElement(IsAssemblyCard),
                        CanTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                        selectMessage: "3 Lv.5 or lower [Mamemon] text cards w/different names",
                        elementCount: 3,
                        reduceCost: 4);
                }
            }
            #endregion

            #region Shared On Play / When Digivolving / On Deletion

            string SharedEffectName = "Reveal top 3, may play 1 play cost 10 or lower [Mamemon] in name/[Mutant] trait Digimon for free";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Reveal the top 3 cards of your deck. You may play 1 play cost 10 or lower Digimon card with [Mamemon] in its name or the [Mutant] trait among them without paying the cost. Trash the rest.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.IsDigimon
                        && cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 10
                        && (cardSource.ContainsCardName("Mamemon") || cardSource.EqualsTraits("Mutant"))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                    revealCount: 3,
                    simplifiedSelectCardConditions:
                    new SimplifiedSelectCardConditionClass[]
                    {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition: CanSelectCardCondition,
                            message: "Select 1 Digimon card to play.",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: 1,
                            selectCardCoroutine: SelectCardCoroutine),
                    },
                    remainingCardsPlace: RemainingCardsPlace.Trash,
                    activateClass: activateClass,
                    canNoSelect: true
                ));

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: new List<CardSource>() { cardSource },
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Library,
                            activateETB: true));
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
                whenDigivolving: true,
                onDeletion: true);

            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 opponent's highest play cost Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Deletion] Delete 1 of your opponent's highest play cost Digimon.";

                bool CanSelectDeleteTargetCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && CardEffectCommons.IsMaxCost(permanent, card.Owner.Enemy, true);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.CanActivateOnDeletion(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteTargetCondition))
                    {
                        SelectPermanentEffect selectDeleteEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectDeleteEffect.SetUp(
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

                        selectDeleteEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectDeleteEffect.Activate());
                    }
                }
            }
            #endregion

            #region All Turns - Blocker
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.ContainsCardName("Mamemon");

                bool Condition()
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

                cardEffects.Add(CardEffectFactory.BlockerStaticEffect(
                    permanentCondition: PermanentCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition));
            }
            #endregion

            #region All Turns - Guard
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.ContainsCardName("Mamemon");

                bool CardSourceCondition(CardSource cardSource)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(cardSource)
                        && cardSource.Owner == card.Owner
                        && cardSource == cardSource.PermanentOfThisCard().TopCard
                        && PermanentCondition(cardSource.PermanentOfThisCard());

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> effects, EffectTiming _timing)
                {
                    if (_timing == EffectTiming.WhenRemoveField)
                    {
                        effects.Add(CardEffectFactory.GuardSelfEffect(isInheritedEffect: false, card: cardSource, condition: null));
                    }

                    return effects;
                }

                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("Your [Mamemon] in name Digimon gain Guard", CanUseCondition, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects, limitTiming: EffectTiming.WhenRemoveField);
                cardEffects.Add(addSkillClass);
            }
            #endregion

            return cardEffects;
        }
    }
}
