using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT12
{
    public class BT12_109 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);

                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsTamer && permanent.TopCard.CardTraits.Contains("Hunter")))
                    {
                        return true;
                    }

                    return false;
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        return true;
                    }

                    return false;
                }
            }
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] Digivolve 1 of your Digimon into a Digimon card with <Save> in its text under one of your Tamers for its digivolution cost.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            if (CardEffectCommons.IsExistOnBattleArea(cardSource))
                            {
                                if (cardSource.Owner == permanent.TopCard.Owner)
                                {
                                    if (cardSource.PermanentOfThisCard().IsTamer)
                                    {
                                        if (cardSource.PermanentOfThisCard().DigivolutionCards.Contains(cardSource))
                                        {
                                            if (cardSource.IsDigimon)
                                            {
                                                if (cardSource.HasSaveText)
                                                {
                                                    if (cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, true, activateClass, root: SelectCardEffect.Root.DigivolutionCards))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        bool CanSelectPermanentCondition1(Permanent permanent1)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent1, card))
                            {
                                if (permanent1.IsTamer)
                                {
                                    if (permanent1.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        Permanent selectedPermanent = null;

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to digivolve.", "The opponent is selecting 1 Digimon to digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            bool CanSelectCardCondition(CardSource cardSource)
                            {
                                if (CardEffectCommons.IsExistOnBattleArea(cardSource))
                                {
                                    if (cardSource.Owner == selectedPermanent.TopCard.Owner)
                                    {
                                        if (cardSource.PermanentOfThisCard().IsTamer)
                                        {
                                            if (cardSource.PermanentOfThisCard().DigivolutionCards.Contains(cardSource))
                                            {
                                                if (cardSource.IsDigimon)
                                                {
                                                    if (cardSource.HasSaveText)
                                                    {
                                                        if (cardSource.CanPlayCardTargetFrame(selectedPermanent.PermanentFrame, true, activateClass, root: SelectCardEffect.Root.DigivolutionCards))
                                                        {
                                                            return true;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                return false;
                            }

                            bool CanSelectPermanentCondition1(Permanent permanent1)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent1, card))
                                {
                                    if (permanent1.IsTamer)
                                    {
                                        if (permanent1.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
                            }

                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                            {
                                Permanent selectedTamer = null;

                                maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition1));

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentCondition1,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine1,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer.", "The opponent is selecting 1 Tamer.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine1(Permanent permanent)
                                {
                                    selectedTamer = permanent;

                                    yield return null;
                                }

                                if (selectedTamer != null)
                                {
                                    if (selectedTamer.DigivolutionCards.Count((cardSource) => CanSelectCardCondition(cardSource)) >= 1)
                                    {
                                        maxCount = Math.Min(1, selectedTamer.DigivolutionCards.Count((cardSource) => CanSelectCardCondition(cardSource)));

                                        List<CardSource> selectedCards = new List<CardSource>();

                                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                        selectCardEffect.SetUp(
                                                    canTargetCondition: CanSelectCardCondition,
                                                    canTargetCondition_ByPreSelecetedList: null,
                                                    canEndSelectCondition: null,
                                                    canNoSelect: () => false,
                                                    selectCardCoroutine: SelectCardCoroutine,
                                                    afterSelectCardCoroutine: null,
                                                    message: "Select 1 Digimon card with <Save> in its text to digivolve from digivolution cards.",
                                                    maxCount: maxCount,
                                                    canEndNotMax: false,
                                                    isShowOpponent: true,
                                                    mode: SelectCardEffect.Mode.Custom,
                                                    root: SelectCardEffect.Root.Custom,
                                                    customRootCardList: selectedTamer.DigivolutionCards,
                                                    canLookReverseCard: true,
                                                    selectPlayer: card.Owner,
                                                    cardEffect: activateClass);

                                        selectCardEffect.SetUpCustomMessage("Select 1 Digimon card with <Save> in its text to digivolve from digivolution cards.", "The opponent is selecting 1 Digimon card with <Save> in its text to digivolve from digivolution cards.");
                                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                                        yield return StartCoroutine(selectCardEffect.Activate());

                                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                                        {
                                            selectedCards.Add(cardSource);

                                            yield return null;
                                        }

                                        yield return ContinuousController.instance.StartCoroutine(new PlayCardClass(
                                                        cardSources: selectedCards,
                                                        hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                                        payCost: true,
                                                        targetPermanent: selectedPermanent,
                                                        isTapped: false,
                                                        root: SelectCardEffect.Root.DigivolutionCards,
                                                        activateETB: true).PlayCard());
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Add this card to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] Add this card to its owner's hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
                }
            }

            return cardEffects;
        }
    }
}