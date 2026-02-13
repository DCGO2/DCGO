using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT18
{
    public class BT18_042 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Koji Minamoto") &&
                           targetPermanent.DigivolutionCards.Count(cardSource => cardSource.ContainsTraits("Hybrid")) >= 5;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 5, ignoreDigivolutionRequirement: true,
                    card: card, condition: null));
            }

            #endregion

            #region When Digivolving/ End of Opponent's Turn Shared

            bool CanSelectSourceCardConditionShared(CardSource cardSource)
            {
                return cardSource.IsDigimon;
            }

            bool CanActivateConditionShared(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                       card.PermanentOfThisCard().DigivolutionCards.Some(CanSelectSourceCardConditionShared);
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Place Digivolution card as your bottom security card to delete all opponent's same level Digimon",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("WDUnsuspend_BT18_042");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] (Once Per Turn) By placing 1 Digimon card from this Digimon's digivolution cards as your bottom security card, delete all of your opponent's Digimon with the same level as the placed card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int chosenDigimonCardLevel = 0;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectSourceCardConditionShared,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 digivolution card to place as bottom security card.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Custom,
                        customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage(
                        "Select 1 digivolution card to place as bottom security card.",
                        "The opponent is selecting 1 digivolution card to place as bottom security card.");

                    yield return StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        if (cardSource.IsACE) yield return ContinuousController.instance.StartCoroutine(new AceOverflowClass(new List<CardSource> { cardSource }).Overflow());
                        yield return ContinuousController.instance.StartCoroutine(
                            CardObjectController.AddSecurityCard(cardSource, toTop: false));

                        chosenDigimonCardLevel = cardSource.Level;
                    }

                    if (chosenDigimonCardLevel > 0)
                    {
                        List<Permanent> destroyTargetPermanents =
                            card.Owner.Enemy.GetBattleAreaDigimons().Filter(permanent => permanent.Level == chosenDigimonCardLevel);

                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                            destroyTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                    }
                }
            }

            #endregion

            #region End of Opponent's Turn

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Place Digivolution card as your bottom security card to delete all opponent's same level Digimon",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("EOTUnsuspend_BT18_042");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[End of Opponent's Turn] (Once Per Turn) By placing 1 Digimon card from this Digimon's digivolution cards as your bottom security card, delete all of your opponent's Digimon with the same level as the placed card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOpponentTurn(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int chosenDigimonCardLevel = 0;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectSourceCardConditionShared,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 digivolution card to place as bottom security card.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Custom,
                        customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage(
                        "Select 1 digivolution card to place as bottom security card.",
                        "The opponent is selecting 1 digivolution card to place as bottom security card.");

                    yield return StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        if (cardSource.IsACE) yield return ContinuousController.instance.StartCoroutine(new AceOverflowClass(new List<CardSource> { cardSource }).Overflow());
                        yield return ContinuousController.instance.StartCoroutine(
                            CardObjectController.AddSecurityCard(cardSource, toTop: false));

                        chosenDigimonCardLevel = cardSource.Level;
                    }

                    if (chosenDigimonCardLevel > 0)
                    {
                        List<Permanent> destroyTargetPermanents =
                            card.Owner.Enemy.GetBattleAreaDigimons().Filter(permanent => permanent.Level == chosenDigimonCardLevel);

                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                            destroyTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                    }
                }
            }

            #endregion

            #region All Turns

            if (timing is EffectTiming.OnAllyAttack or EffectTiming.OnCounterTiming)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By adding the top card of your security stack to the hand, unsuspend this Digimon",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("ATUnsuspend_BT18_042");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns] (Once Per Turn) When a Digimon attacks, by adding the top card of your security stack to the hand, unsuspend this Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, PermanentCondition);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.Owner.SecurityCards.Count >= 1;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource topCard = card.Owner.SecurityCards[0];

                    yield return ContinuousController.instance.StartCoroutine(
                        CardObjectController.AddHandCards(new List<CardSource>() { topCard }, false,
                            activateClass));

                    yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                        player: card.Owner,
                        refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());

                    yield return ContinuousController.instance.StartCoroutine(
                        new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}