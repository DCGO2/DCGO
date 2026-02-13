using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Xeno
namespace DCGO.CardEffects.EX11
{
    public class EX11_066 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Name Rule

            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Also treated as [Zenith]", CanUseCondition, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: changeCardNames);
                cardEffects.Add(changeCardNamesClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                List<string> changeCardNames(CardSource cardSource, List<string> CardNames)
                {
                    if (cardSource == card)
                    {
                        CardNames.Add("Zenith");
                    }

                    return CardNames;
                }
            }

            #endregion

            #region Shared OP / SOYMP

            string SharedEffectName = "Trash 1 Vemmon in text to Draw 1, Gain 1 Memory";

            string SharedEffectDescription(string tag) => $"[{tag}] By trashing 1 card with [Vemmon] in its text from your hand, <Draw 1> and gain 1 memory.";

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card)
                    && card.Owner.HandCards.Count(HasVemmonArchetype) >= 1;
            }

            bool HasVemmonArchetype(CardSource cardSource) => cardSource.HasText("Vemmon");

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool discarded = false;

                int discardCount = 1;

                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: HasVemmonArchetype,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: discardCount,
                    canNoSelect: true,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: AfterSelectCardCoroutine,
                    mode: SelectHandEffect.Mode.Discard,
                    cardEffect: activateClass);

                selectHandEffect.SetUpCustomMessage("Select 1 Card to trash.", "The opponent is selecting 1 card to trash from their hand.");

                yield return StartCoroutine(selectHandEffect.Activate());

                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                {
                    if (cardSources.Count >= 1)
                    {
                        discarded = true;

                        yield return null;
                    }
                }

                if (discarded)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            #endregion

            #region Start of your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("Start of Your Main Phase"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal 2. Place All Vemmon under played or Digivolved digimon, trash the rest.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() => "[All Turns] When your Digimon are played or digivolve, if any of them have [Vemmon] in their texts, by suspending this Tamer, reveal the top 2 cards of your deck. Place all [Vemmon] among them as any of those Digimon's bottom digivolution card. Trash the rest.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition)
                            || CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasText("Vemmon");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent> { card.PermanentOfThisCard() }, hashtable).Tap());

                    if (card.Owner.LibraryCards.Count >= 1)
                    {
                        List<Permanent> playedPermanents = new List<Permanent>();

                        foreach (Hashtable hash in CardEffectCommons.GetHashtablesFromHashtable(hashtable))
                        {
                            playedPermanents.Add(CardEffectCommons.GetPermanentFromHashtable(hash));
                        }

                        List<Permanent> targetPermanents = playedPermanents.Filter(PermanentCondition);

                        List<CardSource> selectedCards = new List<CardSource>();

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndProcessForAll(
                            revealCount: 2,
                            simplifiedSelectCardCondition:
                            new SimplifiedSelectCardConditionClass(
                                    canTargetCondition: cardSource => cardSource.EqualsCardName("Vemmon"),
                                    message: "",
                                    mode: SelectCardEffect.Mode.Custom,
                                    maxCount: -1,
                                    selectCardCoroutine: SelectCardCoroutine),
                            remainingCardsPlace: RemainingCardsPlace.Trash,
                            activateClass: activateClass
                        ));

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            if (cardSource != null) selectedCards.Add(cardSource);
                            yield return null;
                        }

                        if (selectedCards.Count > 0)
                        {
                            while (selectedCards.Any())
                            {
                                Permanent selectedPermament = null;
                                if (targetPermanents.Count > 1)
                                {
                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: permanent => targetPermanents.Contains(permanent),
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
                                        selectedPermament = permanent;
                                        yield return null;
                                    }
                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get the digivolution cards.", "The opponent is selecting 1 Digimon that will get the digivolution cards.");
                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                                }
                                else selectedPermament = targetPermanents[0];

                                if (selectedPermament != null)
                                {
                                    List<CardSource> digivolutionCards_fixed = new List<CardSource>();
                                    digivolutionCards_fixed.Clear(); // Clear here in cases of multiple loops
                                    if (selectedCards.Count > 1)
                                    {
                                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                        selectCardEffect.SetUp(
                                            canTargetCondition: (cardSource) => true,
                                            canTargetCondition_ByPreSelecetedList: null,
                                            canEndSelectCondition: CanEndSelectCondition,
                                            canNoSelect: () => false,
                                            selectCardCoroutine: null,
                                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                            message: "Specify the order to place the cards in the digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                                            maxCount: selectedCards.Count,
                                            canEndNotMax: true,
                                            isShowOpponent: true,
                                            mode: SelectCardEffect.Mode.Custom,
                                            root: SelectCardEffect.Root.Custom,
                                            customRootCardList: selectedCards,
                                            canLookReverseCard: true,
                                            selectPlayer: card.Owner,
                                            cardEffect: activateClass);

                                        selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Cards");

                                        bool CanEndSelectCondition(List<CardSource> cardSources)
                                        {
                                            return !CardEffectCommons.HasNoElement(cardSources);
                                        }

                                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                                        {
                                            digivolutionCards_fixed.AddRange(cardSources);
                                            selectedCards.RemoveAll(cardSources.Contains);
                                            yield return null;
                                        }

                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(digivolutionCards_fixed, "Digivolution Cards", true, true));
                                        yield return ContinuousController.instance.StartCoroutine(selectedPermament.AddDigivolutionCardsBottom(digivolutionCards_fixed, activateClass));
                                    }
                                    else
                                    {
                                        digivolutionCards_fixed.AddRange(selectedCards);
                                        selectedCards.Clear();

                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(digivolutionCards_fixed, "Digivolution Cards", true, true));
                                        yield return ContinuousController.instance.StartCoroutine(selectedPermament.AddDigivolutionCardsBottom(digivolutionCards_fixed, activateClass));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}
