using System.Collections;
using System.Collections.Generic;
using System.Linq;

//ShinMonzaemon
namespace DCGO.CardEffects.BT22
{
    public class BT22_076 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.HasDMTraits)
                    {
                        if (targetPermanent.TopCard.HasLevel)
                        {
                            if (targetPermanent.Level == 5)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 5, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Static Effects

            #region Reduce Cost

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return !CardEffectCommons.IsExistOnField(card);
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent != null)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card))
                        {
                            if (targetPermanent.TopCard.EqualsTraits("Ver.1"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                cardEffects.Add(
                    CardEffectFactory.ChangeDigivolutionCostStaticEffect(
                        changeValue: -2,
                        permanentCondition: PermanentCondition,
                        cardCondition: CardSourceCondition,
                        rootCondition: RootCondition,
                        isInheritedEffect: false,
                        card: card,
                        condition: Condition,
                        setFixedCost: false));
            }

            #endregion

            #region Security Atk +1

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region Armor Purge

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.ArmorPurgeEffect(card: card));
            }

            #endregion

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash face down source, Place 1 in security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("TrashEffect_BT22-076");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] [Once Per Turn] By trashing this Digimon's bottom face-down digivolution card, place 1 Digimon with as much or less DP as this Digimon as the top security card.";
                }

                bool CanSelectTrashSourceCardCondition(CardSource cardSource)
                {
                    return cardSource.IsFlipped && !cardSource.CanNotTrashFromDigivolutionCards(activateClass);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent) &&
                           permanent.HasDP && permanent.DP <= card.PermanentOfThisCard().DP;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.PermanentOfThisCard().DigivolutionCards.Exists(CanSelectTrashSourceCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    CardSource trashTargetCard = card.PermanentOfThisCard().DigivolutionCards.Filter(CanSelectTrashSourceCardCondition)[^1];

                    selectedCards.Add(trashTargetCard);

                    if (selectedCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            new ITrashDigivolutionCards(card.PermanentOfThisCard(), selectedCards, activateClass).TrashDigivolutionCards());

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
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

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place to security.", "The opponent is selecting 1 Digimon to place to security.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                Permanent selectedPermanent = permanent;

                                if (selectedPermanent != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new IPutSecurityPermanent(
                                        selectedPermanent,
                                        CardEffectCommons.CardEffectHashtable(activateClass),
                                        toTop: true).PutSecurity());
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash face down source, Place 1 in security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("TrashEffect_BT22-076");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] By trashing this Digimon's bottom face-down digivolution card, place 1 Digimon with as much or less DP as this Digimon as the top security card.";
                }

                bool CanSelectTrashSourceCardCondition(CardSource cardSource)
                {
                    return cardSource.IsFlipped && !cardSource.CanNotTrashFromDigivolutionCards(activateClass);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent) &&
                           permanent.HasDP && permanent.DP <= card.PermanentOfThisCard().DP;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.PermanentOfThisCard().DigivolutionCards.Exists(CanSelectTrashSourceCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    CardSource trashTargetCard = card.PermanentOfThisCard().DigivolutionCards.Filter(CanSelectTrashSourceCardCondition)[^1];

                    selectedCards.Add(trashTargetCard);

                    if (selectedCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            new ITrashDigivolutionCards(card.PermanentOfThisCard(), selectedCards, activateClass).TrashDigivolutionCards());

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
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

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place to security.", "The opponent is selecting 1 Digimon to place to security.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                Permanent selectedPermanent = permanent;

                                if (selectedPermanent != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new IPutSecurityPermanent(
                                        selectedPermanent,
                                        CardEffectCommons.CardEffectHashtable(activateClass),
                                        toTop: true).PutSecurity());
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