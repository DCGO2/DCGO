using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX8
{
    public class EX8_029 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region DNA Digivolve
            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect($"DNA Digivolution", CanUseCondition, card);
                addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
                addJogressConditionClass.SetNotShowUI(true);
                cardEffects.Add(addJogressConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                JogressCondition GetJogress(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        bool PermanentCondition1(Permanent permanent) => permanent.TopCard.EqualsCardName("Plesiomon");

                        bool PermanentCondition2(Permanent permanent) => permanent.TopCard.ContainsCardName("Seadramon") &&
                                                                         permanent.Levels_ForJogress(card).Any(value => value == 5);

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "Plesiomon"),

                        new JogressConditionElement(PermanentCondition2, "a level 5 w/[Seadramon] in name"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region DNA Digivolve
            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect($"DNA Digivolution", CanUseCondition, card);
                addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
                addJogressConditionClass.SetNotShowUI(true);
                cardEffects.Add(addJogressConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                JogressCondition GetJogress(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        bool PermanentCondition1(Permanent permanent) => (permanent.TopCard.CardColors.Contains(CardColor.Blue) || permanent.TopCard.CardColors.Contains(CardColor.Purple)) &&
                                                                         permanent.Levels_ForJogress(card).Contains(6);

                        bool PermanentCondition2(Permanent permanent) => (permanent.TopCard.CardColors.Contains(CardColor.Black) || permanent.TopCard.CardColors.Contains(CardColor.Yellow)) &&
                                                                         permanent.Levels_ForJogress(card).Contains(6);

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "a level 6 blue/purple Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 6 black/yellow Digimon"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                int maxCost = 14;
                int maxPlay = 12;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 14 play cost of Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Return 14 play cost's total worth of your opponent's Digimon to the bottom of the deck. If DNA digivolving, you may play 12 play cost's total worth of [DS] trait Digimon from this Digimon's digivolution cards without paying the cost.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.GetCostItself <= maxCost)
                        {
                            if (permanent.TopCard.HasPlayCost)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource source)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(source, false, activateClass, SelectCardEffect.Root.DigivolutionCards))
                    {
                        if (source.EqualsTraits("DS"))
                        {
                            if (source.GetCostItself <= maxCost)
                            {
                                if (source.HasPlayCost)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card) &&
                           CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition);

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                            canEndSelectCondition: CanEndSelectCondition,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: true,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        bool CanEndSelectCondition(List<Permanent> permanents)
                        {
                            if (permanents.Count <= 0)
                            {
                                return false;
                            }

                            int sumCost = 0;

                            foreach (Permanent permanent1 in permanents)
                            {
                                sumCost += permanent1.TopCard.GetCostItself;
                            }

                            if (sumCost > maxCost)
                            {
                                return false;
                            }

                            return true;
                        }

                        bool CanTargetCondition_ByPreSelecetedList(List<Permanent> permanents, Permanent permanent)
                        {
                            int sumCost = 0;

                            foreach (Permanent permanent1 in permanents)
                            {
                                sumCost += permanent1.TopCard.GetCostItself;
                            }

                            sumCost += permanent.TopCard.GetCostItself;

                            if (sumCost > maxCost)
                            {
                                return false;
                            }

                            return true;
                        }
                    }

                    if (CardEffectCommons.IsJogress(_hashtable))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) > 0)
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectCardCondition,
                                        canTargetCondition_ByPreSelecetedList: CanTargetCardCondition_ByPreSelecetedList,
                                        canEndSelectCondition: CanEndSelectCardCondition,
                                        canNoSelect: () => true,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select up to 12 play cost to play.",
                                        maxCount: card.PermanentOfThisCard().DigivolutionCards.Count,
                                        canEndNotMax: true,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Custom,
                                        customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select up to 12 play cost to play.", "The opponent is selecting up to 12 play cost to play.");

                            yield return StartCoroutine(selectCardEffect.Activate());

                            bool CanEndSelectCardCondition(List<CardSource> cards)
                            {
                                if (cards.Count <= 0)
                                {
                                    return false;
                                }

                                int sumCost = 0;

                                foreach (CardSource source in cards)
                                {
                                    sumCost += source.GetCostItself;
                                }

                                if (sumCost > maxPlay)
                                {
                                    return false;
                                }

                                return true;
                            }

                            bool CanTargetCardCondition_ByPreSelecetedList(List<CardSource> cards, CardSource source)
                            {
                                int sumCost = 0;

                                foreach (CardSource source1 in cards)
                                {
                                    sumCost += source1.GetCostItself;
                                }

                                sumCost += source.GetCostItself;

                                if (sumCost > maxPlay)
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

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: selectedCards,
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.DigivolutionCards,
                                activateETB: true));
                        }
                    }
                }
            }
            #endregion

            #region All Turns - Immune
            if (timing == EffectTiming.None)
            {
                CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's Digimon's effects", CanUseCondition, card);
                canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                cardEffects.Add(canNotAffectedClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return card.Owner.MemoryForPlayer >= 1;
                    }

                    return false;
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(cardSource.PermanentOfThisCard(), card))
                    {
                        return cardSource.EqualsTraits("DS");
                    }

                    return false;
                }

                bool SkillCondition(ICardEffect cardEffect)
                {
                    if (CardEffectCommons.IsOpponentEffect(cardEffect, card))
                    {
                        if (cardEffect.IsDigimonEffect)
                        {
                            return true;
                        }

                        if (cardEffect.IsDigimonEffect && cardEffect.IsSecurityEffect)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
            #endregion

            #region All Turns - Ignore On Play
            if (timing == EffectTiming.None)
            {
                DisableEffectClass invalidationClass = new DisableEffectClass();
                invalidationClass.SetUpICardEffect("[On Plays] can't activate", CanUseCondition, card);
                invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                cardEffects.Add(invalidationClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return card.Owner.MemoryForPlayer <= 1;
                    }

                    return false;
                }

                bool InvalidateCondition(ICardEffect cardEffect)
                {
                    if (cardEffect != null)
                    {
                        if (cardEffect is ActivateICardEffect)
                        {
                            if (cardEffect.EffectSourceCard != null)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(cardEffect.EffectSourceCard.PermanentOfThisCard(), card))
                                {
                                    if (!cardEffect.EffectSourceCard.PermanentOfThisCard().TopCard.CanNotBeAffected(invalidationClass))
                                    {
                                        if (cardEffect.IsOnPlay)
                                        {

                                            return card.Owner.MemoryForPlayer <= 1;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}