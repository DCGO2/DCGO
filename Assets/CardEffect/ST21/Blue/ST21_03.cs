using System.Collections;
using System.Collections.Generic;

//ST21 Ikkakumon
namespace DCGO.CardEffects.ST21
{
    public class ST21_03 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.Level == 3 && permanent.TopCard.HasAdventureTraits;
                }
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 2, false, card, null));
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfDigimonAfterBattleSecurityEffect(card: card));
            }

            #endregion

            #region On Play/When Digivolving shared

            bool CanActivateConditonShared(Hashtable hashtable)
                => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool CanSelectPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool CanSelectPermanentCondition2(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.HasNoDigivolutionCards;

                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, permanent => CanSelectPermanentCondition(permanent)))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: (permanent) => CanSelectPermanentCondition(permanent),
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will trash digivolution cards.", "The opponent is selecting 1 Digimon that will trash digivolution cards.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanent, trashCount: 2, isFromTop: true, activateClass: activateClass));
                    }
                }

                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, permanent => CanSelectPermanentCondition2(permanent)))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: (permanent) => CanSelectPermanentCondition2(permanent),
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine2,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that cant attack or block.", "The opponent is selecting 1 Digimon that cant attack or block");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    IEnumerator SelectPermanentCoroutine2(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotAttack(
                                targetPermanent: permanent,
                                defenderCondition: null,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass,
                                effectName: "Can't Attack"));

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotBlock(
                            targetPermanent: permanent,
                            attackerCondition: null,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass,
                            effectName: "Can't Block"));
                    }
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Strip top 2, then freeze", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditonShared, (hashtable) => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Play] Trash the top 2 digivolution cards of 1 of your opponent's Digimon. Then, 1 of their Digimon with no digivolution cards can't attack or block until their turn ends.";

                bool CanUseCondition(Hashtable hashtable)
                => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Strip top 2, then freeze", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditonShared, (hashtable) => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Digivolving] Trash the top 2 digivolution cards of 1 of your opponent's Digimon. Then, 1 of their Digimon with no digivolution cards can't attack or block until their turn ends.";

                bool CanUseCondition(Hashtable hashtable)
                => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            #endregion

            #region When Attacking - ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("WhenAttacking_ST21_03");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Attacking] [Once Per Turn] If you have 7 or fewer cards in your hand, <Draw 1>. (Draw 1 card from your deck.)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.HandCards.Count <= 7)
                        {
                            if (card.Owner.LibraryCards.Count >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}