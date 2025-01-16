using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT7_055 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Suspend 1 Digimon and gain Memory", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Suspend 1 of your opponent's Digimon. Then, gain 1 memory for each of your opponent's suspended Digimon.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        return true;
                    }

                    if (card.Owner.CanAddMemory(activateClass))
                    {
                        int count = card.Owner.Enemy.GetBattleAreaDigimons().Count((permanent) => permanent.IsSuspended);

                        if (count >= 1)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
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
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

                int count = card.Owner.Enemy.GetBattleAreaDigimons().Count((permanent) => permanent.IsSuspended);

                if (count >= 1)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(count, activateClass));
                }
            }
        }

        if (timing == EffectTiming.None)
        {
            AddSkillClass addSkillClass = new AddSkillClass();
            addSkillClass.SetUpICardEffect("Opponent's Digimon can't unsuspend unless opponent trash a card from hand", CanUseCondition, card);
            addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);

            cardEffects.Add(addSkillClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOpponentTurn(card))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (!permanent.TopCard.CanNotBeAffected(addSkillClass))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CardSourceCondition(CardSource cardSource)
            {
                if (CardEffectCommons.IsExistOnBattleArea(cardSource))
                {
                    if (cardSource == cardSource.PermanentOfThisCard().TopCard)
                    {
                        if (PermanentCondition(cardSource.PermanentOfThisCard()))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
            {
                if (_timing == EffectTiming.WhenUntapAnyone)
                {
                    ActivateClass activateClass1 = new ActivateClass();
                    activateClass1.SetUpICardEffect("Trash 1 card from hand or this Digimon can't unsuspend", CanUseCondition1, cardSource);
                    activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription());
                    activateClass1.SetRootCardEffect(addSkillClass);
                    cardEffects.Add(activateClass1);

                    string EffectDiscription()
                    {
                        return "[Your Turn] You must trash 1 card in your hand to unsuspend this Digimon.";
                    }

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        if (CardEffectCommons.IsOwnerTurn(cardSource))
                        {
                            if (CardSourceCondition(cardSource))
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    bool CanActivateCondition1(Hashtable hashtable)
                    {
                        if (CardSourceCondition(cardSource))
                        {
                            if (cardSource.PermanentOfThisCard().IsSuspended)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    IEnumerator ActivateCoroutine1(Hashtable _hashtable)
                    {
                        if (CardSourceCondition(cardSource))
                        {
                            if (cardSource.Owner.GetBattleAreaDigimons().Contains(cardSource.PermanentOfThisCard()))
                            {
                                bool discarded = false;

                                if (cardSource.Owner.HandCards.Count >= 1)
                                {
                                    int discardCount = 1;

                                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                    selectHandEffect.SetUp(
                                        selectPlayer: cardSource.Owner,
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
                                        cardEffect: activateClass1);

                                    yield return StartCoroutine(selectHandEffect.Activate());

                                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                                    {
                                        if (cardSources.Count >= 1)
                                        {
                                            discarded = true;

                                            yield return null;
                                        }
                                    }
                                }

                                if (!discarded)
                                {
                                    Permanent selectedPermanent = cardSource.PermanentOfThisCard();

                                    if (selectedPermanent != null)
                                    {
                                        string effectName = "This Digimon can't unsuspend.";

                                        bool CanUseCondition2()
                                        {
                                            return CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent);
                                        }

                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotUnsuspend(
                                                    targetPermanent: selectedPermanent,
                                                    effectDuration: EffectDuration.UntilNextUntap,
                                                    activateClass: activateClass1,
                                                    condition: CanUseCondition2,
                                                    effectName: effectName
                                                ));
                                    }
                                }
                            }
                        }
                    }
                }

                return cardEffects;
            }
        }

        return cardEffects;
    }
}
