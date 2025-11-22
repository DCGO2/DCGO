using System.Collections;
using System.Collections.Generic;

//King Drasil_7D6
namespace DCGO.CardEffects.BT23
{
    public class BT23_072 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Hand - Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<Draw 1>", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsDigimonEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Hand][Main] By paying 3 cost and placing this card as the bottom digivolution card of your [King Drasil_7D6] or [Mother Eater] in the breeding area, <Draw 1>.";
                }

                bool IsProperDigimonInBreeding(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBreedingArea(targetPermanent, card) &&
                           (targetPermanent.TopCard.EqualsCardName("King Drasil_7D6") || targetPermanent.TopCard.EqualsCardName("Mother Eater"));
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnHand(card) &&
                           CardEffectCommons.HasMatchConditionOwnersBreedingPermanent(card, IsProperDigimonInBreeding);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = card.Owner.GetBreedingAreaPermanents()[0];

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(-3, activateClass));

                        yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(
                                new List<CardSource>() { card },
                                activateClass));

                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    }
                }
            }
            #endregion

            #region All Turns

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspending this digimon, 1 played digimon gains <Rush>, <Raid>, <Reboot> and <Blocker>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When any of your Digimon with the [Royal Knight] or [CS] trait are played, by suspending this Digimon, 1 of the played Digimon gains <Rush>, <Raid>, <Reboot> and <Blocker> until your opponent's turn ends.";
                }

                bool PlayedPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           (permanent.TopCard.HasRoyalKnightTraits || permanent.TopCard.HasCSTraits);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PlayedPermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (!card.PermanentOfThisCard().IsSuspended)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(
                        permanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        hashtable: _hashtable).Tap());

                        if (card.PermanentOfThisCard().IsSuspended)
                        {

                            List<Permanent> playedPermanents = new List<Permanent>();
                            List<Hashtable> hashtables = CardEffectCommons.GetHashtablesFromHashtable(_hashtable);

                            if (hashtables != null)
                            {
                                foreach (Hashtable hashtable1 in hashtables)
                                {
                                    Permanent permanent = CardEffectCommons.GetPermanentFromHashtable(hashtable1);

                                    if (permanent != null)
                                        playedPermanents.Add(permanent);
                                }

                                playedPermanents = playedPermanents.Filter(PermanentCondition);
                            }

                            Permanent selectedPermanent = null;

                            bool PermanentCondition(Permanent permanent)
                            {
                                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                    && (permanent.TopCard.HasRoyalKnightTraits || permanent.TopCard.HasCSTraits)
                                    && playedPermanents.Contains(permanent);
                            }

                            if (playedPermanents.Count == 1) selectedPermanent = playedPermanents[0];
                            else
                            {
                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: PermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to gain effects.", "The opponent is selecting 1 Digimon to gain effects.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    selectedPermanent = permanent;
                                    yield return null;
                                }
                            }

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRush(
                                        targetPermanent: selectedPermanent,
                                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                        activateClass: activateClass));

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRaid(
                                        targetPermanent: selectedPermanent,
                                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                        activateClass: activateClass));

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainReboot(
                                        targetPermanent: selectedPermanent,
                                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                        activateClass: activateClass));

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(
                                        targetPermanent: selectedPermanent,
                                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                        activateClass: activateClass));
                            }
                        }
                    }
                }
            }
            #endregion

            #region ESS
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [King Drasil] from sources", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Breeding] [Start of Your Main Phase] If this Digimon has 6 or more digivolution cards, you may play 1 Digimon card with [King Drasil] in its name from its digivolution cards without paying the cost.";
                }

                bool SelectCardPlay(CardSource source)
                {
                    return source.ContainsCardName("King Drasil") &&
                           source.IsDigimon &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource: source, payCost: false, cardEffect: activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBreedingAreaDigimon(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBreedingAreaDigimon(card) &&
                           card.PermanentOfThisCard().DigivolutionCards.Count >= 6;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    List<CardSource> selectedCards = new List<CardSource>();

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                                canTargetCondition: SelectCardPlay,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 digivolution card to play.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: selectedPermanent.DigivolutionCards,
                                canLookReverseCard: false,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to play.", "The opponent is selecting 1 digivolution card to play.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return StartCoroutine(selectCardEffect.Activate());

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.DigivolutionCards, activateETB: true));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}