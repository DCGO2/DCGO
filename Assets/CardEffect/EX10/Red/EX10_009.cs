using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Creepymon
namespace DCGO.CardEffects.EX10
{
    public class EX10_009 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region WD/OD Shared

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                List<Permanent> permanents = card.Owner.Enemy.GetBattleAreaDigimons()
                    .Filter(perm => CardEffectCommons.IsMinDP(perm, card.Owner.Enemy)).ToList();

                bool hasNotDeleted = false;
                if (permanents.Any())
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                        targetPermanents: permanents,
                        activateClass: activateClass,
                        successProcess: null,
                        failureProcess: FailureProcess));

                    IEnumerator FailureProcess()
                    {
                        hasNotDeleted = true;
                        yield return null;
                    }
                }

                if (!permanents.Any() || hasNotDeleted)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(
                        addTrashCount: 5,
                        card.Owner.Enemy,
                        cardEffect: activateClass).AddTrashCardsFromLibraryTop());
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete all lowest DP digimom. if this didnt happen, trash their top 5 deck cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Delete all of your opponent's lowest DP Digimon. If this effect didn't delete, trash their deck's top 5 cards.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            #region On Deletion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete all lowest DP digimom. if this didnt happen, trash their top 5 deck cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] Delete all of your opponent's lowest DP Digimon. If this effect didn't delete, trash their deck's top 5 cards.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card);
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 red/purple digimon with 5k DP to breeding area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] If your opponent has 10 or more cards in their trash, you may play 1 red or purple 5000 DP or lower Digimon card from your trash to your empty breeding area without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.Owner.Enemy.TrashCards.Count >= 10
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition)
                        && !card.Owner.GetBreedingAreaPermanents().Any();
                }

                bool CanSelectCardCondition(CardSource card)
                {
                    return card.IsDigimon
                        && (card.CardColors.Contains(CardColor.Red) || card.CardColors.Contains(CardColor.Purple))
                        && card.BaseCardDP <= 5000
                        && CardEffectCommons.CanPlayAsNewPermanent(card, false, activateClass, isBreedingArea: true);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        CardSource selectedCard = null;

                        #region Select Trash Card

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CanSelectCardCondition));
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 digimon to place in breeding area.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        selectCardEffect.SetUpCustomMessage("Select 1 digimon to place in breeding area.", "The opponent is selecting 1 digimon to place in breeding area.");
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        #endregion

                        if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: new List<CardSource> { selectedCard },
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Trash,
                            activateETB: false,
                            isBreedingArea: true));
                    }
                }
            }

            #endregion

            #region End Of Your Turn

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Attack with this digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn] This Digimon may attack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && card.PermanentOfThisCard().CanAttack(activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.PermanentOfThisCard().CanAttack(activateClass))
                    {
                        SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                        selectAttackEffect.SetUp(
                            attacker: card.PermanentOfThisCard(),
                            canAttackPlayerCondition: () => true,
                            defenderCondition: (permanent) => true,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}