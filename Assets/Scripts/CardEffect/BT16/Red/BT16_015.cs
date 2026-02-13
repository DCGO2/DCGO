using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT16
{
    public class BT16_015 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("Phoenixmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            # region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                cardEffects.Add(CardEffectFactory.BlitzSelfEffect(isInheritedEffect: false, card: card, condition: null, isWhenDigivolving: true));
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.AfterEffectsActivate || timing == EffectTiming.OnStartTurn || timing == EffectTiming.OnEnterFieldAnyone || timing == EffectTiming.OnDigivolutionCardDiscarded)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Add [End of Attack] to all of this Digimon's [On Deletion] effects.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsBackgroundProcess(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] While [Phoenixmon] or [X Antibody] is in this Digimon's digivolution cards, attach [End of Attack] to all of this Digimon's [On Deletion] effects.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardNames.Contains("Phoenixmon") || cardSource.EqualsCardName("X Antibody")) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<ICardEffect> onDeletionEffects = card.PermanentOfThisCard().EffectList(EffectTiming.OnDestroyedAnyone).Where(x => x.IsOnDeletion && !x.IsSecurityEffect).ToList();
                    List<Func<EffectTiming, ICardEffect>> onEndAttackEffects = card.PermanentOfThisCard().UntilOwnerTurnEndEffects.Where(x => x(EffectTiming.OnEndAttack).HashString.StartsWith("EndOfAttack")).ToList();
                    List<ICardEffect> effectsToAdd = new List<ICardEffect>();

                    foreach (ICardEffect deletionEffect in onDeletionEffects)
                    {
                        List<bool> foundMatchingEffect = onEndAttackEffects.Select(x => x(EffectTiming.OnEndAttack).HashString.EndsWith(deletionEffect.EffectSourceCard.CardID)).ToList();

                        if (foundMatchingEffect.Count == 0)
                            effectsToAdd.Add(deletionEffect);
                    }

                    foreach (ICardEffect endOfAttackEffect in effectsToAdd)
                    {
                        ActivateClass activateEndofAttack = new ActivateClass();
                        activateEndofAttack.SetUpICardEffect(endOfAttackEffect.EffectName, CanUseEndOfAttackCondition, card);
                        activateEndofAttack.SetUpActivateClass(CanActivateEndOfAttackCondition, ActivateEndOfAttackCoroutine, endOfAttackEffect.MaxCountPerTurn, endOfAttackEffect.IsOptional, endOfAttackEffect.EffectDiscription);
                        activateEndofAttack.SetIsInheritedEffect(endOfAttackEffect.IsInheritedEffect);
                        activateEndofAttack.SetHashString($"EndOfAttack_{endOfAttackEffect.EffectSourceCard.CardID}");
                        activateEndofAttack.SetEffectSourceCard(endOfAttackEffect.EffectSourceCard);
                        activateEndofAttack.SetEffectSourcePermanent(card.PermanentOfThisCard());
                        activateEndofAttack.SetIsDigimonEffect(true);

                        bool CanUseEndOfAttackCondition(Hashtable hashtable1)
                        {
                            if (card.PermanentOfThisCard().TopCard != card)
                                return false;

                            if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardNames.Contains("Phoenixmon") || cardSource.EqualsCardName("X Antibody")) == 0)
                                return false;

                            return CardEffectCommons.CanTriggerOnEndAttack(hashtable1, card);
                        }

                        bool CanActivateEndOfAttackCondition(Hashtable hashtable1)
                        {
                            return CardEffectCommons.IsExistOnBattleArea(card);
                        }

                        IEnumerator ActivateEndOfAttackCoroutine(Hashtable hashtable)
                        {
                            yield return ContinuousController.instance.StartCoroutine(((ActivateICardEffect)endOfAttackEffect).Activate(hashtable));
                        }

                        CardEffectCommons.AddEffectToPermanent(
                                   targetPermanent: card.PermanentOfThisCard(),
                                   effectDuration: EffectDuration.UntilOwnerTurnEnd,
                                   card: card,
                                   cardEffect: activateEndofAttack,
                                   timing: EffectTiming.OnEndAttack);
                    }

                    yield return null;
                }
            }
            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 digimon, delete 1 digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] You may play 1 11000 DP or lower red Digimon card with [Avian], [Bird], [Beast], [Animal], or [Sovereign], other than [Sea Animal] in one of its traits from your hand without paying the cost. Delete 1 of your opponent's Digimon with as much or less DP as the Digimon this effect played.";
                }

                bool CanPlayTargetCondition(CardSource cardSource)
                {
                    if (cardSource.CardKind == CardKind.Digimon)
                    {
                        if (cardSource.HasDP && cardSource.CardDP <= 11000)
                        {
                            if (cardSource.CardColors.Contains(CardColor.Red))
                            {
                                if (cardSource.HasAvianBeastAnimalTraits)
                                {
                                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass))
                                        return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    CardSource cardToPlay = null;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanPlayTargetCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: false,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Custom,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        cardToPlay = cardSource;
                        yield return null;
                    }

                    if (cardToPlay != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: new List<CardSource> { cardToPlay },
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Hand,
                            activateETB: true));

                        if (CardEffectCommons.HasMatchConditionPermanent(CanDestroyTargetCondition))
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanDestroyTargetCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }

                        bool CanDestroyTargetCondition(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                            {
                                if (permanent.TopCard.HasDP && permanent.DP <= cardToPlay.PermanentOfThisCard().DP)
                                {
                                    return true;
                                }
                            }

                            return false;
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}