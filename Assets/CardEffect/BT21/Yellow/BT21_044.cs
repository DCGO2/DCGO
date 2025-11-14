using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT21
{
    public class BT21_044 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("GeoGreymon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("[Marcus Damon] becomes a Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] For the turn, 1 of your [Marcus Damon]s is also treated as a 3000 DP Digimon, can't digivolve, and gains <Rush> and <Alliance>. Then, 1 of your Digimon may attack.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.EqualsCardName("Marcus Damon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanSelectOwnerPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                    {
                        int maxCount = Math.Min(1, card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect1.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine1,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect1.SetUpCustomMessage("Select 1 [Marcus Damon].", "The opponent is selecting 1 [Marcus Damon].");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                        IEnumerator SelectPermanentCoroutine1(Permanent selectedPermanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.BecomeDigimonThatCantDigivolve(targetPermanent: selectedPermanent, DP: 3000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRush(targetPermanent: selectedPermanent, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainAlliance(targetPermanent: selectedPermanent, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                        }
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnerPermanentCondition))
                    {
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect2 = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect2.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOwnerPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine2,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect2.SetUpCustomMessage("Select 1 Digimon that will attack.",
                            "The opponent is selecting 1 Digimon that will attack.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect2.Activate());

                        IEnumerator SelectPermanentCoroutine2(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null && selectedPermanent.CanAttack(activateClass))
                        {
                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                            selectAttackEffect.SetUp(
                                attacker: selectedPermanent,
                                canAttackPlayerCondition: () => true,
                                defenderCondition: _ => true,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                        }
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("[Marcus Damon] becomes a Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] For the turn, 1 of your [Marcus Damon]s is also treated as a 3000 DP Digimon, can't digivolve, and gains <Rush> and <Alliance>. Then, 1 of your Digimon may attack.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.EqualsCardName("Marcus Damon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanSelectOwnerPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                    {
                        int maxCount = Math.Min(1, card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect1.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine1,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect1.SetUpCustomMessage("Select 1 [Marcus Damon].", "The opponent is selecting 1 [Marcus Damon].");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                        IEnumerator SelectPermanentCoroutine1(Permanent selectedPermanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.BecomeDigimonThatCantDigivolve(targetPermanent: selectedPermanent, DP: 3000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRush(targetPermanent: selectedPermanent, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainAlliance(targetPermanent: selectedPermanent, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                        }
                    }                        

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnerPermanentCondition))
                    {
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect2 = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect2.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOwnerPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine2,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect2.SetUpCustomMessage("Select 1 Digimon that will attack.",
                            "The opponent is selecting 1 Digimon that will attack.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect2.Activate());

                        IEnumerator SelectPermanentCoroutine2(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null && selectedPermanent.CanAttack(activateClass))
                        {
                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                            selectAttackEffect.SetUp(
                                attacker: selectedPermanent,
                                canAttackPlayerCondition: () => true,
                                defenderCondition: _ => true,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                        }
                    }
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [Marcus Damon] on the top of security from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("AddSecurity_BT21_044");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns][Once Per Turn] When any of your yellow or red Tamers are deleted, you may place 1 [Marcus Damon] from your trash as the top security card.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.EqualsCardName("Marcus Damon"))
                    {
                        return true;
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.IsTamer)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.CanAddSecurity(activateClass))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            int maxCount = Math.Min(1, card.Owner.TrashCards.Count((cardSource) => CanSelectCardCondition(cardSource)));

                            List<CardSource> selectedCards = new List<CardSource>();

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectCardCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => false,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 [Marcus Damon] to add to security.",
                                        maxCount: maxCount,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Trash,
                                        customRootCardList: null,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 card to add to security.", "The opponent is selecting 1 card to add to security.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Security Card");

                            yield return StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            if (selectedCards.Count >= 1)
                            {
                                foreach (CardSource selectedCard in selectedCards)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(selectedCard));
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [Marcus Damon] on the top of security from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("ESSAddSecurity_BT21_044");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns][Once Per Turn] When any of your yellow or red Tamers are deleted, you may place 1 [Marcus Damon] from your trash as the top security card.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.EqualsCardName("Marcus Damon"))
                    {
                        return true;
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.IsTamer)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.CanAddSecurity(activateClass))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            int maxCount = Math.Min(1, card.Owner.TrashCards.Count((cardSource) => CanSelectCardCondition(cardSource)));

                            List<CardSource> selectedCards = new List<CardSource>();

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectCardCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => false,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 [Marcus Damon] to add to security.",
                                        maxCount: maxCount,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Trash,
                                        customRootCardList: null,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 card to add to security.", "The opponent is selecting 1 card to add to security.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Security Card");

                            yield return StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            if (selectedCards.Count >= 1)
                            {
                                foreach (CardSource selectedCard in selectedCards)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(selectedCard));
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}