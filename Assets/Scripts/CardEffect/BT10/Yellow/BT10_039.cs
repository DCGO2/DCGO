using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.BT10
{
    public class BT10_039 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Plug-In] Option", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may use 1 Option card with [Plug-In] in its name from your hand without meeting its color requirements or paying its memory cost.";
                }

                bool OptionCondition(CardSource cardSource)
                {
                    if (cardSource.IsOption)
                    {
                        if (cardSource.ContainsCardName("Plug-In"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (OptionCondition(cardSource))
                    {
                        if (!cardSource.CanNotPlayThisOption)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.HandCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    #region ignore color requirement

                    IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                    ignoreColorConditionClass.SetUpICardEffect("ignore color condition", CanUseCondition1, card);
                    ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: OptionCondition);
                    Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                    card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                    ICardEffect GetCardEffect(EffectTiming _timing)
                    {
                        if (_timing == EffectTiming.None)
                        {
                            return ignoreColorConditionClass;
                        }

                        return null;
                    }

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return true;
                    }

                    #endregion

                    if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

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

                        selectHandEffect.SetUpCustomMessage(
                            "Select 1 option card to use.",
                            "The opponent is selecting 1 option card to use.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Used Card");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.PlayOptionCards(
                                cardSources: selectedCards,
                                activateClass: activateClass,
                                payCost: false,
                                root: SelectCardEffect.Root.Hand
                            )
                        );
                    }

                    #region release effect

                    card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);

                    #endregion
                }
            }

            return cardEffects;
        }
    }
}