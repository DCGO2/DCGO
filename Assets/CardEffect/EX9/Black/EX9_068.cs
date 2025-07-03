using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX9
{
    public class EX9_068 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Turn
            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                List<Permanent> playedPermanent = new List<Permanent>();

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1 and gain 1 memory. Then place one digimon face down as a source.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()  
                {
                    return "[Your Turn] When any of your play cost 7 or higher [Cyborg], [Machine] or [DM] trait Digimon are played, by suspending this Tamer, <Draw 1> and gain 1 memory. Then, you may place 1 card from your hand face down as any of those Digimon's bottom digivolution card.";
                }

                bool EnterFieldPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.IsDigimon && permanent.TopCard.HasPlayCost && permanent.TopCard.GetCostItself >= 7 &&
                           (permanent.TopCard.EqualsTraits("Cyborg") || permanent.TopCard.EqualsTraits("Machine") || permanent.TopCard.EqualsTraits("DM")) &&
                           playedPermanent.Contains(permanent);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    List<Hashtable> hash = CardEffectCommons.GetHashtablesFromHashtable(hashtable);

                    hash.ForEach(x =>
                        playedPermanent.Add(CardEffectCommons.GetPermanentFromHashtable(x))
                    );

                    return CardEffectCommons.IsOwnerTurn(card) && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, EnterFieldPermanent);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return isExistOnField(card) && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    if (card.Owner.LibraryCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    }

                    if (card.Owner.CanAddMemory(activateClass))
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                    }

                    if (card.Owner.HandCards.Any())
                    {
                        CardSource selectedCard = null;
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: (cardSource) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: false,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);
                        yield return StartCoroutine(selectHandEffect.Activate());
                        selectHandEffect.SetUpCustomMessage("Select a card to add face down as a source", "Opponent is selecting a card to add face down as a source");

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        if (selectedCard != null)
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, EnterFieldPermanent))
                            {
                                Permanent selectedPermanent = null;
                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: EnterFieldPermanent,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass
                                );

                                selectPermanentEffect.SetUpCustomMessage("Select a Digimon to add source to", "Opponent is selecting a Digimon to add source to");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    selectedPermanent = permanent;
                                    yield return null;
                                }

                                if (selectedPermanent != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddExecutingCard(selectedCard));
                                    yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass, isFacedown: true));                               
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}