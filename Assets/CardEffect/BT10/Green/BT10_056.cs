using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.BT10
{
    public class BT10_056 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 Digimon and it can't unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Suspend 1 of your opponent's Digimon. That Digimon doesn't unsuspend during their next unsuspend phase.";
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
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        foreach (Permanent selectedPermanent in permanents)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantUnsuspendNextActivePhase(
                                    targetPermanent: selectedPermanent,
                                    activateClass: activateClass
                                ));
                        }
                    }
                }
            }

            if (timing == EffectTiming.None)
            {
                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("Your other Digimons gain effects", CanUseCondition, card);
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
                    if (permanent != card.PermanentOfThisCard())
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                        {
                            if (permanent.TopCard.HasPlantTraits)
                            {
                                return true;
                            }

                            if (permanent.TopCard.HasFairyTraits)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    if (PermanentCondition(cardSource.PermanentOfThisCard()))
                    {
                        if (cardSource == cardSource.PermanentOfThisCard().TopCard)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                {
                    if (_timing == EffectTiming.OnDestroyedAnyone)
                    {
                        ActivateClass activateClass1 = new ActivateClass();
                        activateClass1.SetUpICardEffect("Memory +2 and return 1 card from trash to hand", CanUseCondition1, cardSource);
                        activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription());
                        activateClass1.SetRootCardEffect(addSkillClass);
                        activateClass1.SetHashString("Memory+2_BT10_056");
                        cardEffects.Add(activateClass1);

                        string EffectDiscription()
                        {
                            return "[On Deletion] Gain 2 memory, and return 1 Digimon card with 3000 DP or less from your trash to your hand.";
                        }

                        bool CanSelectCardCondition(CardSource cardSource1)
                        {
                            if (cardSource1.IsDigimon)
                            {
                                if (cardSource1.CardDP <= 3000)
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            if (CardSourceCondition(cardSource))
                            {
                                if (CardEffectCommons.CanTriggerOnDeletion(hashtable, cardSource))
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        bool CanActivateCondition1(Hashtable hashtable)
                        {
                            if (CardEffectCommons.CanActivateOnDeletion(cardSource))
                            {
                                return true;
                            }

                            return false;
                        }

                        IEnumerator ActivateCoroutine1(Hashtable _hashtable)
                        {
                            yield return ContinuousController.instance.StartCoroutine(cardSource.Owner.AddMemory(2, activateClass1));

                            if (cardSource.Owner.TrashCards.Count(CanSelectCardCondition) >= 1)
                            {
                                int maxCount = Math.Min(1, card.Owner.TrashCards.Count(CanSelectCardCondition));

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 card to add to your hand.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.AddHand,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass1);

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                            }
                        }
                    }

                    return cardEffects;
                }
            }

            return cardEffects;
        }
    }
}