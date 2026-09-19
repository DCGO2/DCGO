using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX11
{
    // Cendrillmon
    public class EX11_024 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alliance
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Overclock
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.OverclockSelfEffect(trait: "Puppet", isInheritedEffect: false, card: card,
                    condition: null));
            }
            #endregion
            #region Shared Functions

            bool SharedCanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

            bool IsOpponentDigimonCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            #endregion

            #region Shared OP / WD - Play Digimon

            string PlayDigimonEffectName = "May Play level 4 or lower [Puppet] Digimon. May play familiar token per Opponent's Digimon.";

            string PlayDigimonEffectDescription(string tag) => $"[{tag}] You may play 1 level 4 or lower [Puppet] trait Digimon card from your hand without paying the cost. Then, you may play 1 [Familiar] Token for each of your opponent's Digimon. (Digimon/Yellow/3000 DP/[On Deletion] 1 of your opponent's Digimon gets -3000 DP for the turn.)";

            bool PlayDigimonCardCondition(CardSource cardSource)
            {
                return cardSource.IsDigimon
                    && cardSource.EqualsTraits("Puppet")
                    && cardSource.HasLevel
                    && cardSource.Level <= 4;
            }

            IEnumerator PlayDigimonActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionOwnersHand(card, PlayDigimonCardCondition))
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: PlayDigimonCardCondition,
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

                    selectHandEffect.SetUpCustomMessage("Select 1 digimon to play", "The opponent is selecting 1 digimon to play");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource selectedCard)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            new List<CardSource>() { selectedCard },
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Hand,
                            activateETB: true));
                    }
                }

                List<SelectionElement<bool>> selectionElements1 = new List<SelectionElement<bool>>()
                {
                    new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                    new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                };

                string selectPlayerMessage1 = "Will you play Familiar Tokens?";
                string notSelectPlayerMessage1 = "The opponent is choosing if they will you play Familiar Tokens.";

                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage1, notSelectPlayerMessage: notSelectPlayerMessage1);

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                if(GManager.instance.userSelectionManager.SelectedBoolValue)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayFamiliarToken(
                            activateClass, 
                            CardEffectCommons.MatchConditionOpponentsPermanentCount(card, IsOpponentDigimonCondition)
                            ));
                }
            }

            #endregion

            #region On Play - Play Digimon
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(PlayDigimonEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => PlayDigimonActivateCoroutine(hash, activateClass), -1, false, PlayDigimonEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card) &&
                           CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }
            #endregion

            #region When Digivolving - Play Digimon
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(PlayDigimonEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => PlayDigimonActivateCoroutine(hash, activateClass), -1, false, PlayDigimonEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card) &&
                           CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }
            #endregion
            
            #region Shared WD / WA - Minus DP

            string MinusDPEffectName = "Give 1 opponent's digimon -3k DP for each of your digimon.";

            string MinusDPEffectDescription(string tag) => $"[{tag}] To 1 of your opponent's Digimon, give -3000 DP for the turn for each of your Digimon.";

            IEnumerator MinusDPActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsOpponentDigimonCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOpponentDigimonCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        int minusDP = -3000 * card.Owner.GetBattleAreaDigimons().Count;

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            targetPermanent: permanent,
                            changeValue: minusDP,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass));
                    }
                }
            }

            #endregion

            #region When Digivolving - Minus DP
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(MinusDPEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => MinusDPActivateCoroutine(hash, activateClass), -1, false, MinusDPEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card) &&
                           CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }
            #endregion

            #region When Attacking - Minus DP
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(MinusDPEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => MinusDPActivateCoroutine(hash, activateClass), -1, false, MinusDPEffectDescription("When Attacking"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card) &&
                           CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }
            #endregion

            return cardEffects;
        }
    }
}