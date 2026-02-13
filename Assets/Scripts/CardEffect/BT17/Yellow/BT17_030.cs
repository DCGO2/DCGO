using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT17
{
    public class BT17_030 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("Bibimon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Start of Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place cards under this Digimon's digivolution cards to either gain a memory or Recovery +1.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] By placing 1 [Leon Alexander] from your hand as this Digimon's bottom digivolution card, if you have 3 or more security cards, gain 1 memory. If you have 2 or fewer security cards, Recovery +1.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardNames.Contains("Leon Alexander") || cardSource.CardNames.Contains("LeonAlexander"))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
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
                        if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canSelectHand = card.Owner.HandCards.Count(CanSelectCardCondition) >= 1;

                    if (canSelectHand)
                    {
                        GManager.instance.userSelectionManager.SetBool(canSelectHand);
                    }

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                    List<CardSource> selectedCards = new List<CardSource>();

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    if (fromHand)
                    {
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
                    }

                    if (selectedCards.Count >= 1)
                    {
                        if (card.Owner.SecurityCards.Count >= 3)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                        }

                        if (card.Owner.SecurityCards.Count <= 2)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                        }

                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(selectedCards, activateClass));
                    }
                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().TopCard.HasText("Pulsemon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 1000, isInheritedEffect: true, card: card, condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}