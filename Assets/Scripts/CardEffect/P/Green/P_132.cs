using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.P
{
    public class P_132 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("+2000 DP, until end of opponent's turn", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By suspending 1 Digimon, this Digimon gets +2000 DP until the end of your opponent's turn.";
                }

                bool SelectDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: SelectDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectedPermanent,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectedPermanent(Permanent permanent)
                    {
                        if (permanent != null)
                        {
                            if (permanent.TopCard != null)
                            {
                                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    if (!permanent.IsSuspended && permanent.CanSuspend)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                            targetPermanent: card.PermanentOfThisCard(),
                                            changeValue: 2000,
                                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                            activateClass: activateClass));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                bool HasShotoKazama(Permanent permanent)
                {
                    return permanent.TopCard.CardNames.Contains("Shoto Kazama");
                }

                bool Condition()
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasShotoKazama))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.PierceSelfEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition));
            }
            #endregion

            #region Your Turn - ESS
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        return true;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 2000,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition));
            }
            #endregion
            
            return cardEffects;
        }
    }
}