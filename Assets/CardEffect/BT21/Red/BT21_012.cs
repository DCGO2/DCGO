using System.Collections;
using System.Collections.Generic;

//Flamemon
namespace DCGO.CardEffects.BT21
{
    public class BT21_012 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend this digimon, play 1 red tamer with inherit effects", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[Main] By suspending this Digimon, you may play 1 red Tamer card with inherited effects from your hand without paying the cost. If you did, place this Digimon under the played Tamer.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanActivateSuspendCostEffect(card))
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
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass))
                    {
                        if (cardSource.IsTamer)
                        {
                            if (cardSource.CardColors.Contains(CardColor.Red))
                            {
                                if (cardSource.HasInheritedEffect)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedCard = null;
                    bool tamerPlayed = false;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
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
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (selectedCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: new List<CardSource>() { selectedCard }, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Hand, activateETB: true));
                        tamerPlayed = true;
                    }

                    if (tamerPlayed)
                    {
                        yield return ContinuousController.instance.StartCoroutine(selectedCard.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { card }, activateClass));
                    }
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 2000, isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}