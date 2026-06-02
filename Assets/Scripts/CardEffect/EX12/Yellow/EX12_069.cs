using System.Collections;
using System.Collections.Generic;

// Virus Busters
namespace DCGO.CardEffects.EX12
{
    public class EX12_069 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Use Req
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("VB");
                }
            }
            #endregion

            #region Your Turn Security
            if (timing == EffectTiming.OnAllyAttack)
            {
                int AttackingLevel = 0;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May play same level as attacker [VB] for 3 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Security] [Your Turn] [Once Per Turn] When one of your level 4 or higher [VB] trait Digimon attacks, you may play 1 [VB] trait Digimon card of the same level from your hand with the cost reduced by 3.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistInSecurity(card, false)
                    && CardEffectCommons.IsOwnerTurn(card)
                    && CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, PermanentCondition);

                bool CanActivateCondition(Hashtable hashtable)
                {
                    Permanent attackingPermanent = CardEffectCommons.GetAttackerFromHashtable(hashtable);
                    AttackingLevel = attackingPermanent.Level;

                    return CardEffectCommons.IsExistInSecurity(card, false)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                }

                bool PermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.HasLevel
                    && permanent.TopCard.Level >= 4
                    && permanent.TopCard.EqualsTraits("VB");

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.EqualsTraits("Shambala")
                    && cardSource.HasLevel
                    && cardSource.Level == AttackingLevel
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource, true, activateClass, fixedCost: cardSource.GetCostItself - 3);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.PlayForCost,
                        cardEffect: activateClass);

                    selectHandEffect.SetReducedCostTuple((3, null));
                    selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                }
            }
            #endregion

            #region Main Effect
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Replace your bottom sec with this face-up card", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] Add your bottom security card to the hand and place this card face up as the bottom security card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectFactory.ReplaceBottomSecurityWithFaceUpOptionEffect(card, activateClass));
                }
            }
            #endregion

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 lvl 5 or lower [VB] Digimon card from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                 => "[Security] You may play 1 level 5 or lower [VB] trait Digimon card from your hand without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                bool CanPlayCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel
                        && cardSource.Level <= 5
                        && cardSource.EqualsTraits("VB")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectHandEffect selectHandEffect2 = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect2.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanPlayCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.PlayForFree,
                        cardEffect: activateClass);

                    yield return StartCoroutine(selectHandEffect2.Activate());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
