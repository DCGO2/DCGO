using System;
using System.Collections;
using System.Collections.Generic;

// LordKnightmon
namespace DCGO.CardEffects.BT22
{
    public class BT22_067 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel5
                        && targetPermanent.TopCard.HasCSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region Rio Kishibe Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Rie Kishibe");
                }

                bool Condition()
                {
                    return card.Owner.SecurityCards.Count <= 3;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 5,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: Condition)
                );
            }

            #endregion

            #region Raid

            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region Reboot

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #endregion

            #region OP/WD Shared

            bool SharedIsYourDigimon(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
            }

            bool SharedAttackingDigimon(Permanent permanent, ActivateClass activateClass)
            {
                return SharedIsYourDigimon(permanent)
                    && permanent.CanAttack(activateClass);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, SharedIsYourDigimon))
                {
                    Permanent selectedYourPermanent = null;

                    #region Select Gain DP Permanent

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, SharedIsYourDigimon));
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: SharedIsYourDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedYourPermanent = permanent;
                        yield return null;
                    }

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to gain 3K DP", "The opponent is selecting 1 Digimon to gain 3K DP");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    #endregion

                    if (selectedYourPermanent != null) yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.ChangeDigimonDP(selectedYourPermanent, 3000, EffectDuration.UntilOpponentTurnEnd, activateClass));

                    Permanent selectedAttackingPermament = null;

                    #region Select Permanent To Attack

                    int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, perm => SharedAttackingDigimon(perm, activateClass)));
                    SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();
                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: perm => SharedAttackingDigimon(perm, activateClass),
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine1,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    IEnumerator SelectPermanentCoroutine1(Permanent permanent)
                    {
                        selectedAttackingPermament = permanent;
                        yield return null;
                    }

                    selectPermanentEffect1.SetUpCustomMessage("Select 1 Digimon to attack", "The opponent is selecting 1 Digimon to attack");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                    #endregion

                    if (selectedAttackingPermament != null && selectedAttackingPermament.CanAttack(activateClass))
                    {
                        #region Setup SelectAttackEffect

                        SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                        selectAttackEffect.SetUp(
                            attacker: selectedAttackingPermament,
                            canAttackPlayerCondition: () => true,
                            defenderCondition: (permanent) => false,
                            cardEffect: activateClass);

                        #endregion

                        yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                    }
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("+3K DP, then 1 digimon may attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] 1 of your Digimon gets +3000 DP until your opponent's turn ends. Then, 1 of your Digimon may attack a player.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return SharedActivateCoroutine(hashtable, activateClass);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("+3K DP, then 1 digimon may attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] 1 of your Digimon gets +3000 DP until your opponent's turn ends. Then, 1 of your Digimon may attack a player.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return SharedActivateCoroutine(hashtable, activateClass);
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal top 3, Play 1 4 cost or lower Black or Red card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("BT22_067_PlayTamer");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When Digimon attack players, reveal the top 3 cards of your deck. You may play 1 play cost 4 or lower black or red card among them without paying the cost. Trash the rest.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, IsAttackingPlayer);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool IsAttackingPlayer(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent)
                        && GManager.instance.attackProcess.IsAttacking
                        && GManager.instance.attackProcess.DefendingPermanent == null;
                }

                bool IsProperCard(CardSource cardSource)
                {
                    return cardSource.HasPlayCost && cardSource.BasePlayCostFromEntity <= 4 &&
                           (cardSource.CardColors.Contains(CardColor.Black) || cardSource.CardColors.Contains(CardColor.Red)) &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource,false,activateClass,SelectCardEffect.Root.Library);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedCard = null;

                    #region Select Card

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:IsProperCard,
                            message: "Select 1 4 or lower play cost Black/Red card",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: 1,
                            selectCardCoroutine: SelectCardCoroutine),
                        },
                        remainingCardsPlace: RemainingCardsPlace.Trash,
                        activateClass: activateClass
                    ));

                    #endregion

                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddExecutingCard(selectedCard));
                    if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.PlayPermanentCards(new List<CardSource>() { selectedCard }, activateClass, false, false, SelectCardEffect.Root.Execution, true));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}