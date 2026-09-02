using System.Collections;
using System.Collections.Generic;

// Shota Kuroi
namespace DCGO.CardEffects.BT26
{
    public class BT26_092 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 1 [TS] hand card, Draw 1 and gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Start of Your Main Phase] By trashing 1 [TS] trait card from your hand, <Draw 1> and gain 1 memory.";

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.HasTSTraits;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedCardToTrash = null;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectCardCondition,
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

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCardToTrash = cardSource;
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    if (selectedCardToTrash != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashHandAndProcessAccordingToResult(
                            player: card.Owner,
                            hashtable: hashtable,
                            cardToTrash: selectedCardToTrash,
                            activateClass: activateClass,
                            successProcess: SuccessProcess,
                            failureProcess: null));

                        IEnumerator SuccessProcess(CardSource cs)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                            if (card.Owner.CanAddMemory(activateClass))
                            {
                                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                            }
                        }
                    }
                }
            }
            #endregion

            #region Opponent's Turn - Redirect Attack
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By returning 1 [TS] Tamer to bottom of deck, redirect attack to 1 [TS] Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Opponent's Turn] When one of your opponent's Digimon attacks, by returning 1 of your [TS] trait Tamers to the bottom of the deck, you may change the attack target to 1 of your Digimon with the [TS] trait.";

                bool OpponentAttackerCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool CanSelectTamerCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.TopCard.HasTSTraits;

                bool YourDigimon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasTSTraits;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && !CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, OpponentAttackerCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedTamer = null;

                    SelectPermanentEffect selectTamerEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectTamerEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectTamerCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedTamer = permanent;
                        yield return null;
                    }

                    selectTamerEffect.SetUpCustomMessage("Select 1 [TS] trait Tamer to return to the bottom of the deck.", "The opponent is selecting 1 [TS] trait Tamer to return to the bottom of the deck.");

                    yield return ContinuousController.instance.StartCoroutine(selectTamerEffect.Activate());

                    if (selectedTamer != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(new List<Permanent>() { selectedTamer }, CardEffectCommons.CardEffectHashtable(activateClass)).DeckBounce());

                        if (CardEffectCommons.HasMatchConditionPermanent(YourDigimon))
                        {
                            Permanent selectedDefender = null;

                            SelectPermanentEffect selectDefenderEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectDefenderEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: YourDigimon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectDefenderCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectDefenderEffect.SetUpCustomMessage("Select 1 [TS] trait Digimon to change the attack target to.", "The opponent is selecting 1 [TS] trait Digimon to change the attack target to.");

                            yield return ContinuousController.instance.StartCoroutine(selectDefenderEffect.Activate());

                            IEnumerator SelectDefenderCoroutine(Permanent permanent)
                            {
                                selectedDefender = permanent;
                                yield return null;
                            }

                            if (selectedDefender != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SwitchDefender(
                                    activateClass,
                                    false,
                                    selectedDefender));
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

            return cardEffects;
        }
    }
}
