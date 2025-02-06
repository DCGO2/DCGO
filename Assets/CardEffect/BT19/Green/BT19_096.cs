using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT19
{
    public class BT19_096 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Option Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [Royal Base] faceup as bottom security, delete up to 8 play cost total", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] You may place 1 [Royal Base] trait Digimon card from your trash face up as your bottom security card. Then, delete up to 8 play cost's total worth of your opponent's Digimon. For each of your face up security cards, add 2 to the maximum play cost you may choose with this effect.";
                }

                int MaxCost()
                {
                    int maxCost = 8;

                    maxCost += card.Owner.SecurityCards.Count(source => !source.IsFlipped) * 2;

                    return maxCost;
                }

                bool IsRoyalBaseDigimon(CardSource card)
                {
                    return card.IsDigimon && 
                           card.EqualsTraits("Royal Base");
                }

                bool IsOpponenetsDigimon(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.GetCostItself <= MaxCost())
                        {
                            if (permanent.TopCard.HasPlayCost)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if(CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsRoyalBaseDigimon))
                    {
                        CardSource selectedCard = null;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: IsRoyalBaseDigimon,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 card to place as face up bottom security.",
                                    maxCount: 1,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;

                            yield return null;
                        }

                        if (selectedCard != null)
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(selectedCard, false, true));
                    }

                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsOpponenetsDigimon))
                    {
                        int maxCount = card.Owner.Enemy.GetBattleAreaPermanents().Count(IsOpponenetsDigimon);

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOpponenetsDigimon,
                            canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                            canEndSelectCondition: CanEndSelectCondition,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: true,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        bool CanEndSelectCondition(List<Permanent> permanents)
                        {
                            if (permanents.Count <= 0)
                            {
                                return false;
                            }

                            int sumCost = 0;

                            foreach (Permanent permanent1 in permanents)
                            {
                                sumCost += permanent1.TopCard.GetCostItself;
                            }

                            if (sumCost > MaxCost())
                            {
                                return false;
                            }

                            return true;
                        }

                        bool CanTargetCondition_ByPreSelecetedList(List<Permanent> permanents, Permanent permanent)
                        {
                            int sumCost = 0;

                            foreach (Permanent permanent1 in permanents)
                            {
                                sumCost += permanent1.TopCard.GetCostItself;
                            }

                            sumCost += permanent.TopCard.GetCostItself;

                            if (sumCost > MaxCost())
                            {
                                return false;
                            }

                            return true;
                        }
                    }
                }
            }
            #endregion

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects, effectName: $"Delete opponent's all Digimons with the highest play cost");
            }
            #endregion

            return cardEffects;
        }
    }
}