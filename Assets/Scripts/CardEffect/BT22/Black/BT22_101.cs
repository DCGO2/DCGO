using System;
using System.Collections;
using System.Collections.Generic;

// Kyoko Kuremi
namespace DCGO.CardEffects.BT22
{
    public class BT22_101 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Turn

            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend tamer, add 1 [CS] digimon from trash to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When any of your level 4 or higher [CS] trait Digimon are deleted, by suspending this Tamer, return 1 [CS] trait Digimon card from your trash to the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, IsValidPermament);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool IsValidPermament(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.HasLevel && permanent.TopCard.Level >= 4 &&
                           permanent.TopCard.HasCSTraits;
                }

                bool isValidCard(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.HasCSTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, hashtable).Tap());

                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, isValidCard))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, isValidCard));

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                        selectCardEffect.SetUp(
                                    canTargetCondition: isValidCard,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 digimon to add to your hand.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.AddHand,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 [CS] digimon to add to hand", "The opponent is selecting 1 digimon to add to hand");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Selected digimon");
                        yield return StartCoroutine(selectCardEffect.Activate());
                    }
                }
            }

            #endregion

            #region Your Turn

            if (timing == EffectTiming.OnUnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [Alphamon] for 2 reducded cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT22_101_Digivolve");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] [Once Per Turn] When this Tamer unsuspends, if you don't have [Alphamon], this Tamer may digivolve into [Alphamon] in the hand with the digivolution cost reduced by 2.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentUnsuspends(hashtable, IsKyokoKuremi);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && !CardEffectCommons.HasMatchConditionOwnersPermanent(card, perm => IsAlphaMon(perm.TopCard))
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, IsAlphaMon);
                }

                bool IsKyokoKuremi(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && permanent == card.PermanentOfThisCard();
                }

                bool IsAlphaMon(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Alphamon");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: card.PermanentOfThisCard(),
                            cardCondition: IsAlphaMon,
                            payCost: true,
                            reduceCostTuple: (2, null),
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null));
                }
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}