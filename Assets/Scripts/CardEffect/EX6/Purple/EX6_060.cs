using System.Collections;
using System.Collections.Generic;
using System;

// Belphemon: Rage Mode
namespace DCGO.CardEffects.EX6
{
    public class EX6_060 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Belphemon: Sleep Mode");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, 
                    digivolutionCost: 1, 
                    ignoreDigivolutionRequirement: false,
                    card: card, 
                    condition: null));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "Trash up to 3 cards, suspend equal amount of lvl 5 or lower enemy Digimon. Then delete all suspended enemy Digimon with lowest play cost.";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] You may trash up to 3 cards in your hand. For each card trashed by this effect, suspend 1 of your opponent's level 5 or lower Digimon. Then, delete all of your opponent's suspended Digimon with the lowest play cost.";
            }

            bool OpponentsSuspendableTargets(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.HasLevel
                    && permanent.Level <= 5;
            }

            bool LowestPlayCostCondition(Permanent permanent)
            {
                return CardEffectCommons.IsMinCost(permanent, card.Owner.Enemy, true, (permanent) => permanent.IsSuspended);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                List<CardSource> discarded = new List<CardSource>();
                int discardCount = Math.Min(3, card.Owner.HandCards.Count);

                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: (cardSource) => true,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: discardCount,
                    canNoSelect: true,
                    canEndNotMax: true,
                    isShowOpponent: true,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: AfterSelectionCoroutine,
                    mode: SelectHandEffect.Mode.Discard,
                    cardEffect: activateClass);

                yield return StartCoroutine(selectHandEffect.Activate());

                IEnumerator AfterSelectionCoroutine(List<CardSource> cardSources)
                {
                    if (cardSources.Count > 0)
                    {
                        discarded = cardSources.Clone();
                    }

                    yield return null;
                }

                if (discarded.Count > 0)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    int suspendCount = Math.Min(discarded.Count, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, OpponentsSuspendableTargets));

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: OpponentsSuspendableTargets,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: suspendCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

                List<Permanent> destroyTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(LowestPlayCostCondition);
                yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(destroyTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 7GDL from trash to bottom of [Gate of Deadly Sins]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When this Digimon would leave the battle area other than in battle, place 1 card with the [Seven Great Demon Lord] trait from your trash as the bottom digivolution cards of one of your [Gate of Deadly Sins] in your breeding area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card)
                        && !CardEffectCommons.IsByBattle(hashtable);
                }

                bool CanSelect7GDLinTrash(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("Seven Great Demon Lords");
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBreedingArea(permanent, card)
                        && permanent.TopCard.EqualsCardName("Gate of Deadly Sins");
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelect7GDLinTrash))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelect7GDLinTrash,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to place at the bottom of digivolution cards.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to place at the bottom of digivolution cards.", "The opponent is selecting 1 card to place at the bottom of digivolution cards.");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        if (selectedCards.Count >= 1
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition, true))
                        {
                            Permanent selectedPermanent = card.Owner.GetBreedingAreaPermanents()[0];

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(selectedCards, activateClass));
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