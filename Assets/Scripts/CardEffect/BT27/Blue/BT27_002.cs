using System;
using System.Collections;
using System.Collections.Generic;

// Chapmon (BT27-002) — inherited effect only
namespace DCGO.CardEffects.BT27
{
    public class BT27_002 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Inherited — When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 level 4 or lower Aquatic/DATA SQUAD Digimon from digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT27_002_Inherited");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] [Once Per Turn] You may play 1 level 4 or lower [Aquatic] or [DATA SQUAD] trait Digimon card from this Digimon's digivolution cards with the cost reduced by 1.";

                int ReducedCost(CardSource source)
                    => Math.Max(0, source.GetCostItself - 1);

                bool CanSelectSourceCardCondition(CardSource source)
                    => source != null
                       && source.IsDigimon
                       && source.HasLevel
                       && source.Level <= 4
                       && (source.EqualsTraits("Aquatic") || source.EqualsTraits("DATA SQUAD"))
                       && CardEffectCommons.CanPlayAsNewPermanent(
                           cardSource: source,
                           payCost: true,
                           cardEffect: activateClass,
                           fixedCost: ReducedCost(source));

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                       && CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                       && card.PermanentOfThisCard().DigivolutionCards.Some(CanSelectSourceCardCondition);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedCard = null;
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectSourceCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 digivolution card to play (cost -1).",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.DigivolutionCards,
                        customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage(
                        "Select 1 digivolution card to play (cost -1).",
                        "The opponent is selecting 1 digivolution card to play.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource source)
                    {
                        selectedCard = source;
                        yield return null;
                    }

                    if (selectedCard == null)
                    {
                        activateClass.RemoveUse();
                        yield break;
                    }
                    else
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.PlayPermanentCards(
                                cardSources: new List<CardSource> { selectedCard },
                                activateClass: activateClass,
                                payCost: true,
                                isTapped: false,
                                root: SelectCardEffect.Root.DigivolutionCards,
                                activateETB: true,
                                fixedCost: ReducedCost(selectedCard)
                            ));
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
