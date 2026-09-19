using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Erika Mishima
namespace DCGO.CardEffects.BT23
{
    public class BT23_084 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                bool PermamentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasCSTraits;
                }

                bool Condition()
                {
                    return CardEffectCommons.IsOwnerTurn(card);
                }

                cardEffects.Add(CardEffectFactory.Gain1MemoryTamerOwnerDigimonConditionalEffect(
                    effectDescription: "[Start of Your Main Phase] If you have a Digimon with the [CS] trait, gain 1 memory.",
                    permamentCondition: PermamentCondition,
                    condition: Condition,
                    card: card));
            }

            #endregion

            #region End of your turn

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspend this tamer & bouncing 1 [Hudie] digimon to hand, play 1 level 3 [CS] digimon to empty breeding area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn] By suspending this Tamer and returning 1 of your Digimon with the [Hudie] trait to the hand, you may play 1 level 3 Digimon card with the [CS] trait from your hand to your empty breeding area without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsHudieDigimon);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool IsHudieDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasHudieTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsHudieDigimon))
                    {
                        Permanent selectedPermament = null;
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsHudieDigimon));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsHudieDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermament = permanent;
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedPermament != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.BouncePeremanentAndProcessAccordingToResult(
                                targetPermanents: new List<Permanent>() { selectedPermament },
                                activateClass: activateClass,
                                successProcess: SuccessProcess(),
                                failureProcess: null));

                            IEnumerator SuccessProcess()
                            {
                                if (!card.Owner.GetBreedingAreaPermanents().Any())
                                {
                                    bool CanSelectCardCondition(CardSource cardSource)
                                    {
                                        return cardSource.IsDigimon
                                            && cardSource.HasLevel && cardSource.IsLevel3
                                            && cardSource.HasCSTraits;
                                    }

                                    CardSource selectedCard = null;
                                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                                    int maxCount = Math.Min(1, card.Owner.HandCards.Count(CanSelectCardCondition));

                                    selectHandEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectCardCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: true,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        mode: SelectHandEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                                    selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                                    {
                                        selectedCard = cardSource;
                                        yield return null;
                                    }

                                    if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                        cardSources: new List<CardSource>() { selectedCard },
                                        activateClass: activateClass,
                                        payCost: false,
                                        isTapped: false,
                                        root: SelectCardEffect.Root.Hand,
                                        activateETB: true,
                                        isBreedingArea: true));
                                }
                            }
                        }
                    }

                }
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                bool IsHudieOrEaterLegionOrEaterEden()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                             (card.PermanentOfThisCard().TopCard.EqualsCardName("Hudiemon")
                           || card.PermanentOfThisCard().TopCard.EqualsCardName("Eater Legion")
                           || card.PermanentOfThisCard().TopCard.EqualsCardName("Eater EDEN"));
                }

                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: true, card: card,
                    condition: IsHudieOrEaterLegionOrEaterEden));
            }

            #endregion

            #region Blank ESS

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Placeholder to mark as having inheritable", _ => false, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, "No Effect");
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetIsBackgroundProcess(true);
                cardEffects.Add(activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return null;
                }
            }

            #endregion

            return cardEffects;
        }
    }
}