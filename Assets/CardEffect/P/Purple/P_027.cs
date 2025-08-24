using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class P_027 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnDeclaration)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Use 1 Option from hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] <Digi-Burst 2> (Trash 2 of this Digimon's digivolution cards to activate the effect below.) - Use a purple Option card with a memory cost of 7 or less in your hand without paying its memory cost.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.IsOption &&
                       cardSource.CardColors.Contains(CardColor.Purple) &&
                       cardSource.HasUseCost && cardSource.GetCostItself <= 7 &&
                       !cardSource.CanNotPlayThisOption;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (new IDigiBurst(card.PermanentOfThisCard(), 2, activateClass).CanDigiBurst())
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(new IDigiBurst(card.PermanentOfThisCard(), 2, activateClass).DigiBurst());

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

                    selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

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
            }
        }

        return cardEffects;
    }
}
