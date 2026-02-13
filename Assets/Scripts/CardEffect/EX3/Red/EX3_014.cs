using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX3
{
    public class EX3_014 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RushSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                int maxDP()
                {
                    int maxDP = 3000;

                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        maxDP += 2000 * card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.HasDragonTraits);
                    }

                    return maxDP;

                }

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Delete 1 of your opponent's Digimon with 3000 or less. For each card with [Dragon], [saur] or [Ceratopsian] in one of its traits in this Digimon's digivolution cards, add 2000 DP to the maximum DP you can choose with this effect.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(maxDP(), activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
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
                        DigiXrosConditionElement element = new DigiXrosConditionElement(CanSelectCardCondition, "1 Digimon card with [Dragon], [saur] or [Ceratopsian] in one of its traits");

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                if (cardSource.Owner == card.Owner)
                                {
                                    if (cardSource.IsDigimon)
                                    {
                                        if (cardSource.HasDragonTraits)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>();

                        for (int i = 0; i < 5; i++)
                        {
                            elements.Add(element);
                        }

                        bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                        {
                            List<string> cardNames = new List<string>();

                            foreach (CardSource cardSource1 in cardSources)
                            {
                                foreach (string cardName in cardSource1.CardNames)
                                {
                                    if (!cardNames.Contains(cardName))
                                    {
                                        cardNames.Add(cardName);
                                    }
                                }
                            }

                            if (cardSource.CardNames.Count((cardName) => cardNames.Contains(cardName)) >= 1)
                            {
                                return false;
                            }

                            return true;
                        }

                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, CanTargetCondition_ByPreSelecetedList, 2);

                        return digiXrosCondition;
                    }

                    return null;
                }
            }

            return cardEffects;
        }
    }
}