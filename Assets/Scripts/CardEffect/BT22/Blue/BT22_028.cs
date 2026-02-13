using System;
using System.Collections;
using System.Collections.Generic;

// Ariemon
namespace DCGO.CardEffects.BT22
{
    public class BT22_028 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Decode

            if (timing == EffectTiming.WhenRemoveField)
            {
                bool SourceCondition(CardSource source)
                {
                    return source.IsDigimon
                    && source.HasLevel && source.Level <= 6
                    && (source.HasSeaAnimalTraits || source.HasAquaTraits);
                }

                string[] decodeStrings = { "(Lv.6 or lower w/[Aqua]/[Sea Animal] trait)", "Level 6 or lower Digimon card with [Aqua] or [Sea Animal] " };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(card: card, isInheritedEffect: false, decodeStrings: decodeStrings, sourceCondition: SourceCondition, condition: null));
            }

            #endregion

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasYaoQinglan);
                }

                bool HasYaoQinglan(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card) &&
                           targetPermanent.TopCard.EqualsCardName("Yao Qinglan");
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(targetPermanent, card)
                        && targetPermanent.TopCard.EqualsCardName("MarineBullmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 6,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: Condition)
                );
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 of each level (3, 4, and 5)", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may play 1 each of level 3, level 4 and level 5 Digimon cards with [Aqua] or [Sea Animal] in any of their traits from this Digimon's digivolution cards without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool DigimonCondition(int level, CardSource cardSource)
                {
                    return CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass)
                        && cardSource.IsDigimon
                        && cardSource.HasLevel && cardSource.Level == level
                        && (cardSource.HasAquaTraits);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();
                    Permanent thisPermanent = card.PermanentOfThisCard();

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);
                        yield return null;
                    }

                    #region Select Level 3 Digivolution Cards

                    int maxCount = Math.Min(1, thisPermanent.DigivolutionCards.Filter(x => DigimonCondition(3, x)).Count);
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                                canTargetCondition: cs => DigimonCondition(3, cs),
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 level 3 digivolution card to play.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: thisPermanent.DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 level 3 digivolution card to play.", "The opponent is selecting 1 level 3 digivolution card to play.");

                    yield return StartCoroutine(selectCardEffect.Activate());

                    #endregion

                    #region Select Level 4 Digivolution Cards

                    int maxCount1 = Math.Min(1, thisPermanent.DigivolutionCards.Filter(x => DigimonCondition(4, x)).Count);
                    SelectCardEffect selectCardEffect1 = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect1.SetUp(
                                canTargetCondition: cs => DigimonCondition(4, cs),
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 level 4 digivolution card to play.",
                                maxCount: maxCount1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: thisPermanent.DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                    selectCardEffect1.SetUpCustomMessage("Select 1 level 4 digivolution card to play.", "The opponent is selecting 1 level 4 digivolution card to play.");

                    yield return StartCoroutine(selectCardEffect1.Activate());

                    #endregion

                    #region Select Level 3 Digivolution Cards

                    int maxCount2 = Math.Min(1, thisPermanent.DigivolutionCards.Filter(x => DigimonCondition(5, x)).Count);
                    SelectCardEffect selectCardEffect2 = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect2.SetUp(
                                canTargetCondition: cs => DigimonCondition(5, cs),
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 level 5 digivolution card to play.",
                                maxCount: maxCount2,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: thisPermanent.DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                    selectCardEffect1.SetUpCustomMessage("Select 1 level 5 digivolution card to play.", "The opponent is selecting 1 level 5 digivolution card to play.");

                    yield return StartCoroutine(selectCardEffect1.Activate());

                    #endregion

                    if (selectedCards.Count > 0) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.DigivolutionCards, activateETB: true));
                }
            }

            #endregion

            #region When Digivolving OPT

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 digimon as bottom source, bottom deck 1 digimon & unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT22_028_BottomDeck");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] [Once Per Turn] By placing 1 of your other Digimon as this Digimon's bottom digivolution card, return 1 of your opponent's Digimon to the bottom of the deck and this Digimon unsuspends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, DigimonCondition);
                }

                bool DigimonCondition(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(targetPermanent, card)
                        && targetPermanent != card.PermanentOfThisCard();
                }

                bool OpponentDigimonCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, DigimonCondition))
                    {
                        Permanent selectedPermanent = null;

                        #region Select Owner Other Permanent

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, DigimonCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: DigimonCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }
                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to add digivolution source.", "The opponent is selecting 1 Digimon to add digivolution source.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        #endregion

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(new List<Permanent[]>() { new Permanent[] { selectedPermanent, card.PermanentOfThisCard() } }, false, activateClass).PlacePermanentToDigivolutionCards());

                            #region Bottom Deck Opponent Digimon

                            if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, OpponentDigimonCondition))
                            {
                                int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, OpponentDigimonCondition));
                                SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect1.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: OpponentDigimonCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount1,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                                    cardEffect: activateClass);

                                selectPermanentEffect1.SetUpCustomMessage("Select 1 digimon to bottom deck.", "The opponent is selecting a digimon to bottom deck.");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());
                            }

                            #endregion

                            yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                        }
                    }
                }
            }

            #endregion

            #region When Attacking OPT

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 digimon as bottom source, bottom deck 1 digimon & unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT22_028_BottomDeck");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] By placing 1 of your other Digimon as this Digimon's bottom digivolution card, return 1 of your opponent's Digimon to the bottom of the deck and this Digimon unsuspends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, DigimonCondition);
                }

                bool DigimonCondition(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(targetPermanent, card)
                        && targetPermanent != card.PermanentOfThisCard();
                }

                bool OpponentDigimonCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, DigimonCondition))
                    {
                        Permanent selectedPermanent = null;

                        #region Select Owner Other Permanent

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, DigimonCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: DigimonCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }
                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to add digivolution source.", "The opponent is selecting 1 Digimon to add digivolution source.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        #endregion

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(new List<Permanent[]>() { new Permanent[] { selectedPermanent, card.PermanentOfThisCard() } }, false, activateClass).PlacePermanentToDigivolutionCards());

                            #region Bottom Deck Opponent Digimon

                            if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, OpponentDigimonCondition))
                            {
                                int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, OpponentDigimonCondition));
                                SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect1.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: OpponentDigimonCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount1,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                                    cardEffect: activateClass);

                                selectPermanentEffect1.SetUpCustomMessage("Select 1 digimon to bottom deck.", "The opponent is selecting a digimon to bottom deck.");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());
                            }

                            #endregion

                            yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}