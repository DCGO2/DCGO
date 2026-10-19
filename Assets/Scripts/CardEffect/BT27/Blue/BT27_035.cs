using System.Collections;
using System.Collections.Generic;

// MirageGaogamon: Burst Mode // Full Moon Meteor Impact
namespace DCGO.CardEffects.BT27
{
    public class BT27_035 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon side - alternate digivolution
            if (timing == EffectTiming.None)
            {
                bool DataSquadLevelSix(Permanent permanent)
                    => permanent != null && permanent.TopCard != null && permanent.TopCard.EqualsTraits("DATA SQUAD");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: DataSquadLevelSix, digivolutionCost: 5,
                    ignoreDigivolutionRequirement: true, card: card, condition: null, level: 6));
            }
            #endregion

            #region Burst Digivolution
            if (timing == EffectTiming.None)
            {
                AddBurstDigivolutionConditionClass burst = new AddBurstDigivolutionConditionClass();
                burst.SetUpICardEffect("Burst Digivolution", _ => true, card);
                burst.SetUpAddBurstDigivolutionConditionClass(getBurstDigivolutionCondition: GetBurstDigivolution);
                burst.SetNotShowUI(true);
                cardEffects.Add(burst);

                BurstDigivolutionCondition GetBurstDigivolution(CardSource source)
                {
                    if (source != card) return null;

                    bool TamerCondition(Permanent permanent)
                        => permanent != null && permanent.TopCard != null
                           && permanent.TopCard.Owner == card.Owner
                           && card.Owner.GetBattleAreaPermanents().Contains(permanent)
                           && !permanent.CannotReturnToHand(null)
                           && permanent.TopCard.EqualsCardName("Thomas H. Norstein");

                    bool DigimonCondition(Permanent permanent)
                        => permanent != null && permanent.TopCard != null
                           && permanent.TopCard.Owner == card.Owner
                           && card.Owner.GetFieldPermanents().Contains(permanent)
                           && !card.CanNotEvolve(permanent)
                           && permanent.TopCard.EqualsCardName("MirageGaogamon");

                    return new BurstDigivolutionCondition(
                        tamerCondition: TamerCondition, selectTamerMessage: "1 [Thomas H. Norstein]",
                        digimonCondition: DigimonCondition, selectDigimonMessage: "1 [MirageGaogamon]",
                        cost: 0);
                }
            }
            #endregion

            #region Keywords
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
                cardEffects.Add(CardEffectFactory.EvadeSelfEffect(isInheritedEffect: false, card: card, condition: null));
            #endregion

            #region Shared When Digivolving / When Attacking - Option-side Main
            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                "May activate this card's Option-side Main effect",
                ActivateOptionSide,
                tag => $"[{tag}] [Once Per Turn] You may activate 1 [Main] effect on this card's Option side.",
                optional: false, isSkippable: true, maxCountPerTurn: 1,
                hashValue: "BT27_035_WD_WA_Option", whenDigivolving: true, whenAttacking: true);

            IEnumerator ActivateOptionSide(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(
                    CardEffectCommons.ActivateMainOfOptionSide(card, activateClass));
            }
            #endregion

            #region Shared When Digivolving / When Attacking - Hand reduction and security
            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                "Return random-facing opponent hand cards to deck bottom, then security to deck bottom",
                ReduceHandAndSecurity,
                tag => $"[{tag}] If your opponent has 8 or more cards in hand, by returning cards without looking to the bottom of the deck until 7 remain, return their top security card to the bottom of the deck.",
                optional: false,
                additionalActivateCondition: (hashtable, effect) => card.Owner.Enemy.HandCards.Count >= 8,
                maxCountPerTurn: -1,
                hashValue: "BT27_035_WD_WA_Hand", whenDigivolving: true, whenAttacking: true);

            IEnumerator ReduceHandAndSecurity(Hashtable hashtable, ActivateClass activateClass)
            {
                if (card.Owner.Enemy.HandCards.Count < 8) yield break;

                int count = card.Owner.Enemy.HandCards.Count - 7;
                List<CardSource> selectedCards = new List<CardSource>();

                // Follow BT13_033: conceal hand identities, shuffle selection presentation,
                // and select exactly the required number without looking.
                if (card.Owner.isYou)
                    foreach (CardSource source in card.Owner.Enemy.HandCards) source.SetReverse();

                card.Owner.Enemy.HandCards = RandomUtility.ShuffledDeckCards(card.Owner.Enemy.HandCards);

                SelectCardEffect selector = GManager.instance.GetComponent<SelectCardEffect>();
                selector.SetUp(
                    canTargetCondition: _ => true,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    canNoSelect: () => false,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: AfterSelect,
                    message: "Select cards to return to the bottom of the opponent's deck without looking.",
                    maxCount: count,
                    canEndNotMax: false,
                    isShowOpponent: false,
                    mode: SelectCardEffect.Mode.Custom,
                    root: SelectCardEffect.Root.Custom,
                    customRootCardList: card.Owner.Enemy.HandCards,
                    canLookReverseCard: false,
                    selectPlayer: card.Owner,
                    cardEffect: activateClass);
                selector.SetNotShowCard();
                selector.SetUseFaceDown();
                if (card.Owner.isYou) selector.SetNotAddLog();
                selector.SetUpCustomMessage("Select cards to return to the deck bottom.",
                    "The opponent is selecting cards to return to the deck bottom.");

                yield return ContinuousController.instance.StartCoroutine(selector.Activate());

                IEnumerator AfterSelect(List<CardSource> cards)
                {
                    if (cards.Count > 0)
                    {
                        selectedCards.AddRange(cards);
                        yield return ContinuousController.instance.StartCoroutine(
                            CardObjectController.AddLibraryBottomCards(cards,
                                notAddLog: card.Owner.isYou, cardEffect: activateClass));
                    }
                }

                if (card.Owner.isYou)
                    foreach (CardSource source in card.Owner.Enemy.HandCards) source.SetFace();
                else if (selectedCards.Count > 0)
                    yield return ContinuousController.instance.StartCoroutine(
                        GManager.instance.GetComponent<Effects>().ShowCardEffect(selectedCards,
                            "Deck Bottom Cards", true, true));

                // Resolve the security movement only after the hand reduction finishes.
                if (selectedCards.Count == count && card.Owner.Enemy.SecurityCards.Count > 0)
                {
                    List<CardSource> topSecurity = new List<CardSource> { card.Owner.Enemy.SecurityCards[0] };
                    yield return ContinuousController.instance.StartCoroutine(
                        CardObjectController.AddLibraryBottomCards(topSecurity, cardEffect: activateClass));
                    yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                        player: card.Owner.Enemy,
                        refSkillInfos: ref ContinuousController.instance.nullSkillInfos,
                        activateClass).ReduceSecurity());
                }
            }
            #endregion

            #region Option side - Use requirement
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card,
                    source => source.EqualsTraits("DATA SQUAD")));
            }
            #endregion

            #region Option side - Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return an opponent Digimon to hand; if their hand has 8+, may unsuspend a Digimon", CanUse, card);
                activateClass.SetUpActivateClass(null, ActivateOption, -1, false,
                    "[Main] Return 1 of your opponent's Digimon to the hand. Then, if their hand has 8 or more cards, 1 of your Digimon may unsuspend.");
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                bool CanUse(Hashtable hashtable) => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                bool EnemyDigimon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                bool OwnDigimon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                IEnumerator ActivateOption(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(EnemyDigimon))
                    {
                        SelectPermanentEffect selector = GManager.instance.GetComponent<SelectPermanentEffect>();
                        selector.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: EnemyDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Bounce,
                            cardEffect: activateClass);
                        selector.SetUpCustomMessage("Select 1 opponent's Digimon to return to hand.",
                            "The opponent is selecting a Digimon to return to hand.");
                        yield return ContinuousController.instance.StartCoroutine(selector.Activate());
                    }

                    if (card.Owner.Enemy.HandCards.Count >= 8 && CardEffectCommons.HasMatchConditionPermanent(OwnDigimon))
                    {
                        SelectPermanentEffect selector = GManager.instance.GetComponent<SelectPermanentEffect>();
                        selector.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OwnDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: UnsuspendSelected,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);
                        selector.SetUpCustomMessage("You may select 1 of your Digimon to unsuspend.",
                            "The opponent may unsuspend 1 of their Digimon.");
                        yield return ContinuousController.instance.StartCoroutine(selector.Activate());
                    }
                }

                IEnumerator UnsuspendSelected(Permanent permanent)
                {
                    if (permanent != null)
                        yield return ContinuousController.instance.StartCoroutine(
                            new IUnsuspendPermanents(new List<Permanent> { permanent }, activateClass).Unsuspend());
                }
            }
            #endregion

            #region Option side - Arts Digivolution
            if (timing == EffectTiming.None)
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            #endregion

            return cardEffects;
        }
    }
}
