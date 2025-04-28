using System;
using System.Collections;
using System.Collections.Generic;

//Shoutmon DX
namespace DCGO.CardEffects.BT21
{
    public class BT21_027 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.EqualsCardName("ZeigGreymon"))
                    {
                        return true;
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.Level == 5 && targetPermanent.TopCard.EqualsTraits("Xros Heart"))
                    {
                        return true;
                    }
                    return false;
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

            #region DigiXros

            if (timing == EffectTiming.None)
            {
                AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
                addDigiXrosConditionClass.SetUpICardEffect($"DigiXros -3", CanUseCondition, card);
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
                        DigiXrosConditionElement element = new DigiXrosConditionElement(CanSelectCardCondition1, "[OmniShoutmon]");

                        bool CanSelectCardCondition1(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                if (cardSource.Owner == card.Owner)
                                {
                                    if (cardSource.IsDigimon)
                                    {
                                        if (cardSource.EqualsCardNameDigiXros("OmniShoutmon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        DigiXrosConditionElement element2 = new DigiXrosConditionElement(CanSelectCardCondition2, "[ZeigGreymon]");

                        bool CanSelectCardCondition2(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                if (cardSource.Owner == card.Owner)
                                {
                                    if (cardSource.IsDigimon)
                                    {
                                        if (cardSource.EqualsCardNameDigiXros("ZeigGreymon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>() { element, element2 };

                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, null, 3);

                        return digiXrosCondition;
                    }

                    return null;
                }
            }

            #endregion

            #region DigiXros Name

            if (timing == EffectTiming.None)
            {
                ChangeCardNamesForDigiXrosClass changeCardNamesForDigiXrosClass = new ChangeCardNamesForDigiXrosClass();
                changeCardNamesForDigiXrosClass.SetUpICardEffect("Also treated as [OmniShoutmon]/[ZeigGreymon] for a DigiXros", CanUseCondition, card);
                changeCardNamesForDigiXrosClass.SetUpChangeCardNamesForDigiXrosClass(changeCardNames: ChangeCardNames);
                cardEffects.Add(changeCardNamesForDigiXrosClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                List<string> ChangeCardNames(CardSource cardSource, List<string> cardNames)
                {
                    if (cardSource == card)
                    {
                        cardNames.Add("OmniShoutmon");
                        cardNames.Add("ZeigGreymon");
                    }

                    return cardNames;
                }
            }

            #endregion

            #region Sec +1

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(
                changeValue: 1,
                isInheritedEffect: false,
                card: card,
                condition: null));
            }

            #endregion

            #endregion

            #region On Play / When Digivolving Shared

            bool SharedPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy))
                    {
                        return true;
                    }
                }
                return false;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(SharedPermanentCondition))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(SharedPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: SharedPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Destroy lowest DP digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Delete 1 of your opponent's lowest DP Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPlay(hashtable, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(SharedPermanentCondition))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Destroy lowest DP digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Delete 1 of your opponent's lowest DP Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(SharedPermanentCondition))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place up to 4 [Xros Heart]/[Blue Flare] under tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When this Digimon would leave the battle area, you may place up to 4 [Xros Heart]/[Blue Flare] trait Digimon cards from its digivolution cards under 1 of your Tamers.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
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
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTamer))
                        {
                            if (card.PermanentOfThisCard().DigivolutionCards.Filter(x => CanSelectSources(x)) != null)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanSelectTamer(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.IsTamer)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanSelectSources(CardSource source)
                {
                    if (source.IsDigimon)
                    {
                        if (source.EqualsTraits("Xros Heart") || source.EqualsTraits("Blue Flare"))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTamer) && card.PermanentOfThisCard().DigivolutionCards.Filter(x => CanSelectSources(x)) != null)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();
                        int maxCount = Math.Min(4, card.PermanentOfThisCard().DigivolutionCards.Filter(x => CanSelectSources(x)).Count);
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                        selectCardEffect.SetUp(
                                canTargetCondition: CanSelectSources,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select up to 4 sources to save.",
                                maxCount: maxCount,
                                canEndNotMax: true,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass
                            );

                        selectCardEffect.SetUpCustomMessage("Select sources to save.", "The opponent is selecting sources to save.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card/Cards");

                        yield return StartCoroutine(selectCardEffect.Activate());
                        IEnumerator SelectCardCoroutine(CardSource source)
                        {
                            selectedCards.Add(source);
                            yield return null;
                        }

                        if (selectedCards.Count > 0)
                        {
                            Permanent permanent = null;
                            int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, CanSelectTamer));
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectTamer,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SekectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 tamer to place the selected sources under.", "The opponent is selecting 1 tamer to place the selected sources under.");
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SekectPermanentCoroutine(Permanent selectedPermanent)
                            {
                                permanent = selectedPermanent;
                                yield return null;
                            }

                            if (permanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(permanent.AddDigivolutionCardsBottom(selectedCards, activateClass));
                            }
                        }
                    }

                    yield return null;
                }
            }

            #endregion

            return cardEffects;
        }
    }
}