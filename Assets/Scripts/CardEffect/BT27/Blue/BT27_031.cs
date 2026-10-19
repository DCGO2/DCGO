using System.Collections;
using System.Collections.Generic;
using System.Linq;

// JumboGamemon
namespace DCGO.CardEffects.BT27
{
    public class BT27_031 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            // [Digivolve] Lv.5 w/[DATA SQUAD] trait: Cost 4
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                    => permanent.TopCard != null && permanent.TopCard.EqualsTraits("DATA SQUAD");
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 4,
                    ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }

            // <Decode (Lv.5 w/[Aquatic]/[DATA SQUAD] trait)>
            if (timing == EffectTiming.WhenRemoveField)
            {
                bool SourceCondition(CardSource source)
                    => source.IsDigimon && source.HasLevel && source.Level == 5
                       && (source.EqualsTraits("Aquatic") || source.EqualsTraits("DATA SQUAD"));
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(
                    card: card, isInheritedEffect: false,
                    decodeStrings: new[] { "(Lv.5 w/[Aquatic]/[DATA SQUAD] trait)", "Level 5 Digimon with [Aquatic] or [DATA SQUAD] trait" },
                    sourceCondition: SourceCondition, condition: null));
            }

            // [On Play] [When Digivolving] [Counter] [Once Per Turn]
            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                "Place another friendly Digimon/Tamer under JumboGamemon, then bottom-deck an opposing Digimon",
                SharedActivateCoroutine,
                tag => $"[{tag}] [Once Per Turn] By placing 1 of your other Digimon or Tamers as this Digimon's bottom digivolution card, return 1 of your opponent's Digimon with as many digivolution cards as this Digimon or fewer to the bottom of the deck.",
                optional: false, isSkippable: true, maxCountPerTurn: 1,
                hashValue: "BT27_031_OP_WD_COUNTER", onPlay: true, whenDigivolving: true, counter: true);

            bool CanSelectFriendly(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                   && permanent != card.PermanentOfThisCard()
                   && (permanent.IsDigimon || permanent.IsTamer);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (!CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                    || !CardEffectCommons.HasMatchConditionPermanent(CanSelectFriendly))
                {
                    activateClass.RemoveUse();
                    yield break;
                }

                Permanent selected = null;
                SelectPermanentEffect selector = GManager.instance.GetComponent<SelectPermanentEffect>();
                selector.SetUp(selectPlayer: card.Owner, canTargetCondition: CanSelectFriendly,
                    canTargetCondition_ByPreSelecetedList: null, canEndSelectCondition: null,
                    maxCount: 1, canNoSelect: true, canEndNotMax: false,
                    selectPermanentCoroutine: SelectFriendlyCoroutine, afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom, cardEffect: activateClass);
                selector.SetUpCustomMessage("Select another Digimon or Tamer to place under JumboGamemon.",
                    "The opponent is choosing a Digimon or Tamer to place as a digivolution card.");
                yield return ContinuousController.instance.StartCoroutine(selector.Activate());

                IEnumerator SelectFriendlyCoroutine(Permanent permanent)
                {
                    selected = permanent;
                    yield return null;
                }

                if (selected == null || !CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                {
                    activateClass.RemoveUse();
                    yield break;
                }

                yield return ContinuousController.instance.StartCoroutine(
                    new IPlacePermanentToDigivolutionCards(
                        new List<Permanent[]> { new Permanent[] { selected, card.PermanentOfThisCard() } },
                        false, activateClass).PlacePermanentToDigivolutionCards());

                if (!CardEffectCommons.IsExistOnBattleAreaDigimon(card)) yield break;
                bool CanSelectOpponent(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                       && permanent.DigivolutionCards.Count <= card.PermanentOfThisCard().DigivolutionCards.Count;
                if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponent)) yield break;

                SelectPermanentEffect targetSelector = GManager.instance.GetComponent<SelectPermanentEffect>();
                targetSelector.SetUp(selectPlayer: card.Owner, canTargetCondition: CanSelectOpponent,
                    canTargetCondition_ByPreSelecetedList: null, canEndSelectCondition: null,
                    maxCount: 1, canNoSelect: false, canEndNotMax: false,
                    selectPermanentCoroutine: null, afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.PutLibraryBottom, cardEffect: activateClass);
                targetSelector.SetUpCustomMessage("Select 1 opposing Digimon to return to the bottom of the deck.",
                    "The opponent is selecting a Digimon to return to the bottom of the deck.");
                yield return ContinuousController.instance.StartCoroutine(targetSelector.Activate());
            }

            // [All Turns] [Once Per Turn] When effects add to this Digimon's digivolution cards...
            if (timing == EffectTiming.OnAddDigivolutionCards)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play a blue cost 5 or lower source card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true,
                    "[All Turns] [Once Per Turn] When effects add to this Digimon's digivolution cards, you may play 1 play cost 5 or lower blue card from its digivolution cards without paying the cost.");
                activateClass.SetHashString("BT27_031_ALL_TURNS");
                cardEffects.Add(activateClass);

                bool CanSelectSource(CardSource source)
                    => source.GetCostItself <= 5 && source.CardColors.Contains(CardColor.Blue)
                       && (source.IsDigimon || source.IsTamer);
                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                       && CardEffectCommons.CanTriggerOnAddDigivolutionCard(
                           hashtable, permanent => permanent == card.PermanentOfThisCard(),
                           null, source => true);
                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                       && card.PermanentOfThisCard().DigivolutionCards.Any(CanSelectSource);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();
                    SelectCardEffect select = GManager.instance.GetComponent<SelectCardEffect>();
                    select.SetUp(canTargetCondition: CanSelectSource,
                        canTargetCondition_ByPreSelecetedList: null, canEndSelectCondition: null,
                        canNoSelect: () => true, selectCardCoroutine: SelectSourceCoroutine,
                        afterSelectCardCoroutine: null, message: "Select a blue cost 5 or lower source card to play.",
                        maxCount: 1, canEndNotMax: false, isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom, root: SelectCardEffect.Root.Custom,
                        customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                        canLookReverseCard: true, selectPlayer: card.Owner, cardEffect: activateClass);
                    yield return ContinuousController.instance.StartCoroutine(select.Activate());
                    IEnumerator SelectSourceCoroutine(CardSource source)
                    {
                        selectedCards.Add(source);
                        yield return null;
                    }
                    if (selectedCards.Count == 0)
                    {
                        activateClass.RemoveUse();
                        yield break;
                    }
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.PlayPermanentCards(cardSources: selectedCards,
                            activateClass: activateClass, payCost: false, isTapped: false,
                            root: SelectCardEffect.Root.DigivolutionCards, activateETB: true));
                }
            }
            return cardEffects;
        }
    }
}
