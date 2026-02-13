using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT11
{
    public class BT11_086 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                AddMaxTrashCountDigiXrosClass addMaxTrashCountDigiXrosClass = new AddMaxTrashCountDigiXrosClass();
                addMaxTrashCountDigiXrosClass.SetUpICardEffect($"Trash cards can be selected for DigiXros", CanUseCondition, card);
                addMaxTrashCountDigiXrosClass.SetUpAddMaxTrashCountDigiXrosClass(getMaxTrashCount: GetCount);
                addMaxTrashCountDigiXrosClass.SetNotShowUI(true);
                cardEffects.Add(addMaxTrashCountDigiXrosClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                int GetCount(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        return 100;
                    }

                    return 0;
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play level 4 or lower Digimon from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] You may play 1 purple level 4 or lower Digimon card or 1 level 4 or lower Digimon card with [Xros Heart] in its traits from your trash without paying the cost. If DigiXrosing, add 1 to the number of cards you play with this effect.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.Level <= 4)
                        {
                            if (cardSource.HasLevel)
                            {
                                if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                                {
                                    if (cardSource.CardColors.Contains(CardColor.Purple))
                                    {
                                        return true;
                                    }

                                    if (cardSource.CardTraits.Contains("Xros Heart"))
                                    {
                                        return true;
                                    }

                                    if (cardSource.CardTraits.Contains("XrosHeart"))
                                    {
                                        return true;
                                    }
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
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 1;

                        if (CardEffectCommons.IsDijiXros(_hashtable, (digixrosCount) => digixrosCount >= 1))
                        {
                            maxCount += 1;
                        }

                        maxCount = Math.Min(maxCount,
                            card.Owner.TrashCards.Count(CanSelectCardCondition));

                        maxCount = Math.Min(maxCount,
                            card.Owner.fieldCardFrames.Count((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame()));

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select cards to play.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select cards to play.", "The opponent is selecting cards to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Trash, activateETB: true));
                    }
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play level 4 or lower Digimon from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may play 1 purple level 4 or lower Digimon card or 1 level 4 or lower Digimon card with [Xros Heart] in its traits from your trash without paying the cost. If DigiXrosing, add 1 to the number of cards you play with this effect.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.Level <= 4)
                        {
                            if (cardSource.HasLevel)
                            {
                                if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                                {
                                    if (cardSource.CardColors.Contains(CardColor.Purple))
                                    {
                                        return true;
                                    }

                                    if (cardSource.CardTraits.Contains("Xros Heart"))
                                    {
                                        return true;
                                    }

                                    if (cardSource.CardTraits.Contains("XrosHeart"))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
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
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 1;

                        if (CardEffectCommons.IsDijiXros(_hashtable, (digixrosCount) => digixrosCount >= 1))
                        {
                            maxCount += 1;
                        }

                        maxCount = Math.Min(maxCount,
                            card.Owner.TrashCards.Count(CanSelectCardCondition));

                        maxCount = Math.Min(maxCount,
                            card.Owner.fieldCardFrames.Count((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame()));

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select cards to play.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select cards to play.", "The opponent is selecting cards to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Trash, activateETB: true));
                    }
                }
            }

            if (timing == EffectTiming.None)
            {
                bool CanUseCondition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.CardTraits.Contains("Xros Heart"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardTraits.Contains("XrosHeart"))
                        {
                            return true;
                        }

                        if (permanent.HasRetaliation)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.RushStaticEffect(
                    permanentCondition: PermanentCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition
                ));
            }

            if (timing == EffectTiming.None)
            {
                bool CanUseCondition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.CardTraits.Contains("Xros Heart"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardTraits.Contains("XrosHeart"))
                        {
                            return true;
                        }

                        if (permanent.HasRetaliation)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.BlockerStaticEffect(
                    permanentCondition: PermanentCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition));
            }

            if (timing == EffectTiming.None)
            {
                AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
                addDigiXrosConditionClass.SetUpICardEffect($"DigiXros", CanUseCondition, card);
                addDigiXrosConditionClass.SetUpAddDigiXrosConditionClass(getDigiXrosCondition: GetDigiXros);
                addDigiXrosConditionClass.SetNotShowUI(true);
                cardEffects.Add(addDigiXrosConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                DigiXrosCondition GetDigiXros(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        DigiXrosConditionElement element = new DigiXrosConditionElement(CanSelectCardCondition, "[Xros Heart] Digimon");

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                if (cardSource.Owner == card.Owner)
                                {
                                    if (cardSource.IsDigimon)
                                    {
                                        if (cardSource.CardTraits.Contains("Xros Heart"))
                                        {
                                            return true;
                                        }

                                        if (cardSource.CardTraits.Contains("XrosHeart"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>() { element };

                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, null, 3);

                        return digiXrosCondition;
                    }

                    return null;
                }
            }

            return cardEffects;
        }
    }
}