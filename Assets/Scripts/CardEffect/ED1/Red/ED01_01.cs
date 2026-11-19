using System.Collections;
using System.Collections.Generic;

// Rubeusdramon
namespace DCGO.CardEffects.ED01
{
    public class ED01_01 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region When Digivolving Blitz
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                cardEffects.Add(CardEffectFactory.BlitzSelfEffect(isInheritedEffect: false, card: card, condition: null, isWhenDigivolving: true));
            }
            #endregion

            Permanent thisPermanent = card.PermanentOfThisCard();

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete up to 6K DP enemy Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] Delete up to 6000 DP total worth of your opponent's Digimon.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);
                }

                int MaxTotalDP()
                {
                    return card.Owner.MaxDP_DeleteEffect(6000, activateClass);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.HasDP
                        && permanent.DP <= MaxTotalDP();

                int SumDP(List<Permanent> permanents)
                {
                    int sumDP = 0;

                    foreach (Permanent permanent in permanents)
                    {
                        sumDP += permanent.DP;
                    }

                    return sumDP;
                }

                bool CanTargetConditionByPreSelectedList(List<Permanent> permanents, Permanent permanent)
                    => SumDP(permanents) + permanent.DP <= MaxTotalDP();

                bool CanEndSelectCondition(List<Permanent> permanents)
                    => permanents.Count >= 1
                        && SumDP(permanents) <= MaxTotalDP();

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition)) yield break;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: CanTargetConditionByPreSelectedList,
                        canEndSelectCondition: CanEndSelectCondition,
                        maxCount: CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition),
                        canNoSelect: false,
                        canEndNotMax: true,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage($"Select Digimon to delete (up to {MaxTotalDP()} DP total).", "The opponent is selecting Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && permanent.TopCard.EqualsCardName("Kanata Hondou");
                }

                bool CanUseCondition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, PermanentCondition);
                }

                cardEffects.Add(CardEffectFactory.PierceSelfEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition
                ));
            }
            #endregion

            return cardEffects;
        }
    }
}
