using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT20
{
    public class BT20_003 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If this has no tamers in source, you may place 1 tamer with Pulsemon in text, or Soc or SEEKERS trait as a source", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn] [Once Per Turn] You may place 1 of your Tamers with [Pulsemon] in its text or the [SoC] or [SEEKERS] trait as the bottom digivolution card of this Digimon with no Tamer cards in its digivolution cards.";
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

                bool CanBeSelected(CardSource card)
                {
                    if (card.IsTamer && (card.HasText("Pulsemon") || card.HasSocTraits || card.HasSeekersTraits))
                    {
                        return true;
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, permanent => permanent.cardSources.Filter(source => source.IsTamer).Count == 0))
                        {
                            if (card.Owner.GetBattleAreaPermanents().Exists(permanent => CanBeSelected(permanent.TopCard)))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanBeSelected,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select a tamer to add to source",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Custom,
                        customRootCardList: null,
                        canLookReverseCard: false,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass
                        );

                    selectCardEffect.SetUpCustomMessage("Select 1 card to add to source", "The opponent is selecting 1 card to add to source.");
                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    // effect to add Card underneath
                }
            }

            return cardEffects;
        }
    }
}