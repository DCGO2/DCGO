using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Analytics;

// DarknessBagramon
namespace DCGO.CardEffects.EX10
{
    public class EX10_059 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region DigiXros

            if (timing == EffectTiming.None)
            {
                AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
                addDigiXrosConditionClass.SetUpICardEffect($"DigiXros -3", _ => true, card);
                addDigiXrosConditionClass.SetUpAddDigiXrosConditionClass(getDigiXrosCondition: GetDigiXros);
                addDigiXrosConditionClass.SetNotShowUI(true);
                cardEffects.Add(addDigiXrosConditionClass);

                DigiXrosCondition GetDigiXros(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        DigiXrosConditionElement elementAgunimon =
                            new DigiXrosConditionElement(CanSelectCardConditionBagramon, "Bagramon");

                        bool CanSelectCardConditionBagramon(CardSource conditionCardSource)
                        {
                            if (conditionCardSource != null)
                            {
                                if (conditionCardSource.Owner == card.Owner)
                                {
                                    if (conditionCardSource.IsDigimon)
                                    {
                                        if (conditionCardSource.CardNames_DigiXros.Contains("Bagramon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        DigiXrosConditionElement elementBurningGreymon =
                            new DigiXrosConditionElement(CanSelectCardConditionDarkKnightmon, "DarkKnightmon");

                        bool CanSelectCardConditionDarkKnightmon(CardSource conditionCardSource)
                        {
                            if (conditionCardSource != null)
                            {
                                if (conditionCardSource.Owner == card.Owner)
                                {
                                    if (conditionCardSource.IsDigimon)
                                    {
                                        if (conditionCardSource.CardNames_DigiXros.Contains("DarkKnightmon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>()
                            { elementAgunimon, elementBurningGreymon };

                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, null, 3);

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
                activateClass.SetUpICardEffect("Place 1 opponent hand card under a digimon or tamer, then by placing 3 [Bagra Army] digimon from trash under this, delete 1 digimon/tamer with sources", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Choose 1 card in your opponent's hand without looking and place it as any of their Digimon's bottom digivolution card or under any of their Tamers. Then, by placing 3 [Bagra Army] trait Digimon cards from your trash as this Digimon's top digivolution cards, delete 1 of their Digimon or Tamers with cards under it.";
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

                bool IsOpponentPermament(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card) &&
                           !permanent.TopCard.IsOption;
                }

                bool IsOpponentPermamentWithSources(Permanent permanent)
                {
                    return IsOpponentPermament(permanent) && permanent.StackCards.Count >= 2;
                }

                bool IsBagraArmyDigimon(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.HasBagraArmyTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.Enemy.HandCards.Count >= 1 && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsOpponentPermament))
                    {
                        CardSource selectedHandCard = null;

                        #region Select 1 card in opponent's hand

                        if (card.Owner.isYou)
                        {
                            foreach (CardSource cardSource in card.Owner.Enemy.HandCards)
                            {
                                cardSource.SetReverse();
                            }
                        }

                        card.Owner.Enemy.HandCards = RandomUtility.ShuffledDeckCards(card.Owner.Enemy.HandCards);
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: (cardSource) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card in your opponent's hand to add as a source card.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: false,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: card.Owner.Enemy.HandCards,
                            canLookReverseCard: false,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetNotShowCard();
                        selectCardEffect.SetUseFaceDown();

                        if (card.Owner.isYou)
                        {
                            selectCardEffect.SetNotAddLog();
                        }

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedHandCard = cardSource;
                            yield return null;
                        }

                        selectCardEffect.SetUpCustomMessage("Select 1 card to add as a source card.", "The opponent is selecting 1 to add as a source card.");
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        #endregion

                        if (selectedHandCard != null)
                        {
                            Permanent selectedPermanent = null;

                            #region Select Enemy Permament to add source

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, IsOpponentPermament));

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsOpponentPermament,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
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

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or tamer to add the source card underneath", "The opponent is selecting 1 Digimon or tamer to add the source card underneath");
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            #endregion

                            if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(
                                addedDigivolutionCards: new List<CardSource>() { selectedHandCard },
                                cardEffect: activateClass));
                        }
                    }

                    if (CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, IsBagraArmyDigimon) >= 3)
                    {
                        Permanent thisPermament = card.PermanentOfThisCard();
                        List<CardSource> selectedCards = new List<CardSource>();

                        #region Select 3 Bagra Army Digimon in Trash

                        int maxCount = Math.Min(3, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, IsBagraArmyDigimon));
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: IsBagraArmyDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 3 [Bagra Army] digimon",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        selectCardEffect.SetUpCustomMessage("Select 3 [Bagra Army] digimon to add as source", "The opponent is selecting 3 [Bagra Army] digimon to add as source");
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        #endregion

                        if (selectedCards.Count == 3)
                        {
                            yield return ContinuousController.instance.StartCoroutine(thisPermament.AddDigivolutionCardsTop(
                                addedDigivolutionCards: selectedCards,
                                cardEffect: activateClass));

                            if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsOpponentPermamentWithSources))
                            {
                                #region Select Enemy Permament to delete

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                                int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, IsOpponentPermamentWithSources));

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: IsOpponentPermamentWithSources,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount1,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Destroy,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or tamer with sources to destroy", "The opponent is selecting 1 Digimon or tamer with sources to destroy");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                #endregion
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
                activateClass.SetUpICardEffect("Place 1 opponent hand card under a digimon or tamer, then by placing 3 [Bagra Army] digimon from trash under this, delete 1 digimon/tamer with sources", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Choose 1 card in your opponent's hand without looking and place it as any of their Digimon's bottom digivolution card or under any of their Tamers. Then, by placing 3 [Bagra Army] trait Digimon cards from your trash as this Digimon's top digivolution cards, delete 1 of their Digimon or Tamers with cards under it.";
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

                bool IsOpponentPermament(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card) &&
                           !permanent.TopCard.IsOption;
                }

                bool IsOpponentPermamentWithSources(Permanent permanent)
                {
                    return IsOpponentPermament(permanent) && permanent.StackCards.Count >= 2;
                }

                bool IsBagraArmyDigimon(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.HasBagraArmyTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.Enemy.HandCards.Count >= 1 && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsOpponentPermament))
                    {
                        CardSource selectedHandCard = null;

                        #region Select 1 card in opponent's hand

                        if (card.Owner.isYou)
                        {
                            foreach (CardSource cardSource in card.Owner.Enemy.HandCards)
                            {
                                cardSource.SetReverse();
                            }
                        }

                        card.Owner.Enemy.HandCards = RandomUtility.ShuffledDeckCards(card.Owner.Enemy.HandCards);
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: (cardSource) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card in your opponent's hand to add as a source card.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: false,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: card.Owner.Enemy.HandCards,
                            canLookReverseCard: false,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetNotShowCard();
                        selectCardEffect.SetUseFaceDown();

                        if (card.Owner.isYou)
                        {
                            selectCardEffect.SetNotAddLog();
                        }

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedHandCard = cardSource;
                            yield return null;
                        }

                        selectCardEffect.SetUpCustomMessage("Select 1 card to add as a source card.", "The opponent is selecting 1 to add as a source card.");
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        #endregion

                        if (selectedHandCard != null)
                        {
                            Permanent selectedPermanent = null;

                            #region Select Enemy Permament to add source

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, IsOpponentPermament));

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsOpponentPermament,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
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

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or tamer to add the source card underneath", "The opponent is selecting 1 Digimon or tamer to add the source card underneath");
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            #endregion

                            if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(
                                addedDigivolutionCards: new List<CardSource>() { selectedHandCard },
                                cardEffect: activateClass));
                        }
                    }

                    if (CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, IsBagraArmyDigimon) >= 3)
                    {
                        Permanent thisPermament = card.PermanentOfThisCard();
                        List<CardSource> selectedCards = new List<CardSource>();

                        #region Select 3 Bagra Army Digimon in Trash

                        int maxCount = Math.Min(3, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, IsBagraArmyDigimon));
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: IsBagraArmyDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 3 [Bagra Army] digimon",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        selectCardEffect.SetUpCustomMessage("Select 3 [Bagra Army] digimon to add as source", "The opponent is selecting 3 [Bagra Army] digimon to add as source");
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        #endregion

                        if (selectedCards.Count == 3)
                        {
                            yield return ContinuousController.instance.StartCoroutine(thisPermament.AddDigivolutionCardsTop(
                                addedDigivolutionCards: selectedCards,
                                cardEffect: activateClass));

                            if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsOpponentPermamentWithSources))
                            {
                                #region Select Enemy Permament to delete

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                                int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, IsOpponentPermamentWithSources));

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: IsOpponentPermamentWithSources,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount1,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Destroy,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or tamer with sources to destroy", "The opponent is selecting 1 Digimon or tamer with sources to destroy");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                #endregion
                            }
                        }
                    }
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.None)
            {
                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("This Digimon gains all [All Turns] effects on all level 6 [Bagra Army] trait Digimon cards in its digivolution cards.", CanUseCondition, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);

                cardEffects.Add(addSkillClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    if (PermanentCondition(cardSource.PermanentOfThisCard()))
                    {
                        if (cardSource == cardSource.PermanentOfThisCard().TopCard)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                {
                    if (CardSourceCondition(cardSource))
                    {
                        foreach (CardSource cardSource1 in cardSource.PermanentOfThisCard().DigivolutionCards)
                        {
                            if (cardSource1.IsFlipped)
                                continue;

                            if (cardSource1.HasBagraArmyTraits)
                            {
                                if (cardSource1.HasLevel && cardSource1.IsLevel6)
                                {
                                    foreach (ICardEffect cardEffect in cardSource1.cEntity_EffectController.GetCardEffects_ExceptAddedEffects(_timing, card))
                                    {
                                        if (!cardEffect.IsSecurityEffect && !cardEffect.IsInheritedEffect)
                                        {
                                            if (cardEffect.EffectDiscription.StartsWith("[All Turns]"))
                                                cardEffects.Add(cardEffect);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return cardEffects;
                }
            }

            #endregion

            return cardEffects;
        }
    }
}