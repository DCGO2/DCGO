using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT19
{
    public class BT19_070 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel4 && targetPermanent.TopCard.ContainsTraits("Composite");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region [DigiXros -1] 3 Lv.4 [Composite] trait Digimon cards w/different card numbers

            if (timing == EffectTiming.None)
            {
                AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
                addDigiXrosConditionClass.SetUpICardEffect($"DigiXros", CanUseCondition, card);
                addDigiXrosConditionClass.SetUpAddDigiXrosConditionClass(getDigiXrosCondition: GetDigiXros);
                addDigiXrosConditionClass.SetNotShowUI(true);
                cardEffects.Add(addDigiXrosConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                DigiXrosCondition GetDigiXros(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        DigiXrosConditionElement element = new DigiXrosConditionElement(CanSelectCardCondition,
                            "1 Lv.4 Digimon card with [Composite] trait");

                        bool CanSelectCardCondition(CardSource source)
                        {
                            return source && source.Owner == card.Owner &&
                                   source.IsDigimon && source.HasLevel && source.IsLevel4 &&
                                   source.CardTraits.Contains("Composite");
                        }

                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>();

                        for (int i = 0; i < 3; i++)
                        {
                            elements.Add(element);
                        }

                        bool CanTargetConditionByPreSelectedList(List<CardSource> cardSources, CardSource source)
                        {
                            List<string> cardIDs = new List<string>();

                            foreach (var cs in cardSources.Where(cs => !cardIDs.Contains(cs.CardID)))
                            {
                                cardIDs.Add(cs.CardID);
                            }

                            return !cardIDs.Contains(source.CardID);
                        }

                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, CanTargetConditionByPreSelectedList, 1);

                        return digiXrosCondition;
                    }

                    return null;
                }
            }

            #endregion

            #region On Play/ When Digivolving Shared

            bool CanSelectOwnerPermanentConditionShared(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
            }

            bool CanSelectLevelOpponentPermanentConditionShared(Permanent permanent, int level)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                       permanent.IsDigimon &&
                       permanent.TopCard.HasLevel &&
                       permanent.Level == level;
            }

            bool CanActivateConditionShared(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                       CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnerPermanentConditionShared);
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 of your Digimon to delete your opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Play] By deleting 1 of your Digimon, delete 1 of your opponent's level 3 Digimon, 1 of their level 4 Digimon, and 1 of their level 5 Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOwnerPermanentConditionShared,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 of your Digimon to delete.",
                        "The opponent is selecting 1 of their Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                targetPermanents: new List<Permanent>() { selectedPermanent }, activateClass: activateClass,
                                successProcess: _ => SuccessProcess(), failureProcess: null));
                    }

                    IEnumerator SuccessProcess()
                    {
                        List<Permanent> selectedPermanents = new List<Permanent>();

                        IEnumerator SelectTargetCoroutine(Permanent permanent)
                        {
                            selectedPermanents.Add(permanent);

                            yield return null;
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(permanent =>
                                CanSelectLevelOpponentPermanentConditionShared(permanent, 3)))
                        {
                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: permanent => CanSelectLevelOpponentPermanentConditionShared(permanent, 3),
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectTargetCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select a level 3 Digimon to delete.", "Opponent is selecting Digimon to delete.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(permanent =>
                                CanSelectLevelOpponentPermanentConditionShared(permanent, 4)))
                        {
                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: permanent => CanSelectLevelOpponentPermanentConditionShared(permanent, 4),
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectTargetCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select a level 4 Digimon to delete.", "Opponent is selecting Digimon to delete.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(permanent =>
                                CanSelectLevelOpponentPermanentConditionShared(permanent, 5)))
                        {
                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: permanent => CanSelectLevelOpponentPermanentConditionShared(permanent, 5),
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectTargetCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select a level 5 Digimon to delete.", "Opponent is selecting Digimon to delete.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }

                        if(selectedPermanents.Count > 0)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                                selectedPermanents,
                                CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 of your Digimon to delete your opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] By deleting 1 of your Digimon, delete 1 of your opponent's level 3 Digimon, 1 of their level 4 Digimon, and 1 of their level 5 Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOwnerPermanentConditionShared,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 of your Digimon to delete.",
                        "The opponent is selecting 1 of their Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                targetPermanents: new List<Permanent>() { selectedPermanent }, activateClass: activateClass,
                                successProcess: _ => SuccessProcess(), failureProcess: null));
                    }

                    IEnumerator SuccessProcess()
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(permanent =>
                                CanSelectLevelOpponentPermanentConditionShared(permanent, 3)))
                        {
                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: permanent => CanSelectLevelOpponentPermanentConditionShared(permanent, 3),
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(permanent =>
                                CanSelectLevelOpponentPermanentConditionShared(permanent, 4)))
                        {
                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: permanent => CanSelectLevelOpponentPermanentConditionShared(permanent, 4),
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(permanent =>
                                CanSelectLevelOpponentPermanentConditionShared(permanent, 5)))
                        {
                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: permanent => CanSelectLevelOpponentPermanentConditionShared(permanent, 5),
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }
                }
            }

            #endregion

            #region On Deletion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 of your Digimon to play [Machinedramon] from your trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Deletion] By deleting 1 of your level 4 or lower purple or red Digimon, you may play 1 [Machinedramon] from your trash without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanSelectOwnerPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.HasLevel && permanent.Level <= 4 &&
                           (permanent.TopCard.CardColors.Contains(CardColor.Purple) ||
                            permanent.TopCard.CardColors.Contains(CardColor.Red));
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.EqualsCardName("Machinedramon") &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnerPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOwnerPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 of your Digimon to delete.",
                        "The opponent is selecting 1 of their Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                targetPermanents: new List<Permanent>() { selectedPermanent }, activateClass: activateClass,
                                successProcess: _ => SuccessProcess(), failureProcess: null));
                    }

                    IEnumerator SuccessProcess()
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to play.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Trash,
                            activateETB: true));
                    }
                }
            }

            #endregion

            #region Security Attack - ESS

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: true, card: card,
                    condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}