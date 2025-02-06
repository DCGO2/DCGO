using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT6_077 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Trash 1 card from hand to gain Blocker and Retaliation", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] You may trash 1 card in your hand to have this Digimon gain <Blocker> (When an opponent's Digimon attacks, you may suspend this Digimon to force the opponent to attack it instead) and <Retaliation> (When this Digimon is deleted after losing a battle, delete the Digimon it was battling) until the end of your opponent's next turn.";
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
                if (isExistOnField(card))
                {
                    if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                    {
                        if (card.Owner.HandCards.Count >= 1)
                        {
                            bool discarded = false;

                            int discardCount = 1;

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: (cardSource) => true,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: discardCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                mode: SelectHandEffect.Mode.Discard,
                                cardEffect: activateClass);

                            yield return StartCoroutine(selectHandEffect.Activate());

                            IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                            {
                                if (cardSources.Count >= 1)
                                {
                                    discarded = true;

                                    yield return null;
                                }
                            }

                            if (discarded)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(targetPermanent: card.PermanentOfThisCard(), effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRetaliation(targetPermanent: card.PermanentOfThisCard(), effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                            }
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.None)
        {
            ChangeCardColorClass changeCardColorClass = new ChangeCardColorClass();
            changeCardColorClass.SetUpICardEffect($"Also treated as black", CanUseCondition, card);
            changeCardColorClass.SetUpChangeCardColorClass(ChangeCardColors: ChangeCardColors);

            cardEffects.Add(changeCardColorClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    return true;
                }

                return false;
            }



            List<CardColor> ChangeCardColors(CardSource cardSource, List<CardColor> CardColors)
            {
                if (cardSource == card)
                {
                    CardColors.Add(CardColor.Black);
                }

                return CardColors;
            }
        }

        return cardEffects;
    }
}
