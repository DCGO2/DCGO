using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Abbadomon Core
namespace DCGO.CardEffects.EX9
{
    public class EX9_057 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.EqualsCardName("Abbadomon"))
                    {
                        return true;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Collision, Piercing, Security Atk +1, Reboot, Blocker
            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.CollisionSelfStaticEffect(false, card, null));
            }

            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Breeding - Opponents Turn
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 4 Digi-Egg [Negamon] to Digi-Egg deck, to move to battle area.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateBreedingCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Breeding] [Opponent's Turn] When one of your opponent's Digimon attacks, by returning 4 [Negamon] from your trash or your Digimon's digivolution cards to the bottom of the Digi-Egg deck, this Digimon moves to the battle area.";
                }

                int NegamonCount()
                {
                    int value = 0;

                    value += CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CanSelectCardCondition);

                    foreach (Permanent perm in card.Owner.GetBattleAreaDigimons())
                    {
                        value += perm.DigivolutionCards.Count(CanSelectCardCondition);
                    }

                    return value;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsOpponentPermanent(permanent, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigiEgg &&
                           cardSource.EqualsCardName("Negamon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBreedingAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, PermanentCondition))
                            {
                                return (NegamonCount() >= 4);
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateBreedingCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBreedingAreaDigimon(card))
                    {
                        return card.PermanentOfThisCard().CanMove;
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    List<CardSource> negamonCards = new List<CardSource>();

                    negamonCards.AddRange(card.Owner.TrashCards.Where(CanSelectCardCondition));

                    foreach (Permanent perm in card.Owner.GetBattleAreaDigimons())
                    {
                        negamonCards.AddRange(perm.DigivolutionCards.Where(CanSelectCardCondition));
                    }

                    if (negamonCards.Count >= 4)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Will you return 4 [Negamon] back to Digi-Egg deck?";
                        string notSelectPlayerMessage = "The opponent is choosing whether or not to return 4 [Negamon].";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool willReturn = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (willReturn)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(negamonCards));

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(negamonCards, "Digi-Egg deck Bottom Cards", true, true));

                            if (card.Owner.CanMove)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.MovePermanent(card.Owner.GetBreedingAreaPermanents()[0].PermanentFrame));
                            }
                        }
                    }
                }
            }
            #endregion

            #region When Moving/When Digivolving/When Attacking Shared
            bool SourceCondition(CardSource source)
            {
                return source.IsDigimon &&
                       source.HasLevel && source.Level <= 6 &&
                       source.HasText("Negamon");
            }

            bool OpponentMinLevelPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                       CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                       CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, SourceCondition) >= 3;
            }

            #endregion

            #region When Moving
            if (timing == EffectTiming.OnMove)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 3 Digimon as sources, delete opponents lowest level digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsDigimonEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Moving] By placing 3 level 6 or lower Digimon cards with [Negamon] in their texts from your trash as this Digimon's top digivolution cards, delete all of your opponent's lowest level Digimon.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnMove(hashtable, PermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool sourcesAdded = false;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                    canTargetCondition: SourceCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    canNoSelect: () => true,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: AfterSelectCardCoroutine,
                    message: "Select cards to place as the top digivolution sources",
                    maxCount: 3,
                    canEndNotMax: false,
                    isShowOpponent: false,
                    mode: SelectCardEffect.Mode.Custom,
                    root: SelectCardEffect.Root.Trash,
                    customRootCardList: null,
                    canLookReverseCard: true,
                    selectPlayer: card.Owner,
                    cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count >= 3)
                        {
                            cardSources.Reverse();
                            yield return ContinuousController.instance.StartCoroutine(card.Owner.brainStormObject.CloseBrainstrorm(card));

                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsTop(cardSources, activateClass));

                            sourcesAdded = true;
                        }
                    }

                    if (sourcesAdded)
                    {
                        List<Permanent> destroyTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(OpponentMinLevelPermanentCondition);

                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                            destroyTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 3 Digimon as sources, delete opponents lowest level digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By placing 3 level 6 or lower Digimon cards with [Negamon] in their texts from your trash as this Digimon's top digivolution cards, delete all of your opponent's lowest level Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool sourcesAdded = false;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                    canTargetCondition: SourceCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    canNoSelect: () => true,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: AfterSelectCardCoroutine,
                    message: "Select cards to place as the top digivolution sources",
                    maxCount: 3,
                    canEndNotMax: false,
                    isShowOpponent: false,
                    mode: SelectCardEffect.Mode.Custom,
                    root: SelectCardEffect.Root.Trash,
                    customRootCardList: null,
                    canLookReverseCard: true,
                    selectPlayer: card.Owner,
                    cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count >= 3)
                        {
                            cardSources.Reverse();
                            yield return ContinuousController.instance.StartCoroutine(card.Owner.brainStormObject.CloseBrainstrorm(card));

                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsTop(cardSources, activateClass));

                            sourcesAdded = true;
                        }
                    }

                    if (sourcesAdded)
                    {
                        List<Permanent> destroyTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(OpponentMinLevelPermanentCondition);

                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                            destroyTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                    }
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 3 Digimon as sources, delete opponents lowest level digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] By placing 3 level 6 or lower Digimon cards with [Negamon] in their texts from your trash as this Digimon's top digivolution cards, delete all of your opponent's lowest level Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool sourcesAdded = false;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                    canTargetCondition: SourceCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    canNoSelect: () => true,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: AfterSelectCardCoroutine,
                    message: "Select cards to place as the top digivolution sources",
                    maxCount: 3,
                    canEndNotMax: false,
                    isShowOpponent: false,
                    mode: SelectCardEffect.Mode.Custom,
                    root: SelectCardEffect.Root.Trash,
                    customRootCardList: null,
                    canLookReverseCard: true,
                    selectPlayer: card.Owner,
                    cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count >= 3)
                        {
                            cardSources.Reverse();
                            yield return ContinuousController.instance.StartCoroutine(card.Owner.brainStormObject.CloseBrainstrorm(card));

                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsTop(cardSources, activateClass));

                            sourcesAdded = true;
                        }
                    }

                    if (sourcesAdded)
                    {
                        List<Permanent> destroyTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(OpponentMinLevelPermanentCondition);

                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                            destroyTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}