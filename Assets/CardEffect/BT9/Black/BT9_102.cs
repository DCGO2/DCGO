using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT9_102 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OptionSkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] You may trash 1 card with [Cyborg] or [Machine] in its traits in your hand to have all of your level 6 Digimon with [Machine] in their traits gain <Rush> (This Digimon can attack the turn it comes into play) and [On Play] If this Digimon has a digivolution card, <Blitz>. (This Digimon can attack when your opponent has 1 or more memory.)for the turn.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.CardTraits.Contains("Cyborg"))
                {
                    return true;
                }

                if (cardSource.CardTraits.Contains("Machine"))
                {
                    return true;
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                {
                    bool discarded = false;

                    int discardCount = 1;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: discardCount,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count >= 1)
                        {
                            discarded = true;

                            yield return null;
                        }
                    }

                    if (discarded)
                    {
                        bool PermanentCondition(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                            {
                                if (permanent.TopCard.CardTraits.Contains("Machine"))
                                {
                                    if (permanent.TopCard.HasLevel)
                                    {
                                        if (permanent.Level == 6)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRushPlayerEffect(
                                                permanentCondition: PermanentCondition,
                                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                                activateClass: activateClass));

                        AddSkillClass addSkillClass = new AddSkillClass();
                        addSkillClass.SetUpICardEffect("Your Digimons get Blitz", CanUseCondition1, card);
                        addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                        CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilEachTurnEnd, card: card, cardEffect: addSkillClass, timing: EffectTiming.None);

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            return true;
                        }

                        bool CardSourceCondition(CardSource cardSource)
                        {
                            if (CardEffectCommons.IsExistOnBattleArea(cardSource))
                            {
                                if (cardSource == cardSource.PermanentOfThisCard().TopCard)
                                {
                                    if (PermanentCondition(cardSource.PermanentOfThisCard()))
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

                        List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                        {
                            if (_timing == EffectTiming.OnEnterFieldAnyone)
                            {
                                bool Condition()
                                {
                                    if (CardSourceCondition(cardSource))
                                    {
                                        if (cardSource.PermanentOfThisCard().DigivolutionCards.Count >= 1)
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                cardEffects.Add(CardEffectFactory.BlitzSelfEffect(isInheritedEffect: false,
                                    card: cardSource,
                                    condition: Condition,
                                    isWhenDigivolving: false));
                            }

                            return cardEffects;
                        }
                    }
                }
            }
        }


        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect($"Trash 1 card from hand to delete 1 Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] You may trash 1 Digimon card with [Cyborg] or [Machine] in its traits in your hand to delete 1 of your opponent's Digimon whose play cost is less than or equal to the trashed card's play cost.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.CardTraits.Contains("Cyborg"))
                {
                    return true;
                }

                if (cardSource.CardTraits.Contains("Machine"))
                {
                    return true;
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    int discardCount = 1;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: discardCount,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count >= 1)
                        {
                            foreach (CardSource cardSource in cardSources)
                            {
                                selectedCards.Add(cardSource);
                            }

                            yield return null;
                        }
                    }

                    if (selectedCards.Count >= 1)
                    {
                        foreach (CardSource selectedCard in selectedCards)
                        {
                            bool CanSelectPermanentCondition(Permanent permanent)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                                {
                                    if (permanent.TopCard.GetCostItself <= selectedCard.GetCostItself)
                                    {
                                        if (permanent.TopCard.HasPlayCost)
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
                            }

                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
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
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Destroy,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                            }
                        }
                    }
                }
            }
        }

        return cardEffects;
    }
}
