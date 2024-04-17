using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class Bakemon_BT15_073 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone || timing == EffectTiming.OnDestroyedAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Draw 1 and trash 1 card from hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] [On Deletion] <Draw 1>. Then, trash 1 card in your hand.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card) || CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (card.Owner.LibraryCards.Count >= 1 || card.Owner.HandCards.Count >= 1)
                {
                    if(CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            return true;
                        }
                    }

                    if (CardEffectCommons.CanActivateOnDeletion(card))
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                if (card.Owner.HandCards.Count >= 1)
                {
                    int discardCount = 1;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: (cardSource) => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: discardCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    yield return StartCoroutine(selectHandEffect.Activate());
                }
            }
        }

        if (timing == EffectTiming.OnDestroyedAnyone)
        {
            cardEffects.Add(CardEffectFactory.RetaliationSelfEffect(isInheritedEffect: true, card: card, condition: null));
        }

        return cardEffects;
    }
}