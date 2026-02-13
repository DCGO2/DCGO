using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Matadormon
namespace DCGO.CardEffects.BT23
{
    public class BT23_066 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.IsLevel4
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

            #region Scapegoat
            if(timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                string EffectDiscription()
                {
                    return "<Scapegoat> (When this Digimon would be deleted other than by your own effects, by deleting 1 of your other Digimon, prevent that deletion.)";
                }

                cardEffects.Add(CardEffectFactory.ScapegoatSelfEffect(isInheritedEffect: false, card: card, condition: null, effectName: "<Scapegoat>", effectDiscription: EffectDiscription()));
            }
            #endregion

            #region Shared OP/WD

            const string SharedEffectDescription = "Delete 1 of your opponent's level 4 or lower Digimon. Then, if digivolving from the trash, you may play 1 play cost 3 or lower card with the [Undead] or [CS] trait from your trash without paying the cost.";

            bool SharedCanSelectOpponentDigimonCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.Level <= 4)
                    {
                        if (permanent.TopCard.HasLevel)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool SharedCanSelectUndeadOrCSCardCondition(CardSource cardSource, ActivateClass activateClass)
            {
                // Is digimon or tamer?
                if (cardSource.IsDigimon || cardSource.IsTamer)
                {
                    // Is undead or CS?
                    if (cardSource.HasUndeadTraits || cardSource.HasCSTraits)
                    {
                        // Is 3-cost or lower?
                        if (cardSource.GetCostItself <= 3)
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            bool RootCondition(SelectCardEffect.Root root)
            {
                return root == SelectCardEffect.Root.Trash;
            }

            IEnumerator SharedActivateCoroutine(Hashtable _hashtable, ActivateClass activateClass)
            {
                // First, delete 1 opponent lvl 4 or lower digimon.
                if (CardEffectCommons.HasMatchConditionPermanent(SharedCanSelectOpponentDigimonCondition))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(SharedCanSelectOpponentDigimonCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: SharedCanSelectOpponentDigimonCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

                // Then, if digivolving from the trash, may play a 3-cost or lower [Undead] or [CS] card from trash
                if (CardEffectCommons.CanTriggerWhenDigivolving(_hashtable, card, RootCondition))
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => SharedCanSelectUndeadOrCSCardCondition(cardSource, activateClass)))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 1;

                        if (card.Owner.TrashCards.Count((cardSource) => SharedCanSelectUndeadOrCSCardCondition(cardSource, activateClass)) <= maxCount)
                        {
                            maxCount = card.Owner.TrashCards.Count((cardSource) => SharedCanSelectUndeadOrCSCardCondition(cardSource, activateClass));
                        }

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: (cardSource) => SharedCanSelectUndeadOrCSCardCondition(cardSource, activateClass),
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to play.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Trash,
                            activateETB: true));
                    }
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete a digimon, then play a digimon from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, (hashTable) => SharedActivateCoroutine(hashTable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] " + SharedEffectDescription;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion 

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete a digimon, then play a digimon from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, (hashTable) => SharedActivateCoroutine(hashTable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] " + SharedEffectDescription;
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
            }
            #endregion

            #region All Turns - ESS

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Protect others", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("AllTurn_BT23_066");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once per Turn] When any of your other Digimon would leave the battle area, by deleting this Digimon, they don't leave.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent != card.PermanentOfThisCard() &&
                           permanent.willBeRemoveField;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        Permanent thisCardPermanent = card.PermanentOfThisCard();

                        // If there are other digimon on field about to leave
                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                        {
                            List<Permanent> permanents = new List<Permanent>();

                            if (_hashtable.ContainsKey("Permanents"))
                            {
                                if (_hashtable["Permanents"] is List<Permanent>)
                                {
                                    permanents = (List<Permanent>)_hashtable["Permanents"];

                                    permanents = permanents.Filter(PermanentCondition);

                                    // Offer to delete this digimon
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                        targetPermanents: new List<Permanent>() { thisCardPermanent },
                                        activateClass: activateClass,
                                        successProcess: permanents => SuccessProcess(),
                                        failureProcess: null));
                                }
                            }

                            // If deletion was successful, protect each Digimon that was about to leave.
                            IEnumerator SuccessProcess()
                            {
                                foreach (Permanent permanent in permanents)
                                {
                                    permanent.willBeRemoveField = false;

                                    permanent.HideDeleteEffect();
                                    permanent.HideHandBounceEffect();
                                    permanent.HideDeckBounceEffect();
                                    permanent.HideWillRemoveFieldEffect();
                                }

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