using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// Tactimon
namespace DCGO.CardEffects.EX10
{
    public class EX10_055 : CEntity_Effect
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
                                        if (cardSource.EqualsTraits("BagraArmy"))
                                        {
                                            return true;
                                        }

                                        if (cardSource.EqualsTraits("Bagra Army"))
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
                activateClass.SetUpICardEffect("Choose 1 of your digimon, delete it and 1 opponent digimon with equal or less level", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] You may choose 1 of your Digimon. Delete the chosen Digimon and 1 of your opponent's Digimon with as high or lower a level as it.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool IsYourDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsYourDigimon))
                    {
                        List<Permanent> permanentList = new List<Permanent>();

                        #region Select Your Digimon to Destroy

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, IsYourDigimon));

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsYourDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            permanentList.Add(permanent);
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to destroy", "The opponent is selecting 1 Digimon to destroy");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        #endregion

                        if (permanentList.Any())
                        {
                            bool CanSelectOpponentDigimon(Permanent permanent)
                            {
                                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                                    && permanent.TopCard.HasLevel
                                    && permanent.TopCard.Level <= permanentList[0].Level;
                            }

                            if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectOpponentDigimon))
                            {
                                #region Select Opponent Digimon to Destroy

                                SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();
                                int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, CanSelectOpponentDigimon));

                                selectPermanentEffect1.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectOpponentDigimon,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount1,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine1,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                IEnumerator SelectPermanentCoroutine1(Permanent permanent)
                                {
                                    permanentList.Add(permanent);
                                    yield return null;
                                }

                                selectPermanentEffect1.SetUpCustomMessage("Select 1 Digimon to destroy", "The opponent is selecting 1 Digimon to destroy");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                                #endregion
                            }

                            yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                                            destroyTargetPermanents: permanentList,
                                            hashtable: CardEffectCommons.OnDeletionHashtable(
                                            permanentList,
                                            activateClass,
                                            null,
                                            false
                                            )).Destroy()
                            );
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Choose 1 of your digimon, delete it and 1 opponent digimon with equal or less level", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may choose 1 of your Digimon. Delete the chosen Digimon and 1 of your opponent's Digimon with as high or lower a level as it.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool IsYourDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsYourDigimon))
                    {
                        List<Permanent> permanentList = new List<Permanent>();

                        #region Select Your Digimon to Destroy

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, IsYourDigimon));

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsYourDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            permanentList.Add(permanent);
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to destroy", "The opponent is selecting 1 Digimon to destroy");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        #endregion

                        if (permanentList.Any())
                        {
                            bool CanSelectOpponentDigimon(Permanent permanent)
                            {
                                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                                    && permanent.TopCard.HasLevel
                                    && permanent.TopCard.Level <= permanentList[0].Level;
                            }

                            if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectOpponentDigimon))
                            {
                                #region Select Opponent Digimon to Destroy

                                SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();
                                int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, CanSelectOpponentDigimon));

                                selectPermanentEffect1.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectOpponentDigimon,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount1,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine1,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                IEnumerator SelectPermanentCoroutine1(Permanent permanent)
                                {
                                    permanentList.Add(permanent);
                                    yield return null;
                                }

                                selectPermanentEffect1.SetUpCustomMessage("Select 1 Digimon to destroy", "The opponent is selecting 1 Digimon to destroy");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                                #endregion
                            }

                            yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                                            destroyTargetPermanents: permanentList,
                                            hashtable: CardEffectCommons.OnDeletionHashtable(
                                            permanentList,
                                            activateClass,
                                            null,
                                            false
                                            )).Destroy()
                            );
                        }
                    }
                }
            }

            #endregion

            #region All Turns - Once Per Turn

            if (timing == EffectTiming.WhenRemoveField)
            {
                List<Permanent> removedPermanents = new List<Permanent>();

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 2 source cards, prevent deletion", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("EX10_055_Subsitute");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When any of your [Bagra Army] trait Digimon would leave the battle area by effects, by trashing any 2 of this Digimon's digivolution cards, they don't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, IsBagraArmyDigimon)
                        && CardEffectCommons.IsByEffect(hashtable, effect => effect != null);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    removedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable).Filter(IsBagraArmyDigimon);

                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.PermanentOfThisCard().DigivolutionCards.Count >= 2;
                }

                bool IsBagraArmyDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasBagraArmyTraits;
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
                                foreach (Permanent permanent in removedPermanents)
                                {
                                    #region Remove Events from Permanent

                                    permanent.HideDeleteEffect();
                                    permanent.HideHandBounceEffect();
                                    permanent.HideDeckBounceEffect();
                                    permanent.HideWillRemoveFieldEffect();

                                    permanent.DestroyingEffect = null;
                                    permanent.IsDestroyedByBattle = false;
                                    permanent.HandBounceEffect = null;
                                    permanent.LibraryBounceEffect = null;
                                    permanent.willBeRemoveField = false;

                                    #endregion
                                }

                                yield return null;
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
