using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT11
{
    public class BT11_042 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Add 1 card from security to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may search your security stack, reveal 1 card with [Angel], [Archangel], or [Fallen Angel] in its traits from it, and add it to your hand. If you added a card, <Recovery +1 (Deck)>. Then, shuffle your security stack.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("Angel"))
                    {
                        return true;
                    }

                    if (cardSource.CardTraits.Contains("Archangel"))
                    {
                        return true;
                    }

                    if (cardSource.CardTraits.Contains("Fallen Angel"))
                    {
                        return true;
                    }

                    if (cardSource.CardTraits.Contains("FallenAngel"))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.SecurityCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        int maxCount = Math.Min(1, card.Owner.SecurityCards.Count);

                        CardSource selectedCard = null;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            message: "Select 1 card to add to your hand.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.AddHand,
                            root: SelectCardEffect.Root.Security,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            foreach (CardSource cardSource in cardSources)
                            {
                                selectedCard = cardSource;
                            }

                            if (cardSources.Count >= 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                                    player: card.Owner,
                                    refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());
                            }

                            yield return null;
                        }

                        if (selectedCard != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                        }

                        ContinuousController.instance.PlaySE(GManager.instance.ShuffleSE);

                        card.Owner.SecurityCards = RandomUtility.ShuffledDeckCards(card.Owner.SecurityCards);
                    }
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("Memory+1_BT11_042");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn][Once Per Turn] When you play [LadyDevimon] or [Mirei Mikagura], gain 1 memory.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.CardNames.Contains("LadyDevimon"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardNames.Contains("Mirei Mikagura") || permanent.TopCard.CardNames.Contains("MireiMikagura"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            if (timing == EffectTiming.None)
            {
                bool CanUseCondition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent.TopCard.CardColors.Contains(CardColor.Purple)))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.CardTraits.Contains("Angel"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardTraits.Contains("Archangel"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardTraits.Contains("Fallen Angel"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardTraits.Contains("FallenAngel"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.BlockerStaticEffect(permanentCondition: PermanentCondition, isInheritedEffect: true, card: card, condition: CanUseCondition));
            }

            return cardEffects;
        }
    }
}