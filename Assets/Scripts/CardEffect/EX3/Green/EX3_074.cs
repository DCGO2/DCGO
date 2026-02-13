using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX3
{
    public class EX3_074 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

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
                        bool PermanentCondition1(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.IsDigimon)
                                        {
                                            if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                            {
                                                if (permanent.TopCard.CardColors.Contains(CardColor.Green))
                                                {
                                                    if (permanent.Levels_ForJogress(card).Contains(6))
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

                        bool PermanentCondition2(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.IsDigimon)
                                        {
                                            if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                            {
                                                if (permanent.TopCard.CardColors.Contains(CardColor.Blue))
                                                {
                                                    if (permanent.Levels_ForJogress(card).Contains(6))
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

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "a level 6 green Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 6 blue Digimon"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place a card to digivolution cards and play 1 Digimon from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may place 1 green or blue Digimon card with [Dramon] in its name from your hand under this Digimon as its bottom digivolution card. When DNA digivolving, you may play 1 green or blue Digimon card with [Dramon] in its name and 12000 DP or less from your hand without paying the cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.HasDramonName)
                            {
                                if (cardSource.IsDigimon)
                                {
                                    if (cardSource.CardColors.Contains(CardColor.Green) || cardSource.CardColors.Contains(CardColor.Blue))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.HasDramonName)
                            {
                                if (cardSource.IsDigimon)
                                {
                                    if (cardSource.CardColors.Contains(CardColor.Green) || cardSource.CardColors.Contains(CardColor.Blue))
                                    {
                                        if (cardSource.CardDP <= 12000)
                                        {
                                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
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

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                            {
                                List<CardSource> selectedCards = new List<CardSource>();

                                int maxCount = 1;

                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectHandEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                                yield return StartCoroutine(selectHandEffect.Activate());

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCards.Add(cardSource);

                                    yield return null;
                                }

                                if (selectedCards.Count >= 1)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(selectedCards, activateClass));
                                }
                            }

                            bool isJogress = false;

                            if (_hashtable != null)
                            {
                                if (_hashtable.ContainsKey("isJogress"))
                                {
                                    if (_hashtable["isJogress"] is bool)
                                    {
                                        isJogress = (bool)_hashtable["isJogress"];
                                    }
                                }
                            }

                            if (isJogress)
                            {
                                if (card.Owner.HandCards.Count(CanSelectCardCondition1) >= 1)
                                {
                                    List<CardSource> selectedCards = new List<CardSource>();

                                    int maxCount = 1;

                                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                    selectHandEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectCardCondition1,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: true,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        mode: SelectHandEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                                    selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                                    yield return StartCoroutine(selectHandEffect.Activate());

                                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                                    {
                                        selectedCards.Add(cardSource);

                                        yield return null;
                                    }

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Hand, activateETB: true));
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon and suspend 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("Unsuspen_EX3_074");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns][Once Per Turn] When this Digimon becomes suspended, unsuspend it, and suspend 1 of your opponent's Digimon.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenSelfPermanentSuspends(hashtable, card))
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
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        Permanent selectedPermanent = card.PermanentOfThisCard();

                        yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());
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
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            return cardEffects;
        }
    }
}