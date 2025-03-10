using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT20
{
    public class BT20_008 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region start of main phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash a card, draw a card, gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] By trashing 1 card with [Huckmon] or [Sistermon] in its name or the [Royal Knight] trait in your hand, <Draw 1> and gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanSelect(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.ContainsCardName("Sistermon") || cardSource.ContainsCardName("Huckmon") || cardSource.HasRoyalKnightTraits)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                    bool discarded = false;
                    if (card.Owner.HandCards.Count(CanSelect) >= 1)
                    {
                        int maxCount = 1;
                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelect,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);


                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count == 1)
                            {
                                discarded = true;
                                yield return null;
                            }
                        }

                        selectHandEffect.SetUpCustomMessage("Select 1 card to discard.", "The opponent is selecting 1 card to discard.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Discarded Card");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    }

                    if (discarded)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    }
                }
            }
            #endregion

            #region ESS
            if (timing == EffectTiming.None)
            {
                bool Condition()
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

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        return true;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                    permanentCondition: PermanentCondition,
                    changeValue: 1000,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition,
                    effectName: () => "Your Digimons gain DP +1000"));
            }
            #endregion

            return cardEffects;
        }
    }
}