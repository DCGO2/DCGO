using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DCGO.CardEffects.BT20
{
    public class BT20_035 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivovle Cost

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel5 &&
                           (targetPermanent.TopCard.HasText("Pulsemon") ||
                           targetPermanent.TopCard.HasSeekersTraits);
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false,
                    card: card, condition: null));
            }

            #endregion

            #region Fortitude
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.FortitudeSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 Digimon or Tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] Suspend 1 of your opponent's Digimon or Tamers. Then, 1 of their Digimon or Tamers can't unsuspend until the end of their turn.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card) &&
                           (permanent.IsDigimon || permanent.IsTamer);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentToSuspend = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentToSuspend.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentToSuspend.Activate());
                        
                        SelectPermanentEffect selectPermanentToNotUnsuspend = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentToNotUnsuspend.SetUp(
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

                        selectPermanentToNotUnsuspend.SetUpCustomMessage("Select 1 Digimon or Tamer that will be unable to unsuspend.", "The opponent is selecting 1 Digimon or Tamer that will be unable to unsuspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentToNotUnsuspend.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantUnsuspendUntilOpponentTurnEnd(
                                targetPermanent: selectedPermanent,
                                activateClass: activateClass
                            ));
                        }
                    }

                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnAddDigivolutionCards)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Activate [When Digiolving] effect, Then may attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When Tamer cards are placed in this Digimon's digivolution cards, activate 1 of this Digimon's [When Digivolving] effects. Then, 1 of your Digimon may attack your opponent's Digimon.";
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.CanAttack(activateClass))
                        {
                            if (card.Owner.Enemy.GetBattleAreaDigimons().Count((enemyDigimon) => permanent.CanAttackTargetDigimon(enemyDigimon, activateClass)) >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnAddDigivolutionCard(
                               hashtable: hashtable,
                               permanentCondition: permanent => permanent == card.PermanentOfThisCard(),
                               cardEffectCondition: null,
                               cardCondition: cardSource =>
                                   cardSource.IsTamer);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<ICardEffect> candidateEffects = card.PermanentOfThisCard().EffectList(EffectTiming.OnEnterFieldAnyone)
                       .Clone()
                       .Filter(cardEffect => cardEffect != null && cardEffect is ActivateICardEffect && !cardEffect.IsSecurityEffect && cardEffect.IsWhenDigivolving);

                    if (candidateEffects.Count >= 1)
                    {
                        ICardEffect selectedEffect = null;

                        if (candidateEffects.Count == 1)
                        {
                            selectedEffect = candidateEffects[0];
                        }
                        else
                        {
                            List<SkillInfo> skillInfos = candidateEffects
                                .Map(cardEffect => new SkillInfo(cardEffect, null, EffectTiming.None));

                            List<CardSource> cardSources = candidateEffects
                                .Map(cardEffect => cardEffect.EffectSourceCard);

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: (cardSource) => true,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 effect to activate.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: false,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: cardSources,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetNotShowCard();
                            selectCardEffect.SetUpSkillInfos(skillInfos);
                            selectCardEffect.SetUpAfterSelectIndexCoroutine(AfterSelectIndexCoroutine);

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator AfterSelectIndexCoroutine(List<int> selectedIndexes)
                            {
                                if (selectedIndexes.Count == 1)
                                {
                                    selectedEffect = candidateEffects[selectedIndexes[0]];
                                    yield return null;
                                }
                            }
                        }

                        if (selectedEffect != null)
                        {
                            Hashtable effectHashtable = CardEffectCommons.WhenDigivolvingCheckHashtableOfCard(selectedEffect.EffectSourceCard);

                            if (!selectedEffect.IsDisabled)
                            {
                                selectedEffect.SetIsDigimonEffect(true);
                                yield return ContinuousController.instance.StartCoroutine(
                                ((ActivateICardEffect)selectedEffect).Activate_Optional_Effect_Execute(effectHashtable));
                            }
                        }
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                    {
                        List<Permanent> selectedPermanents = new List<Permanent>();

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition1));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition1,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will attack.", "The opponent is selecting 1 Digimon that will attack.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            selectedPermanents = permanents;
                            yield return null;
                        }

                        foreach (Permanent permanent in selectedPermanents)
                        {
                            Permanent selectedPermanent = permanent;

                            if (selectedPermanent != null)
                            {
                                if (selectedPermanent.CanAttack(activateClass))
                                {
                                    SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                    selectAttackEffect.SetUp(
                                        attacker: selectedPermanent,
                                        canAttackPlayerCondition: () => false,
                                        defenderCondition: (permanent) => true,
                                        cardEffect: activateClass);

                                    yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region ESS

            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<Recovery +1(Deck)>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("Recover_BT20-035");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When your security stack is removed from, if this Digimon has [Fenriloogamon] in its name, <Recovery +1(Deck)>.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, player => player == card.Owner);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.PermanentOfThisCard().TopCard.ContainsCardName("Fenriloogamon");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}