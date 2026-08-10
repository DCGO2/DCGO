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
            if (timing == EffectTiming.None)
            {
                bool CanUseEndOfAttackCondition(Hashtable hashtable, ActivateClass activateClass)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateEndOfAttackCondition(Hashtable hashtable, ActivateClass activateClass)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.PermanentOfThisCard().TopCard == card
                        && PermanentHasCorrectSources();//check this card is still top card and still has correct sources, so the card still has the copy effect
                }

                bool GainEffectCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.IsExistOnBattleArea(card)
                        && PermanentHasCorrectSources();
                }

                bool PermanentHasCorrectSources() => card.PermanentOfThisCard() != null && card.PermanentOfThisCard().DigivolutionCards.Any((cardSource) => cardSource.EqualsCardName("Phoenixmon") || cardSource.EqualsCardName("X Antibody"));

                bool cardSourceCondition(CardSource cardSource) => cardSource == card;

                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("Attach [End of Attack] to all this Digimon's [On Deletion]", GainEffectCondition, card);
                cardEffects.Add(addSkillClass);

                List<ICardEffect> GetEffects(CardSource sourceCard, List<ICardEffect> getCardEffects, EffectTiming _timing)
                {
                    getCardEffects ??= new List<ICardEffect>();

                    if (sourceCard == null || _timing is not EffectTiming.OnEndAttack)
                        return getCardEffects;

                    List<ActivateClass> onDeletionEffects = card.PermanentOfThisCard().EffectList(EffectTiming.OnDestroyedAnyone).Where(x => x.IsOnDeletion && !x.IsSecurityEffect).Cast<ActivateClass>().ToList();

                    foreach (ActivateClass activateClass in onDeletionEffects)
                    {
                        getCardEffects.Add(activateClass);

                        activateClass.SetOriginalEffectSourceCard(activateClass.EffectSourceCard);

                        //EOA version of effect is on Phoenixmon X but will need to check if the source card is still in the expected place to activate. Capturing this information when the class is created for reference in conditions below
                        bool wasInherited = activateClass.IsInheritedEffect;
                        bool wasLinked = activateClass.IsLinkedEffect;

                        activateClass.SetIsInheritedEffect(false);
                        activateClass.SetIsLinkedEffect(false);

                        activateClass.SetCanUseCondition(
                            hashtable => CanUseEndOfAttackCondition(hashtable, activateClass)
                        );

                        Permanent thisPermanent = card.PermanentOfThisCard();
                        CardSource originalCard = activateClass.OriginalEffectSourceCard;

                        activateClass.SetCanActivateCondition(
                            hashtable => {
                                if (CanActivateEndOfAttackCondition(hashtable, activateClass))
                                {
                                    //Check source card is still in position to activate
                                    if (wasInherited)
                                        return thisPermanent.DigivolutionCards.Contains(originalCard);
                                    else if (wasLinked)
                                        return thisPermanent.LinkedCards.Contains(originalCard);
                                    else
                                        return thisPermanent.TopCard == originalCard;
                                }
                                return false;
                            }
                        );

                        activateClass.SetHashString(GenerateHashString(card, activateClass.OriginalEffectSourceCard, activateClass.HashString));
                    }

                    return getCardEffects;
                }

                addSkillClass.SetUpAddSkillClass(
                    cardSourceCondition: cardSourceCondition,
                    getEffects: GetEffects,
                    limitTiming: EffectTiming.OnEndAttack //important as it also prevents stack overflow on this effect
                );

                string GenerateHashString(CardSource card, CardSource cardSource, string source)
                {
                    string sourceHashString = source ??= "";
                    return $"{card.CardIndex}-copying-{cardSource.CardIndex}-effect-{sourceHashString}";
                }
            }
            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 digimon, delete 1 digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Deletion] You may play 1 11000 DP or lower red Digimon card with [Avian], [Bird], [Beast], [Animal], or [Sovereign], other than [Sea Animal] in one of its traits from your hand without paying the cost. Delete 1 of your opponent's Digimon with as much or less DP as the Digimon this effect played.";
                }

                bool CanPlayTargetCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasDP
                        && cardSource.CardDP <= 11000
                        && cardSource.HasCardColor(CardColor.Red)
                        && cardSource.HasAvianBeastAnimalTraits
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card, activateClass);
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
                            return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                                && permanent.TopCard.HasDP
                                && permanent.DP <= cardToPlay.PermanentOfThisCard().DP;
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
