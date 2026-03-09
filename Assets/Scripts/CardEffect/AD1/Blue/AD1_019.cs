using System;
using System.Collections;
using System.Collections.Generic;

// Matt Ishida & T.K. Takaishi (AD1-019)
namespace DCGO.CardEffects.AD1
{
    public class AD1_019 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                cardEffects.Add(CardEffectFactory.Gain1MemoryTamerOpponentDigimonEffect(card));
            }

            #endregion

            #region Your Turn

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When any of your Digimon digivolve into an [ADVENTURE] trait Digimon, by suspending this Tamer, you may play 1 [ADVENTURE] trait card from your hand. For every 2 of your Tamers' colors, reduce this effect's play cost by 1.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermamentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool PermamentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent)
                        && permanent.TopCard.HasAdventureTraits;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                        cardSource.HasAdventureTraits &&
                        CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                bool IsOwnerTamer(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleAreaTamer(permanent);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                        new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass,
                        SuccessProcess,
                        null));

                    IEnumerator SuccessProcess(List<Permanent> suspendedPermaments)
                    {
                        #region Select Hand Card

                        CardSource selectedCard = null;
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, CanSelectCardCondition));

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

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        selectHandEffect.SetUpCustomMessage("Select 1 [Adventure] digimon to play.", "The opponent is selecting 1 [Adventure] digimon to play");
                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                        #endregion

                        #region Get Cost Reduction By Tamer Colours

                        var tamerColours = CardEffectCommons.GetUniqueColourCountOnOwnerBattleArea(card, IsOwnerTamer);
                        var costReduction = tamerColours / 2;

                        #endregion

                        if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            new List<CardSource>() { selectedCard },
                            activateClass,
                            true,
                            false,
                            SelectCardEffect.Root.Hand,
                            true,
                            fixedCost: selectedCard.BasePlayCostFromEntity - costReduction));
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

            return cardEffects;
        }
    }
}