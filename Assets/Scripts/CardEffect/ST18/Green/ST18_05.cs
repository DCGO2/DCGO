using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.ST18
{
    public class ST18_05 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region All Turns

            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 of your Digimon gains 3000 DP.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("Buff_ST18_05");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns] (Once Per Turn) When this Digimon is suspended by an effect, 1 of your Digimon with [Bird] in one of its traits or with the [Vortex Warriors] trait gets +3000 DP until then end of your opponent's turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable,
                               permanent => permanent == card.PermanentOfThisCard()) &&
                           CardEffectCommons.IsByEffect(hashtable, null);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if(CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        return permanent.TopCard.ContainsTraits("Avian") ||
                               permanent.TopCard.ContainsTraits("Bird") ||
                               permanent.TopCard.ContainsTraits("Vortex Warriors");
                    }
                    
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 Digimon that will get +3000 DP.",
                        "The opponent is selecting 1 Digimon that will get +3000 DP.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            targetPermanent: permanent,
                            changeValue: 3000,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));
                    }
                }
            }

            #endregion

            #region Piercing - ESS

            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(
                    isInheritedEffect: true,
                    card: card,
                    condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}