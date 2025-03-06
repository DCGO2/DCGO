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

            if (timing == EffectTiming.OnStartMainPhase)
            {
                #region start of main phase
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash a card, draw a card, gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] By trashing 1 card with [Huckmon] or [Sistermon] in its name or the [Royal Knight] trait";
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
                    }

                    if (discarded)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    }
                }
                #endregion
            }
            
            if (timing == EffectTiming.None)
            {
                #region inherited
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("All Digimon gain DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription ()
                {
                    return "[Your Turn] All of your Digimon get +1000 DP.";
                }

                bool CanUseCondition (Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea (card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine (Hashtable hashtable)
                {
                    List<Permanent> permanents = card.Owner.GetBattleAreaDigimons();

                    if (permanents.Count >= 1)
                    {
                        foreach (Permanent permanent in permanents)
                        {
                            yield return CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: 1000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass);
                        }
                    }
                }
                #endregion
            }
            return cardEffects;
        }
    }
}