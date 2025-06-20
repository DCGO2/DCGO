using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT20
{
    public class BT20_058 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Delete 1 of your opponent's Digimon with a play cost of 7 or less.";
                }

                bool SelectOpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.HasPlayCost && permanent.TopCard.GetCostItself <= 7;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable,card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(SelectOpponentsDigimon);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: SelectOpponentsDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to destory.", "The opponent is selecting 1 Digimon to destroy.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Delete 1 of your opponent's Digimon with a play cost of 7 or less.";
                }

                bool SelectOpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.HasPlayCost && permanent.TopCard.GetCostItself <= 7;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(SelectOpponentsDigimon);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: SelectOpponentsDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to destory.", "The opponent is selecting 1 Digimon to destroy.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 11 cost or lower Digimon from sources", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When this Digimon would leave the battle are, you may play 1 play cost 11 or lower Digimon card with the [Machine] or [Cyborg] trait from this Digimon's digivolution cards without paying the cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           cardSource.HasPlayCost && cardSource.GetCostItself <= 11 &&
                           (cardSource.EqualsTraits("Machine") || cardSource.EqualsTraits("Cyborg")) &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.DigivolutionCards);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    int maxCount = 1;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to play.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to play.", "The opponent is selecting 1 digivolution card to play.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return StartCoroutine(selectCardEffect.Activate());

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
                        root: SelectCardEffect.Root.DigivolutionCards,
                        activateETB: true));
                }
            }
            #endregion

            #region DigiXros
            if (timing == EffectTiming.None)
            {
                AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
                addDigiXrosConditionClass.SetUpICardEffect($"DigiXros -2", CanUseCondition, card);
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
                        DigiXrosConditionElement elementRaijinmon = new DigiXrosConditionElement(SelectRaijinmonCondition, "Raijinmon");

                        bool SelectRaijinmonCondition(CardSource conditionCardSource)
                        {
                            if (conditionCardSource != null)
                            {
                                if (conditionCardSource.Owner == card.Owner)
                                {
                                    if (conditionCardSource.IsDigimon)
                                    {
                                        if (conditionCardSource.EqualsCardNameDigiXros("Raijinmon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        DigiXrosConditionElement elementFujinmon = new DigiXrosConditionElement(SelectFujinmonCondition, "Fujinmon");

                        bool SelectFujinmonCondition(CardSource conditionCardSource)
                        {
                            if (conditionCardSource != null)
                            {
                                if (conditionCardSource.Owner == card.Owner)
                                {
                                    if (conditionCardSource.IsDigimon)
                                    {
                                        if (conditionCardSource.EqualsCardNameDigiXros("Fujinmon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        DigiXrosConditionElement elementSuijinmon = new DigiXrosConditionElement(SelectSuijinmonCondition, "Suijinmon");

                        bool SelectSuijinmonCondition(CardSource conditionCardSource)
                        {
                            if (conditionCardSource != null)
                            {
                                if (conditionCardSource.Owner == card.Owner)
                                {
                                    if (conditionCardSource.IsDigimon)
                                    {
                                        if (conditionCardSource.EqualsCardNameDigiXros("Suijinmon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }


                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>() { elementRaijinmon, elementFujinmon,  elementSuijinmon};

                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, null, 2);

                        return digiXrosCondition;
                    }

                    return null;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}