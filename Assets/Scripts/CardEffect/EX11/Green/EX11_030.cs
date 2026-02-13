using System.Collections;
using System.Collections.Generic;

// ForgeBeemon
namespace DCGO.CardEffects.EX11
{
    public class EX11_030 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel3 &&
                           targetPermanent.TopCard.EqualsTraits("Royal Base");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false,
                    card: card, condition: null));
            }

            #endregion

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

            #region OP/WD Shared

            string SharedEffectName() => "Add top face down security to hand. Place [Royal Base] face up to bottom of security.";

            string SharedEffectDescription(string tag) => $"[{tag}] Add your top face-down security card to the hand. Then, you may place 1 [Royal Base] trait Digimon card from your hand face up as the bottom security card.";

            bool SharedCanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            bool IsRoyalBaseDigimon(CardSource cardSource)
            {
                return cardSource.IsDigimon
                    && cardSource.EqualsTraits("Royal Base");
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
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

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(hash => SharedCanActivateCondition(hash, activateClass), (hashTable) => SharedActivateCoroutine(hashTable, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card)
                        && CardEffectCommons.IsExistOnBattleArea(card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(hash => SharedCanActivateCondition(hash, activateClass), (hashTable) => SharedActivateCoroutine(hashTable, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card)
                        && CardEffectCommons.IsExistOnBattleArea(card);
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
