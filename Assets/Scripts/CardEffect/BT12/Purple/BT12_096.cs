using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Tagiru Akashi
namespace DCGO.CardEffects.BT12
{
    public class BT12_096 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Turn
            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolution Cost -1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetHashString("DigivolutionCost-1_BT12_094");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] If one of your Digimon digivolves into a Digimon card with <Save> in its text, by suspending this Tamer and placing 1 card from under one of your Tamers under that Digimon as one of its digivolution cards, reduce the digivolution cost by 1.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(hashtable, PermanentCondition, CardCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.DigivolutionCards.Any();
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasSaveText;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    Permanent giveDigivolutionCardsTamer = null;
                    List<CardSource> selectedDigivolutionCards = new List<CardSource>();
                    Permanent getDigivolutionCardsDigimon = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer that has digivolution cards.", "The opponent is selecting 1 Tamer that has digivolution cards.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        giveDigivolutionCardsTamer = permanent;

                        yield return null;
                    }

                    if (giveDigivolutionCardsTamer != null)
                    {
                        bool isFaceDown = false;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: (cardSource) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 digivolution card.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: giveDigivolutionCardsTamer.DigivolutionCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUseFaceDown();

                        selectCardEffect.SetUpCustomMessage("Select 1 digivolution card.", "The opponent is selecting 1 digivolution card.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedDigivolutionCards.Add(cardSource);

                            if(selectedDigivolutionCards[0].IsFaceDown)
                            {
                                isFaceDown = true;
                            }

                            yield return null;
                        }

                        bool CanSelectPermanentCondition1(Permanent permanent)
                        {
                            List<Permanent> Permanents = CardEffectCommons.GetPermanentsFromHashtable(_hashtable);

                            return Permanents != null
                                && Permanents.Contains(permanent)
                                && CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                                && !permanent.IsToken;
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                        {
                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition1,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine1,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get digivolution cards.", "The opponent is selecting 1 Digimon that will get digivolution cards.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine1(Permanent permanent)
                            {
                                getDigivolutionCardsDigimon = permanent;

                                yield return null;
                            }
                        }

                        if (getDigivolutionCardsDigimon != null
                        && selectedDigivolutionCards.Count >= 1)
                        {
                            foreach (CardSource selectedCard in selectedDigivolutionCards)
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().RemoveDigivolveRootEffect(selectedCard, giveDigivolutionCardsTamer));
                            }

                            yield return ContinuousController.instance.StartCoroutine(getDigivolutionCardsDigimon.AddDigivolutionCardsBottom(selectedDigivolutionCards, activateClass, false, isFaceDown));

                            ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                            ChangeCostClass changeCostClass = new ChangeCostClass();
                            changeCostClass.SetUpICardEffect("Digivolution Cost -1", CanUseCondition1, card);
                            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                            card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                return true;
                            }

                            int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                            {
                                if (CardSourceCondition(cardSource)
                                && RootCondition(root)
                                && PermanentsCondition(targetPermanents))
                                {
                                    Cost -= 1;
                                }

                                return Cost;
                            }

                            bool PermanentsCondition(List<Permanent> targetPermanents)
                            {
                                return targetPermanents != null
                                    && targetPermanents.Count(PermanentCondition) >= 1;
                            }

                            bool PermanentCondition(Permanent targetPermanent)
                            {
                                return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card);
                            }

                            bool CardSourceCondition(CardSource cardSource)
                            {
                                return cardSource.HasSaveText;
                            }

                            bool RootCondition(SelectCardEffect.Root root)
                            {
                                return true;
                            }

                            bool isUpDown()
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}