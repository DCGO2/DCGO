using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT16
{
    public class BT16_024 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            var cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("Angemon");
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

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon digivolves into a Digimon card from security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetHashString("MagnaAngemon_BT16_024_OnPlay_WhenDigivolving");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return
                        "[On Play] If it's your turn, search your security stack. This Digimon may digivolve into a Digimon card with the [Angel] " +
                        "or [Three Great Angels] trait among them with the digivolution cost reduced by 2. Then, shuffle your security stack. If this effect digivolved, " +
                        "you may place 1 Digimon card with the [Angel], [Archangel] or [Three Great Angels] trait from your hand at the bottom of your security stack.";
                }

                bool CanSelectCardDigivolutionCondition(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("Angel") || cardSource.CardTraits.Contains("Three Great Angels") || cardSource.CardTraits.Contains("ThreeGreatAngels"))
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (CardEffectCommons.IsExistOnBattleArea(card))
                            {
                                if (cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, false, activateClass))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardRecoverCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.HasAngelTraitRestrictive)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOwnerTurn(card) && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.SecurityCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        int maxCount = Math.Min(1, card.Owner.SecurityCards.Count);

                        CardSource selectedCard = null;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardDigivolutionCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to digivolve.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Security,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;

                            yield return null;
                        }

                        ContinuousController.instance.PlaySE(GManager.instance.ShuffleSE);

                        card.Owner.SecurityCards = RandomUtility.ShuffledDeckCards(card.Owner.SecurityCards);

                        if (selectedCard != null)
                        {
                            if (CardEffectCommons.IsExistOnBattleArea(card))
                            {
                                GManager.instance.turnStateMachine.gameContext.IsSecurityLooking = true;

                                if (selectedCard.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame,
                                        PayCost: false, activateClass, root: SelectCardEffect.Root.Security))
                                {
                                    PlayCardClass playCardClass = new PlayCardClass(
                                        cardSources: new List<CardSource>() { selectedCard },
                                        hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                        payCost: true,
                                        targetPermanent: card.PermanentOfThisCard(),
                                        isTapped: false,
                                        root: SelectCardEffect.Root.Security,
                                        activateETB: true);

                                    playCardClass.SetReducedCost(2);

                                    yield return ContinuousController.instance.StartCoroutine(playCardClass.PlayCard());
                                }

                                GManager.instance.turnStateMachine.gameContext.IsSecurityLooking = false;

                                if (CardEffectCommons.IsDigivolvedByTheEffect(card.PermanentOfThisCard(), selectedCard,
                                        activateClass))
                                {
                                    if (card.Owner.CanAddSecurity(activateClass))
                                    {
                                        if (card.Owner.HandCards.Count(CanSelectCardRecoverCondition) >= 1)
                                        {
                                            List<CardSource> selectedCards = new List<CardSource>();

                                            maxCount = 1;

                                            SelectHandEffect selectHandEffect =
                                                GManager.instance.GetComponent<SelectHandEffect>();

                                            selectHandEffect.SetUp(
                                                selectPlayer: card.Owner,
                                                canTargetCondition: CanSelectCardRecoverCondition,
                                                canTargetCondition_ByPreSelecetedList: null,
                                                canEndSelectCondition: null,
                                                maxCount: maxCount,
                                                canNoSelect: true,
                                                canEndNotMax: false,
                                                isShowOpponent: true,
                                                selectCardCoroutine: SelectCardCoroutine1,
                                                afterSelectCardCoroutine: null,
                                                mode: SelectHandEffect.Mode.Custom,
                                                cardEffect: activateClass);

                                            selectHandEffect.SetUpCustomMessage(
                                                "Select 1 card to place at the bottom of security.",
                                                "The opponent is selecting 1 card to place at the bottom of security.");
                                            selectHandEffect.SetUpCustomMessage_ShowCard("Security Bottom Card");

                                            yield return StartCoroutine(selectHandEffect.Activate());

                                            IEnumerator SelectCardCoroutine1(CardSource cardSource)
                                            {
                                                selectedCards.Add(cardSource);

                                                yield return null;
                                            }

                                            foreach (CardSource cardSource in selectedCards)
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(
                                                    CardObjectController.AddSecurityCard(cardSource, toTop: false));
                                            }
                                        }
                                    }
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
                activateClass.SetUpICardEffect("This Digimon digivolves into a Digimon card from security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetHashString("MagnaAngemon_BT16_024_OnPlay_WhenDigivolving");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return
                        "[When Digivolving] If it's your turn, search your security stack. This Digimon may digivolve into a Digimon card with the [Angel] " +
                        "or [Three Great Angels] trait among them with the digivolution cost reduced by 2. Then, shuffle your security stack. If this effect digivolved, " +
                        "you may place 1 Digimon card with the [Angel], [Archangel] or [Three Great Angels] trait from your hand at the bottom of your security stack.";
                }

                bool CanSelectCardDigivolutionCondition(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("Angel") || cardSource.CardTraits.Contains("Three Great Angels") || cardSource.CardTraits.Contains("ThreeGreatAngels"))
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (CardEffectCommons.IsExistOnBattleArea(card))
                            {
                                if (cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, false, activateClass))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardRecoverCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.HasAngelTraitRestrictive)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOwnerTurn(card) && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.SecurityCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        int maxCount = Math.Min(1, card.Owner.SecurityCards.Count);

                        CardSource selectedCard = null;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardDigivolutionCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to digivolve.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Security,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;

                            yield return null;
                        }

                        ContinuousController.instance.PlaySE(GManager.instance.ShuffleSE);

                        card.Owner.SecurityCards = RandomUtility.ShuffledDeckCards(card.Owner.SecurityCards);

                        if (selectedCard != null)
                        {
                            if (CardEffectCommons.IsExistOnBattleArea(card))
                            {
                                GManager.instance.turnStateMachine.gameContext.IsSecurityLooking = true;

                                if (selectedCard.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame,
                                        PayCost: false, activateClass, root: SelectCardEffect.Root.Security))
                                {
                                    PlayCardClass playCardClass = new PlayCardClass(
                                        cardSources: new List<CardSource>() { selectedCard },
                                        hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                        payCost: true,
                                        targetPermanent: card.PermanentOfThisCard(),
                                        isTapped: false,
                                        root: SelectCardEffect.Root.Security,
                                        activateETB: true);

                                    playCardClass.SetReducedCost(2);

                                    yield return ContinuousController.instance.StartCoroutine(playCardClass.PlayCard());
                                }

                                GManager.instance.turnStateMachine.gameContext.IsSecurityLooking = false;

                                if (CardEffectCommons.IsDigivolvedByTheEffect(card.PermanentOfThisCard(), selectedCard,
                                        activateClass))
                                {
                                    if (card.Owner.CanAddSecurity(activateClass))
                                    {
                                        if (card.Owner.HandCards.Count(CanSelectCardRecoverCondition) >= 1)
                                        {
                                            List<CardSource> selectedCards = new List<CardSource>();

                                            maxCount = 1;

                                            SelectHandEffect selectHandEffect =
                                                GManager.instance.GetComponent<SelectHandEffect>();

                                            selectHandEffect.SetUp(
                                                selectPlayer: card.Owner,
                                                canTargetCondition: CanSelectCardRecoverCondition,
                                                canTargetCondition_ByPreSelecetedList: null,
                                                canEndSelectCondition: null,
                                                maxCount: maxCount,
                                                canNoSelect: true,
                                                canEndNotMax: false,
                                                isShowOpponent: true,
                                                selectCardCoroutine: SelectCardCoroutine1,
                                                afterSelectCardCoroutine: null,
                                                mode: SelectHandEffect.Mode.Custom,
                                                cardEffect: activateClass);

                                            selectHandEffect.SetUpCustomMessage(
                                                "Select 1 card to place at the bottom of security.",
                                                "The opponent is selecting 1 card to place at the bottom of security.");
                                            selectHandEffect.SetUpCustomMessage_ShowCard("Security Bottom Card");

                                            yield return StartCoroutine(selectHandEffect.Activate());

                                            IEnumerator SelectCardCoroutine1(CardSource cardSource)
                                            {
                                                selectedCards.Add(cardSource);

                                                yield return null;
                                            }

                                            foreach (CardSource cardSource in selectedCards)
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(
                                                    CardObjectController.AddSecurityCard(cardSource, toTop: false));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Inherited Effect

            if (timing == EffectTiming.None)
            {
                bool CanUseCondition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        return permanent.TopCard.HasAngelTraitRestrictive;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.BlockerStaticEffect(permanentCondition: PermanentCondition, isInheritedEffect: true, card: card, condition: CanUseCondition));
            }

            #endregion

            return cardEffects;
        }
    }
}