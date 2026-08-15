using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Monimon
namespace DCGO.CardEffects.BT26
{
    public class BT26_006 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Inherit
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 2 Bagra Army digivolution cards to play/use 1 Bagra Army card for -2", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT26_006_Inherit");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] [Once Per Turn] By trashing any 2 digivolution cards from your [Bagra Army] trait Digimon, you may play or use 1 [Bagra Army] trait card from your hand with the cost reduced by 2.";

                bool TrashSourcePermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.ContainsTraits("Bagra Army");

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.Owner.GetBattleAreaDigimons().Where(TrashSourcePermanentCondition).Sum(permanent => permanent.DigivolutionCards.Count) >= 2;

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                        permanentCondition: TrashSourcePermanentCondition,
                        cardCondition: null,
                        maxCount: 2,
                        canNoTrash: true,
                        isFromOnly1Permanent: false,
                        activateClass: activateClass,
                        selectString: "digivolution cards",
                        afterSelectionCoroutine: AfterTrashCoroutine));

                    IEnumerator AfterTrashCoroutine(Permanent trashSourcePermanent, List<CardSource> trashedCards)
                    {
                        if (trashedCards.Count == 2)
                        {
                            bool CanSelectCardCondition(CardSource cardSource)
                                => cardSource.ContainsTraits("Bagra Army")
                                    && ((cardSource.IsOption
                                        && !cardSource.CanNotPlayThisOption)
                                    || (cardSource.HasPlayCost
                                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, true, activateClass, fixedCost: cardSource.GetCostItself - 2)));

                            if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                            {
                                CardSource selectedCard = null;

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

                                selectHandEffect.SetUpCustomMessage("Select 1 [Bagra Army] card to play/use.", "The opponent is selecting 1 [Bagra Army] card to play/use.");

                                yield return StartCoroutine(selectHandEffect.Activate());

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCard = cardSource;
                                    yield return null;
                                }

                                if (selectedCard != null)
                                {
                                    ChangeCostClass changeCostClass = new ChangeCostClass();
                                    changeCostClass.SetUpICardEffect("Play/Use Cost -2", _ => true, card);
                                    changeCostClass.SetUpChangeCostClass(
                                        changeCostFunc: ChangeCost, cardSourceCondition: _ => true, rootCondition: _ => true,
                                        isUpDown: () => true, isCheckAvailability: () => false, isChangePayingCost: () => true);

                                    ICardEffect GetCardEffect(EffectTiming _timing) => _timing == EffectTiming.None ? changeCostClass : null;
                                    card.Owner.UntilCalculateFixedCostEffect.Add(GetCardEffect);

                                    int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                                        => cost - 2;

                                    if (selectedCard.IsOption)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                                            cardSources: new List<CardSource> { selectedCard },
                                            activateClass: activateClass,
                                            payCost: true,
                                            root: SelectCardEffect.Root.Hand));
                                    }
                                    else
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                            cardSources: new List<CardSource> { selectedCard },
                                            activateClass: activateClass,
                                            payCost: true,
                                            isTapped: false,
                                            root: SelectCardEffect.Root.Hand,
                                            activateETB: true));
                                    }

                                    card.Owner.UntilCalculateFixedCostEffect.Remove(GetCardEffect);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
