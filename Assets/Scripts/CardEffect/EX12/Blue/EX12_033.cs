using System.Collections;
using System.Collections.Generic;

// Amphimon // Frozen Crystal
namespace DCGO.CardEffects.EX12
{
    public class EX12_033 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Effects
            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.HasText("Jellymon")
                            || permanent.TopCard.EqualsTraits("DS");
                }
            
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, true, card, null, level: 5));
            }
            #endregion

            #region Shared WD / WA / Counter
            string SharedEffectName = "May trash up to 3 cards from hand to give 1 enemy Digimon -4K DP per card for turn";

            string SharedEffectDescription(string tag)
                => $"[{tag}] You may trash up to 3 cards in your hand. Then, to 1 of your opponent's Digimon, give -4000 DP until your turn ends for each card this effect trashed.";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    isSkippable: true,
                    whenDigivolving: true,
                    whenAttacking: true,
                    counter: true);

            bool CanSelectEnemyDigimonCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                int cardSourcesCount = 0;

                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: _ => true,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 3,
                    canNoSelect: true,
                    canEndNotMax: true,
                    isShowOpponent: true,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: AfterSelectCardCoroutine,
                    mode: SelectHandEffect.Mode.Discard,
                    cardEffect: activateClass);

                selectHandEffect.SetUpCustomMessage("Select up to 3 cards to trash.", "The opponent is selecting up to 3 cards to trash from their hand.");

                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                {
                    if (cardSources.Count >= 1)
                    {
                        cardSourcesCount = cardSources.Count;

                        yield return null;
                    }
                }

                if (cardSourcesCount >= 1
                && CardEffectCommons.HasMatchConditionPermanent(CanSelectEnemyDigimonCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectEnemyDigimonCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage($"Select 1 Digimon that will get DP -{cardSourcesCount*4000}.", $"The opponent is selecting 1 Digimon that will get DP -{cardSourcesCount*4000}.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: -cardSourcesCount * 4000, effectDuration: EffectDuration.UntilOwnerTurnEnd, activateClass: activateClass));
                    }
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Bot deck 3 cards from trash to prevent", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("EX12_033_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When any of your Digimon with [Jellymon] in their texts or the [DS] trait would leave the battle area, by returning 3 cards from your trash to the bottom of the deck, they don't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.HasText("Jellymon")
                            || permanent.TopCard.EqualsTraits("DS"));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool isUsed = false;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: _ => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        message: "Select 3 cards",
                        maxCount: 3,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage(
                        "Select 3 cards to return to the bottom of the deck.",
                        "The opponent is selecting 3 cards to return to the bottom of the deck.");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count == 3)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ReturnRevealedCardsToLibraryBottom(
                                remainingCards: cardSources,
                                activateClass: activateClass
                            ));

                            List<Permanent> removedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable).Filter(PermanentCondition);

                            foreach (Permanent permanent in removedPermanents)
                            {
                                permanent.willBeRemoveField = false;
                                permanent.HideDeleteEffect();
                                permanent.HideHandBounceEffect();
                                permanent.HideDeckBounceEffect();
                                permanent.HideWillRemoveFieldEffect();
                            }

                            isUsed = true;
                            yield return null;
                        }
                    }

                    if (!isUsed) activateClass.RemoveUse();
                }
            }
            #endregion
            #endregion 

            #region Option Effects
            #region Use Req.
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.HasText("DS");
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 4 enemy Digimon/Tamer sources, then may bounce 1 enemy sourceless Digimon/Tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] Trash any 4 cards under your opponent's Digimon or Tamers. Then, you may return 1 of your opponent's Digimon or Tamers without cards under it to the hand.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectEnemyDigiTamerPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                        && (permanent.IsDigimon
                            || permanent.IsTamer);
                }

                bool CanSelectEnemySourcelessPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                        && (permanent.IsDigimon
                            || permanent.IsTamer)
                        && permanent.DigivolutionCards.Count == 0;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if(CardEffectCommons.HasMatchConditionPermanent(CanSelectEnemyDigiTamerPermanentCondition))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                            permanentCondition: CanSelectEnemyDigiTamerPermanentCondition,
                            cardCondition: _ => true,
                            maxCount: 4,
                            canNoTrash: false,
                            isFromOnly1Permanent: false,
                            activateClass: activateClass,
                            afterSelectionCoroutine: null,
                            selectString: "Digimon or Tamer"
                        ));
                    }

                    if(CardEffectCommons.HasMatchConditionPermanent(CanSelectEnemySourcelessPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectEnemySourcelessPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Bounce,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or Tamer without cards under it to return to the hand.", "The opponent is selecting 1 Digimon or Tamer without cards under it to return to the hand.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            #region Arts Digivolution
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }
            #endregion
            #endregion

            return cardEffects;
        }
    }
}
