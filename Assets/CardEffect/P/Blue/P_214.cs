using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Betamon (X Antibody)
namespace DCGO.CardEffects.P
{
    public class P_214 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Betamon") || targetPermanent.TopCard.EqualsCardName("ModokiBetamon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 0,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region Decode

            if (timing == EffectTiming.WhenRemoveField)
            {
                bool SourceCondition(CardSource source)
                {
                    return source.EqualsCardName("Betamon") | source.EqualsCardName("ModokiBetamon");
                }

                string[] decodeStrings = { "(Betamon/ModokiBetamon)", "Betamon or ModokiBetamon" };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(card: card, isInheritedEffect: false, decodeStrings: decodeStrings, sourceCondition: SourceCondition, condition: null));
            }

            #endregion

            #region Shared OP / WD

            string SharedEffectName() => "By placing this digimon as bottom source, bottom deck 1 Digimon";

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] By placing this Digimon as the bottom digivolution card of any of your other Digimon with [Seadramon] in its text, return 1 of your opponent's Digimon with as high or lower a level as 1 of your Digimon with [Seadramon] in its text to the bottom of the deck.";
            }

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card) &&
                    CardEffectCommons.HasMatchConditionPermanent(CanTuckTargetCondition);
            }

            bool CanTuckTargetCondition(Permanent permanent)
            {
                return CanSelectOwnSeadramonInName(permanent) &&
                    permanent != card.PermanentOfThisCard();
            }

            bool CanSelectOwnSeadramonInName(Permanent permanent)
            {
                return CardEffectCommons.IsOwnerPermanent(permanent, card) &&
                    permanent.TopCard.HasText("Seadramon");
            }

            bool CanSelectEnemyDigimon(Permanent permanent, int level)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                    permanent.TopCard.HasLevel &&
                    permanent.TopCard.Level <= level;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool added = false;

                #region tuck this card under a Seadramon in Text

                if (CardEffectCommons.HasMatchConditionPermanent(CanTuckTargetCondition))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanTuckTargetCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanTuckTargetCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get a digivolution card.", "The opponent is selecting 1 Digimon that will get a digivolution card.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        Permanent selectedPermanent = permanent;

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(new List<Permanent[]>() { new Permanent[] { card.PermanentOfThisCard(), selectedPermanent } }, false, activateClass).PlacePermanentToDigivolutionCards());

                            if (CardEffectCommons.IsExistOnBattleArea(card))
                            {
                                if (selectedPermanent.DigivolutionCards.Contains(card))
                                {
                                    added = true;
                                }
                            }
                        }
                    }
                }

                #endregion

                int level = -1;

                #region Select a Seadramon in name to set level

                if (added)
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectOwnSeadramonInName));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOwnSeadramonInName,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to dottom deck a Digimon with as high or lower level.", "The opponent is selecting 1 Digimon to bottom deck.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        Permanent selectedPermanent = permanent;

                        if (selectedPermanent != null)
                        {
                           level = selectedPermanent.TopCard.Level;
                        }

                        yield return null;
                    }
                }

                #endregion

                #region Bottom Deck opponent digimon

                if (level > 0)
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount((permanent) => CanSelectEnemyDigimon(permanent, level)));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: (permanent) => CanSelectEnemyDigimon(permanent, level),
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to dottom deck.", "The opponent is selecting 1 Digimon to bottom deck.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(new List<Permanent>() { permanent }, hashtable).DeckBounce());                         
                    }
                }

                #endregion
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition,(hash) => SharedActivateCoroutine(hash, activateClass), -1, true, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition,(hash) => SharedActivateCoroutine(hash, activateClass), -1, true, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }
            #endregion

            #region ESS

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Prevent this Digimon from leaving the battle area by trashing 2 same-level digivolution cards.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Substitute_P_214");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                 => "[All Turns] When this Digimon with [Seadramon] in its text would leave the battle area by your opponent's effects, by trashing 2 same-level cards in its digivolution cards, it doesn't leave.";

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (!cardSource.CanNotTrashFromDigivolutionCards(activateClass) &&
                        cardSource.HasLevel &&
                        CardEffectCommons.IsExistOnBattleArea(card) && 
                        card.PermanentOfThisCard().DigivolutionCards.Contains(card))
                    {
                        foreach (CardSource cardSource1 in card.PermanentOfThisCard().DigivolutionCards)
                        {
                            if (cardSource != cardSource1 &&
                                cardSource1.HasLevel &&
                                !cardSource1.CanNotTrashFromDigivolutionCards(activateClass) &&
                                cardSource.Level == cardSource1.Level)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                        CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card) &&
                        CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOpponentEffect(cardEffect, card)) &&
                        card.PermanentOfThisCard().TopCard.HasText("Seadramon");
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                        card.PermanentOfThisCard().DigivolutionCards.Any(CanSelectCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        Permanent selectedPermanent = card.PermanentOfThisCard();

                        if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition) >= 2)
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            int maxCount = 2;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectCardCondition,
                                        canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                        canEndSelectCondition: CanEndSelectCondition,
                                        canNoSelect: () => true,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select cards to discard.",
                                        maxCount: maxCount,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Custom,
                                        customRootCardList: selectedPermanent.DigivolutionCards,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                            selectCardEffect.SetNotShowCard();
                            yield return StartCoroutine(selectCardEffect.Activate());

                            bool CanEndSelectCondition(List<CardSource> cardSources)
                            {
                                if (CardEffectCommons.HasNoElement(cardSources))
                                {
                                    return false;
                                }

                                List<int> levels = cardSources
                                .Map(cardSource1 => cardSource1.Level)
                                .Distinct()
                                .ToList();

                                if (levels.Count > 1)
                                {
                                    return false;
                                }

                                return true;
                            }

                            bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                            {
                                List<int> levels = cardSources
                                .Map(cardSource1 => cardSource1.Level)
                                .Concat(new List<int>() { cardSource.Level })
                                .Distinct()
                                .ToList();

                                if (levels.Count > 1)
                                {
                                    return false;
                                }

                                return true;
                            }

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);
                                yield return null;
                            }

                            if (selectedCards.Count >= 1)
                            {
                                if (selectedCards.Count == 2)
                                {
                                    selectedPermanent.HideDeleteEffect();
                                    selectedPermanent.HideHandBounceEffect();
                                    selectedPermanent.HideDeckBounceEffect();
                                    selectedPermanent.HideWillRemoveFieldEffect();

                                    selectedPermanent.DestroyingEffect = null;
                                    selectedPermanent.HandBounceEffect = null;
                                    selectedPermanent.LibraryBounceEffect = null;
                                    selectedPermanent.willBeRemoveField = false;
                                }

                                yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(
                                    selectedPermanent,
                                    selectedCards,
                                    activateClass).TrashDigivolutionCards());
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