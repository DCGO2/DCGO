using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT16
{
    public class BT16_102 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Armor Purge and Blocker

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.ArmorPurgeEffect(card: card));
            }

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.ContainsCardName("Magnamon") && targetPermanent.TopCard.CardColors.Count == 2)
                    {
                        return true;
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 5,
                    ignoreDigivolutionRequirement: true,
                    card: card,
                    condition: null));
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Become unaffected by effects, gain DP and unsuspend.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If [Magnamon (X-Antibody)] or a card with the [Armor Form] trait is in this Digimon's digivolution cards, this Digimon isn't affected by your opponent's effects and gains +3000 DP until the end of your opponent's turn. Then, unsuspend this Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                    {
                        return CardEffectCommons.IsExistOnBattleArea(card);
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }
                    
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    if (selectedPermanent != null)
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) =>
                                (cardSource.EqualsCardName("Magnamon (X Antibody)") ||
                                cardSource.ContainsTraits("Armor Form")) &&
                                !cardSource.IsFlipped)>= 1)
                        {
                            CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                            canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's effects", CanUseCondition1, card);
                            canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                            selectedPermanent.UntilOpponentTurnEndEffects.Add((_timing) => canNotAffectedClass);

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                return CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent);
                            }

                            bool CardCondition(CardSource cardSource)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                {
                                    if (cardSource == selectedPermanent.TopCard)
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            bool SkillCondition(ICardEffect cardEffect)
                            {
                                if (cardEffect != null)
                                {
                                    if (cardEffect.EffectSourceCard != null)
                                    {
                                        if (cardEffect.EffectSourceCard.Owner == card.Owner.Enemy)
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: selectedPermanent,
                                changeValue: 3000,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));
                        }

                        if (CardEffectCommons.CanUnsuspend(selectedPermanent))
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());
                        }
                    }
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Activate one of this Digimon's [When Digivolving] effects.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT16_102_AllTurns");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns][Once per turn] When a card is removed from a security stack, you may activate 1 of this Digimon's [When Digivolving] effects.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, player => player == card.Owner || player == card.Owner.Enemy))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, player => player == card.Owner || player == card.Owner.Enemy))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
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
                                yield return ContinuousController.instance.StartCoroutine(
                                ((ActivateICardEffect)selectedEffect).Activate_Optional_Effect_Execute(effectHashtable));
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
