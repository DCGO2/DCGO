using System;
using System.Collections;
using System.Collections.Generic;

// Bagramon
namespace DCGO.CardEffects.EX10
{
    public class EX10_056 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region DigiXros

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
                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                if (cardSource.Owner == card.Owner)
                                {
                                    if (cardSource.IsDigimon)
                                    {
                                        if (cardSource.CardTraits.Contains("BagraArmy"))
                                        {
                                            return true;
                                        }

                                        if (cardSource.CardTraits.Contains("Bagra Army"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        DigiXrosConditionElement element = new DigiXrosConditionElement(CanSelectCardCondition, "[Bagra Army] Digimon");
                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>();

                        for (int i = 0; i < 2; i++)
                        {
                            elements.Add(element);
                        }

                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, null, 2);

                        return digiXrosCondition;
                    }

                    return null;
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 opponent digimon under a digimon or tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] You may place 1 of your opponent's Digimon as any of their other Digimon's bottom digivolution card or under any of their Tamers.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && HasValidEnemyPermanents();
                }

                bool HasValidEnemyPermanents()
                {
                    int digimon = card.Owner.Enemy.GetBattleAreaDigimons().Count;
                    int notOptions = card.Owner.Enemy.GetBattleAreaPermanents().Filter(x => !x.IsOption).Count;

                    return digimon >= 1 &&
                           notOptions >= 2;
                }

                bool IsEnemyDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsEnemyDigimon))
                    {
                        Permanent selectedPermamentToSource = null;

                        #region Select Enemy Digimon to add as Source

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsEnemyDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermamentToSource = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to move under a digimon/tamer", "The opponent is selecting 1 Digimon move under a digimon/tamer");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        #endregion

                        if (selectedPermamentToSource != null)
                        {
                            bool IsEnemyPermanent(Permanent permanent)
                            {
                                return (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                                    || CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaTamer(permanent, card))
                                    && permanent != selectedPermamentToSource;
                            }

                            if (CardEffectCommons.HasMatchConditionPermanent(IsEnemyPermanent))
                            {
                                Permanent selectedPermanent = null;

                                #region Select Enemy Permament to add source

                                SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();
                                int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsEnemyPermanent));

                                selectPermanentEffect1.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: IsEnemyPermanent,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount1,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine1,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                IEnumerator SelectPermanentCoroutine1(Permanent permanent)
                                {
                                    selectedPermanent = permanent;
                                    yield return null;
                                }

                                selectPermanentEffect1.SetUpCustomMessage("Select 1 Digimon or tamer to add the source card underneath", "The opponent is selecting 1 Digimon or tamer to add the source card underneath");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                                #endregion

                                if (selectedPermanent != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(
                                        permanentArrays: new List<Permanent[]>() { new Permanent[] { selectedPermamentToSource, selectedPermanent } },
                                        toTop: false,
                                        cardEffect: activateClass).PlacePermanentToDigivolutionCards());
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 opponent digimon under a digimon or tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may place 1 of your opponent's Digimon as any of their other Digimon's bottom digivolution card or under any of their Tamers.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && HasValidEnemyPermanents();
                }

                bool HasValidEnemyPermanents()
                {
                    int digimon = card.Owner.Enemy.GetBattleAreaDigimons().Count;
                    int notOptions = card.Owner.Enemy.GetBattleAreaPermanents().Filter(x => !x.IsOption).Count;

                    return digimon >= 1 &&
                           notOptions >= 2;
                }

                bool IsEnemyDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsEnemyDigimon))
                    {
                        Permanent selectedPermamentToSource = null;

                        #region Select Enemy Digimon to add as Source

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsEnemyDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermamentToSource = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to move under a digimon/tamer", "The opponent is selecting 1 Digimon move under a digimon/tamer");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        #endregion

                        if (selectedPermamentToSource != null)
                        {
                            bool IsEnemyPermanent(Permanent permanent)
                            {
                                return (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                                    || CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaTamer(permanent, card))
                                    && permanent != selectedPermamentToSource;
                            }

                            if (CardEffectCommons.HasMatchConditionPermanent(IsEnemyPermanent))
                            {
                                Permanent selectedPermanent = null;

                                #region Select Enemy Permament to add source

                                SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();
                                int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsEnemyPermanent));

                                selectPermanentEffect1.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: IsEnemyPermanent,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount1,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine1,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                IEnumerator SelectPermanentCoroutine1(Permanent permanent)
                                {
                                    selectedPermanent = permanent;
                                    yield return null;
                                }

                                selectPermanentEffect1.SetUpCustomMessage("Select 1 Digimon or tamer to add the source card underneath", "The opponent is selecting 1 Digimon or tamer to add the source card underneath");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                                #endregion

                                if (selectedPermanent != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(
                                        permanentArrays: new List<Permanent[]>() { new Permanent[] { selectedPermamentToSource, selectedPermanent } },
                                        toTop: false,
                                        cardEffect: activateClass).PlacePermanentToDigivolutionCards());
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region All Turns - Once Per Turn

            #region Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 2 source cards, trash opponent top security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("EX10_056_AllTurns");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When any of your opponent's Digimon or Tamers digivolve or effects place cards under them, by trashing any 2 of this Digimon's digivolution cards, trash your opponent's top security card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermamentCondition);
                }

                bool PermamentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.PermanentOfThisCard().DigivolutionCards.Count >= 2;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.PermanentOfThisCard().DigivolutionCards.Count >= 2)
                    {
                        Permanent thisPermanent = card.PermanentOfThisCard();
                        List<CardSource> selectedCards = new List<CardSource>();

                        #region Trash Digivolution Cards

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: _ => true,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 2 digivolution cards to trash.",
                                    maxCount: 2,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: thisPermanent.DigivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        selectCardEffect.SetUseFaceDown();
                        selectCardEffect.SetUpCustomMessage("Select 2 digivolution cards to trash.", "The opponent is selecting 2 digivolution cards to trash.");
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        #endregion

                        if (selectedCards.Count == 2)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsAndProcessAccordingToResult(
                                targetPermanent: thisPermanent,
                                targetDigivolutionCards: selectedCards,
                                activateClass: activateClass,
                                successProcess: SuccessProcess,
                                failureProcess: null));

                            IEnumerator SuccessProcess(List<CardSource> trashedSources)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                    player: card.Owner.Enemy,
                                    destroySecurityCount: 1,
                                    cardEffect: activateClass,
                                    fromTop: true).DestroySecurity());
                            }
                        }
                    }
                    yield return null;
                }
            }

            #endregion

            #region Add Digivolution Cards

            if (timing == EffectTiming.OnAddDigivolutionCards)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 2 source cards, trash opponent top security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("EX10_056_AllTurns");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When any of your opponent's Digimon or Tamers digivolve or effects place cards under them, by trashing any 2 of this Digimon's digivolution cards, trash your opponent's top security card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnAddDigivolutionCard(hashtable, PermamentCondition, cardEffect => cardEffect.EffectSourceCard != null, null);
                }

                bool PermamentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.PermanentOfThisCard().DigivolutionCards.Count >= 2;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.PermanentOfThisCard().DigivolutionCards.Count >= 2)
                    {
                        Permanent thisPermanent = card.PermanentOfThisCard();
                        List<CardSource> selectedCards = new List<CardSource>();

                        #region Trash Digivolution Cards

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: _ => true,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 2 digivolution cards to trash.",
                                    maxCount: 2,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: thisPermanent.DigivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        selectCardEffect.SetUpCustomMessage("Select 2 digivolution cards to trash.", "The opponent is selecting 2 digivolution cards to trash.");
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        #endregion

                        if (selectedCards.Count == 2)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsAndProcessAccordingToResult(
                                targetPermanent: thisPermanent,
                                targetDigivolutionCards: selectedCards,
                                activateClass: activateClass,
                                successProcess: SuccessProcess,
                                failureProcess: null));

                            IEnumerator SuccessProcess(List<CardSource> trashedSources)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                    player: card.Owner.Enemy,
                                    destroySecurityCount: 1,
                                    cardEffect: activateClass,
                                    fromTop: true).DestroySecurity());
                            }
                        }
                    }
                    yield return null;
                }
            }

            #endregion

            #endregion

            return cardEffects;
        }
    }
}