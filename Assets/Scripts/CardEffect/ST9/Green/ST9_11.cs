using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class ST9_11 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
            addJogressConditionClass.SetUpICardEffect($"DNA Digivolution", CanUseCondition, card);
            addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
            addJogressConditionClass.SetNotShowUI(true);
            cardEffects.Add(addJogressConditionClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                return true;
            }

            JogressCondition GetJogress(CardSource cardSource)
            {
                if (cardSource == card)
                {
                    bool PermanentCondition1(Permanent permanent)
                    {
                        if (permanent != null)
                        {
                            if (permanent.TopCard != null)
                            {
                                if (permanent.TopCard.Owner == card.Owner)
                                {
                                    if (permanent.IsDigimon)
                                    {
                                        if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                        {
                                            if (permanent.TopCard.CardColors.Contains(CardColor.Green))
                                            {
                                                if (permanent.Levels_ForJogress(card).Contains(4))
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        return false;
                    }

                    bool PermanentCondition2(Permanent permanent)
                    {
                        if (permanent != null)
                        {
                            if (permanent.TopCard != null)
                            {
                                if (permanent.TopCard.Owner == card.Owner)
                                {
                                    if (permanent.IsDigimon)
                                    {
                                        if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                        {
                                            if (permanent.TopCard.CardColors.Contains(CardColor.Blue))
                                            {
                                                if (permanent.Levels_ForJogress(card).Contains(4))
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        return false;
                    }

                    JogressConditionElement[] elements = new JogressConditionElement[]
                    {
                        new JogressConditionElement(PermanentCondition1, "a level 4 green Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 4 blue Digimon"),
                    };

                    JogressCondition jogressCondition = new JogressCondition(elements, 0);

                    return jogressCondition;
                }

                return null;
            }
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Suspend 1 Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Suspend 1 of your opponent's Digimon. When DNA digivolving, that Digimon doesn't unsuspend during its owner's next unsuspend phase.";
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
                    return true;
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
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        foreach (Permanent selectedPermanent in permanents)
                        {
                            bool isJogress = CardEffectCommons.IsJogress(_hashtable);

                            if (isJogress)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantUnsuspendNextActivePhase(
                                    targetPermanent: selectedPermanent,
                                    activateClass: activateClass
                                ));
                            }
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.None)
        {
            int count()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    return card.PermanentOfThisCard().TopCard.CardColors.Count;
                }

                return 0;
            }

            bool Condition()
            {
                if (CardEffectCommons.IsOwnerTurn(card))
                {
                    if (count() >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect<Func<int>>(changeValue: () => 1000 * count(), isInheritedEffect: true, card: card, condition: Condition));
        }

        return cardEffects;
    }
}
