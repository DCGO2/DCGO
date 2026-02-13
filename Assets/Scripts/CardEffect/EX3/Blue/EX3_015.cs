using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX3
{
    public class EX3_015 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Your 1 Digimon gets Jamming and a digivolution card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] 1 of your blue Digimon gains <Jamming> for the turn. (This Digimon can't be deleted in battles against Security Digimon.) When played from digivolution cards, you may place 1 blue level 5 or lower Digimon card from your hand under that Digimon as its bottom digivolution card.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.CardColors.Contains(CardColor.Blue))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardColors.Contains(CardColor.Blue))
                    {
                        if (cardSource.Level <= 5)
                        {
                            if (cardSource.HasLevel)
                            {
                                if (cardSource.IsDigimon)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        Permanent selectedPermanent = null;

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will gain Jamming.", "The opponent is selecting 1 Digimon that will gain Jamming.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainJamming(
                                targetPermanent: permanent,
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass));
                        }

                        if (CardEffectCommons.CanTriggerOnPlay(_hashtable, card, root => root == SelectCardEffect.Root.DigivolutionCards))
                        {
                            if (CardEffectCommons.IsFromDigimonDigivolutionCards(_hashtable))
                            {
                                if (selectedPermanent != null)
                                {
                                    if (!selectedPermanent.IsToken)
                                    {
                                        if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                                        {
                                            List<CardSource> selectedCards = new List<CardSource>();

                                            maxCount = 1;

                                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

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

                                            selectHandEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                                            selectHandEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                                            yield return StartCoroutine(selectHandEffect.Activate());

                                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                                            {
                                                selectedCards.Add(cardSource);

                                                yield return null;
                                            }

                                            if (selectedCards.Count >= 1)
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(selectedCards,
                                                activateClass));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}