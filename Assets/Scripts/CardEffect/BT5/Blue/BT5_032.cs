using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT5_032 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnAllyAttack)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Trash digivolution cards and gain Jamming", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Attacking] Trash up to 2 digivolution cards from the bottom of 1 of your opponent's Digimon. Then, if your opponent has a Digimon with no digivolution cards in play, this Digimon gains <Jamming> (This Digimon can't be deleted in battles against Security Digimon) for the turn.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    return true;
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will trash digivolution cards.", "The opponent is selecting 1 Digimon that will trash digivolution cards.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    Permanent selectedPermanent = permanent;

                    int maxCount = Math.Min(2, permanent.DigivolutionCards.Count);

                    SelectCountEffect selectCountEffect = GManager.instance.GetComponent<SelectCountEffect>();

                    selectCountEffect.SetUp(
                        SelectPlayer: card.Owner,
                        targetPermanent: permanent,
                        MaxCount: maxCount,
                        CanNoSelect: false,
                        Message: "How many digivolution cards will you trash?",
                        Message_Enemy: "The opponent is choosing how many digivolution cards to trash.",
                        SelectCountCoroutine: SelectCountCoroutine);

                    yield return ContinuousController.instance.StartCoroutine(selectCountEffect.Activate());

                    IEnumerator SelectCountCoroutine(int count)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedPermanent, trashCount: count, isFromTop: false, activateClass: activateClass));
                    }
                }

                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, (permanent) => permanent.IsDigimon && permanent.HasNoDigivolutionCards))
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainJamming(targetPermanent: selectedPermanent, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                }
            }
        }

        if (timing == EffectTiming.None)
        {
            bool AttackerCondition(Permanent attacker)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(attacker, card))
                {
                    if (attacker.HasNoDigivolutionCards)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool Condition()
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }
            
            cardEffects.Add(CardEffectFactory.CanNotAttackStaticEffect(attackerCondition: AttackerCondition, defenderCondition: null, isInheritedEffect: false, card: card, condition: Condition, effectName: "Opponent's Digimon without digivolution cards can't Attack"));
        }

        if (timing == EffectTiming.None)
        {
            bool DefenderCondition(Permanent attacker)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(attacker, card))
                {
                    if (attacker.HasNoDigivolutionCards)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool Condition()
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            cardEffects.Add(CardEffectFactory.CanNotBlockStaticEffect(attackerCondition: null, defenderCondition: DefenderCondition, isInheritedEffect: false, card: card, condition: Condition, effectName: "Opponent's Digimon without digivolution cards can't Block"));
        }

        return cardEffects;
    }
}
