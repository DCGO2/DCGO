using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT18
{
    public class BT18_079 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            // Any purple
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.IsTamer && targetPermanent.TopCard.CardColors.Contains(CardColor.Purple);
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false,
                    card: card, condition: null));
            }

            // Koichi Kimura
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Koichi Kimura");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false,
                    card: card, condition: null));
            }

            // Duskmon
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Duskmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false,
                    card: card, condition: null));
            }

            #endregion

            #region On Play/ When Digivolving Shared

            int OpponentColorCount()
            {
                List<CardColor> cardColors = new List<CardColor>();

                foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaPermanents()
                             .Where(permanent => permanent.IsDigimon || permanent.IsTamer))
                {
                    cardColors.AddRange(permanent.TopCard.CardColors);
                }

                cardColors = cardColors.Distinct().ToList();

                return cardColors.Count;
            }

            bool CanActivateConditionShared(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash both player's decks and gain +1000 DP for every card trashed", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Play] For each color among your opponent's Digimon and Tamers, trash the top card of both players decks. Then, this Digimon gets +1000 DP for each card trashed by this effect for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int trashCount = OpponentColorCount();

                    int ownerTrashed = Math.Min(card.Owner.LibraryCards.Count, trashCount);
                    int opponentTrashed = Math.Min(card.Owner.Enemy.LibraryCards.Count, trashCount);

                    yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(
                        trashCount,
                        card.Owner.Enemy, 
                        activateClass).AddTrashCardsFromLibraryTop());

                    yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(
                        trashCount,
                        card.Owner,
                        activateClass).AddTrashCardsFromLibraryTop());

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                        targetPermanent: card.PermanentOfThisCard(),
                        changeValue: 1000 * (ownerTrashed + opponentTrashed),
                        effectDuration: EffectDuration.UntilEachTurnEnd,
                        activateClass: activateClass));
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash both player's decks and gain +1000 DP for every card trashed", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] For each color among your opponent's Digimon and Tamers, trash the top card of both players decks. Then, this Digimon gets +1000 DP for each card trashed by this effect for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int trashCount = OpponentColorCount();

                    int ownerTrashed = Math.Min(card.Owner.LibraryCards.Count, trashCount);
                    int opponentTrashed = Math.Min(card.Owner.Enemy.LibraryCards.Count, trashCount);

                    yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(
                        trashCount,
                        card.Owner.Enemy,
                        activateClass).AddTrashCardsFromLibraryTop());

                    yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(
                        trashCount,
                        card.Owner,
                        activateClass).AddTrashCardsFromLibraryTop());

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                        targetPermanent: card.PermanentOfThisCard(),
                        changeValue: 1000 * (ownerTrashed + opponentTrashed),
                        effectDuration: EffectDuration.UntilEachTurnEnd,
                        activateClass: activateClass));
                }
            }

            #endregion

            #region End of Attack

            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 Digimon to delete all of your opponent's Digimon with the lowest level",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[End of Attack] By deleting 1 level 4 or lower purple Digimon, delete all of your opponent's Digimon with the lowest level.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnEndAttack(hashtable, card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent) &&
                           permanent.TopCard.HasLevel &&
                           permanent.Level <= 4 &&
                           permanent.TopCard.CardColors.Contains(CardColor.Purple);
                }
                
                bool OpponentMinLevelPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;

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
                        selectedPermanent = permanent;

                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                targetPermanents: new List<Permanent>() { selectedPermanent }, activateClass: activateClass,
                                successProcess: _ => SuccessProcess(), failureProcess: null));

                        IEnumerator SuccessProcess()
                        {
                            List<Permanent> destroyTargetPermanents =
                                card.Owner.Enemy.GetBattleAreaDigimons().Filter(OpponentMinLevelPermanentCondition);

                            yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                                destroyTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                        }
                    }
                }
            }

            #endregion

            #region Retaliation - ESS

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.RetaliationSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}