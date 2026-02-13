using System.Collections;
using System.Collections.Generic;

// FunBeemon
namespace DCGO.CardEffects.EX11
{
    public class EX11_025 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel2
                        && targetPermanent.TopCard.HasRoyalBaseTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 0,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion Alternative Digivolution Condition

            #region All Turns - Security

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistInSecurity(card, false)
                        && CardEffectCommons.IsOpponentTurn(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsTraits("Royal Base");
                }

                cardEffects.Add(CardEffectFactory.RebootStaticEffect(permanentCondition: PermanentCondition, isInheritedEffect: false, card: card, Condition));
            }

            #endregion

            #region Start of Your Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Add top face down security to hand. Place [Royal Base] face up to bottom of security.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Start of Your Main Phase] Add your top face-down security card to the hand. Then, you may place 1 [Royal Base] trait Digimon card from your hand face up as the bottom security card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool IsRoyalBaseDigimon(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.EqualsTraits("Royal Base");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    foreach (CardSource source in card.Owner.SecurityCards)
                    {
                        if (!source.IsFlipped)
                            continue;

                        yield return ContinuousController.instance.StartCoroutine(
                            CardObjectController.AddHandCards(new List<CardSource>() { source }, false, activateClass));

                        yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                            player: card.Owner,
                            refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());

                        break;
                    }

                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, IsRoyalBaseDigimon))
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsRoyalBaseDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.PutSecurityBottom,
                            cardEffect: activateClass);

                        selectHandEffect.SetIsFaceup();

                        yield return StartCoroutine(selectHandEffect.Activate());
                    }
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 1000, isInheritedEffect: true, card: card, condition: Condition));
            }

            #endregion

            return cardEffects;
        }
    }
}
