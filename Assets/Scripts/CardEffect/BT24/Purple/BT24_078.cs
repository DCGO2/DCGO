using System.Collections;
using System.Collections.Generic;

// Creepymon (X Antibody)
namespace DCGO.CardEffects.BT24
{
    public class BT24_078 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Condition

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Creepymon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Trash Your Turn
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into this to trash top opponent security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() => "[Trash] [Your Turn] When one of your [Creepymon] attacks, if your opponent has 10 or more cards in their trash, by digivolving it into this card without paying the cost, trash your opponent's top security card.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card) 
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, PermanentCondition);
                }

                bool PermanentCondition(Permanent permanent) => permanent.TopCard.EqualsCardName("Creepymon");

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card)
                        && card.Owner.Enemy.TrashCards.Count >= 10
                        && card.CanPlayCardTargetFrame(GManager.instance.attackProcess.AttackingPermanent.PermanentFrame, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = GManager.instance.attackProcess.AttackingPermanent;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: selectedPermanent,
                                cardCondition: null,
                                payCost: false,
                                reduceCostTuple: null,
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: false,
                                activateClass: activateClass,
                                successProcess: null,
                                ignoreSelection: true));
                    
                    if (selectedPermanent.TopCard == card)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner.Enemy,
                            destroySecurityCount: 1,
                            cardEffect: activateClass,
                            fromTop: true).DestroySecurity());
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete opponent's lowest level, play a Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() => "[When Digivolving] Delete all of your opponent's lowest level Digimon. Then, you may play up to 4 play cost's total worth of [Evil] or [Fallen Angel] trait cards from your trash without paying the cost. For every 10 cards in your opponent's trash, add 4 to this effect's play cost maximum.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) 
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanSelectPermamentCondition(Permanent permanent) => CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy);

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return (cardSource.EqualsTraits("Evil")
                            || cardSource.EqualsTraits("Fallen Angel"))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> destroyTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(CanSelectPermamentCondition);
                    yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(destroyTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());

                    List<CardSource> selectedCards = new List<CardSource>();
                    int totalCost = 4 * (1 + (card.Owner.Enemy.TrashCards.Count / 10));
                    int maxCount = CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CanSelectCardCondition);

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                        canEndSelectCondition: CanEndSelectCardCondition,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: $"Select up to {totalCost} play cost worth of digimon to play",
                        maxCount: maxCount, 
                        canEndNotMax: true,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select digimon to play", "The opponent is selecting digimon to play");

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

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

                        if (sumCost > totalCost)
                        {
                            return false;
                        }

                        return true;
                    }

                    bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                    {
                        int sumCost = 0;

                        foreach (CardSource cardSource1 in cardSources)
                        {
                            sumCost += cardSource1.GetCostItself;
                        }

                        sumCost += cardSource.GetCostItself;

                        if (sumCost > totalCost)
                        {
                            return false;
                        }

                        return true;
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    if (selectedCards != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: selectedCards,
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Trash,
                        activateETB: true));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
