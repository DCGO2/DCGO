using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX7
{
    public class EX7_027 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Overclock

            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.OverclockSelfEffect(trait: "Puppet", isInheritedEffect: false, card: card,
                    condition: null));
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 level 3 [Puppet] Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] You may play 1 level 3 Digimon card with the [Puppet] trait from your hand without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool IsLevel3PuppetCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           cardSource.HasLevel &&
                           cardSource.IsLevel3 &&
                           cardSource.ContainsTraits("Puppet") &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionOwnersHand(card, IsLevel3PuppetCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, IsLevel3PuppetCardCondition))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsLevel3PuppetCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 Digimon card to play.",
                            "The opponent is selecting 1 Digimon card to play.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass,
                                payCost: false, isTapped: false, root: SelectCardEffect.Root.Hand, activateETB: true));
                    }
                }
            }

            #endregion

            #region All Turns - ESS

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 of your other Digimon to prevent this Digimon from leaving",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Substitute_EX7_027");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns][Once Per Turn] When this Digimon would leave the battle area other than by one of your effects, by deleting 1 of your Tokens or 1 of your other Digimon with the [Puppet] trait trait, prevent it from leaving.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent != card.PermanentOfThisCard() &&
                           (permanent.IsToken || permanent.TopCard.ContainsTraits("Puppet"));
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card) &&
                           !CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOwnerEffect(cardEffect, card));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        Permanent thisCardPermanent = card.PermanentOfThisCard();
                        
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.",
                            "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                    targetPermanents: new List<Permanent>() { permanent },
                                    activateClass: activateClass,
                                    successProcess: _ => SuccessProcess(),
                                    failureProcess: null));

                            IEnumerator SuccessProcess()
                            {
                                thisCardPermanent.willBeRemoveField = false;

                                thisCardPermanent.HideDeleteEffect();
                                thisCardPermanent.HideHandBounceEffect();
                                thisCardPermanent.HideDeckBounceEffect();
                                thisCardPermanent.HideWillRemoveFieldEffect();

                                yield return null;
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