using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT12
{
    public class BT12_044 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Security Attack -2", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Until the end of your opponent's turn, 1 of your opponent's Digimon gains <Security Attack -2>. (This Digimon checks 2 fewer security cards.)";
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
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage(
                            "Selcect 1 Digimon that will get Security Attack -2.",
                            "The opponent is selecting 1 Digimon that will get Security Attack -2.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(
                                targetPermanent: permanent,
                                changeValue: -2,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));
                        }
                    }

                    //Initial check for your turn effect

                    int count()
                    {
                        return card.Owner.Enemy.GetBattleAreaDigimons().Count((permanent) => permanent.HasSecurityAttackChanges);
                    }

                    List<ICardEffect> SecFromLamp = card.PermanentOfThisCard().EffectList(EffectTiming.None).Where(x => x.HashString == "SecAttackFromLampEffect").ToList();

                    if (count() > SecFromLamp.Count())
                    {
                        int timesToRunLoop = count() - SecFromLamp.Count();

                        for (int i = 0; i < timesToRunLoop; i++)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(
                            targetPermanent: card.PermanentOfThisCard(),
                            changeValue: 1,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass,
                            activateAnimation: false,
                            hashstring: "SecAttackFromLampEffect"));
                        }
                    }
                }
            }

            #endregion

            #region Your Turn

            if (timing == EffectTiming.AfterEffectsActivate || timing == EffectTiming.OnStartTurn || timing == EffectTiming.OnEnterFieldAnyone || timing == EffectTiming.OnUseOption || timing == EffectTiming.OptionSkill || timing == EffectTiming.SecuritySkill || timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain Security Attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsBackgroundProcess(true);
                cardEffects.Add(activateClass);

                int count()
                {
                    return card.Owner.Enemy.GetBattleAreaDigimons().Count((permanent) => permanent.HasSecurityAttackChanges);
                }

                string EffectDiscription()
                {
                    return "[Your Turn] This Digimon gains [Security A+1] for each of your opponent's Digimon with [Security Attack].";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                bool OpponentHasDigimonWithSecChanges(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.IsDigimon)
                        {
                            if (permanent.HasSecurityAttackChanges)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(OpponentHasDigimonWithSecChanges))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<ICardEffect> SecFromLamp = card.PermanentOfThisCard().EffectList(EffectTiming.None).Where(x => x.HashString == "SecAttackFromLampEffect").ToList();

                    if (count() > SecFromLamp.Count())
                    {
                        int timesToRunLoop = count() - SecFromLamp.Count();

                        for (int i = 0; i < timesToRunLoop; i++)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(
                            targetPermanent: card.PermanentOfThisCard(),
                            changeValue: 1,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass,
                            activateAnimation: false,
                            hashstring: "SecAttackFromLampEffect"));
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}