using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT11
{
    public class BT11_016 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Activate 1 [On Deletion] effect", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Activate_BT11_016");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn][Once Per Turn] When a card is removed from your opponent's security stack, you may activate 1 of this Digimon's [On Deletion] effects.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, player => player == card.Owner.Enemy))
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
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        List<ICardEffect> candidateEffects = card.PermanentOfThisCard().EffectList(EffectTiming.OnDestroyedAnyone)
                                .Clone()
                                .Filter(cardEffect => cardEffect != null && cardEffect is ActivateICardEffect && !cardEffect.IsSecurityEffect && cardEffect.IsOnDeletion);

                        if (candidateEffects.Count >= 1)
                        {
                            ICardEffect selectedEffect = null;

                            if (candidateEffects.Count == 1)
                            {
                                selectedEffect = candidateEffects[0];
                            }
                            else
                            {
                                List<SkillInfo> skillInfos = candidateEffects
                                    .Map(cardEffect => new SkillInfo(cardEffect, null, EffectTiming.None));

                                List<CardSource> cardSources = candidateEffects
                                    .Map(cardEffect => cardEffect.EffectSourceCard);

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: (cardSource) => true,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: AfterSelectCoroutine,
                                    message: "Select 1 effect to activate.",
                                    maxCount: 1,
                                    canEndNotMax: false,
                                    isShowOpponent: false,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: cardSources,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetNotShowCard();
                                selectCardEffect.SetUpSkillInfos(skillInfos);

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                IEnumerator AfterSelectCoroutine(List<CardSource> selectedCards)
                                {
                                    if (selectedCards.Count > 0)
                                    {
                                        selectedEffect = candidateEffects.Where(cardEffect => cardEffect.EffectSourceCard == selectedCards[0]).ToList()[0];
                                        yield return null;
                                    }
                                }
                            }

                            if (selectedEffect != null)
                            {
                                Hashtable effectHashtable = CardEffectCommons.OnDeletionHashtable(
                                    new List<Permanent>() { card.PermanentOfThisCard() },
                                    selectedEffect,
                                    null,
                                    false);

                                bool NewCanUseCondition(Hashtable hashtable)
                                {
                                    return true;
                                }

                                Func<Hashtable, bool> OldCanUseCondition;

                                OldCanUseCondition = selectedEffect.CanUseCondition;

                                selectedEffect.SetCanUseCondition(NewCanUseCondition);

                                /*
                                card.Owner.TrashCards.Add(card);
                                card.Owner.TrashCards.Add(selectedEffect.EffectSourceCard);
                                selectedEffect.EffectSourceCard.PermanentJustBeforeRemoveField = card.PermanentOfThisCard();
                                card.PermanentJustBeforeRemoveField = card.PermanentOfThisCard();
                                bool canUse = selectedEffect.CanUse(effectHashtable);
                                card.Owner.TrashCards.Remove(card);
                                card.Owner.TrashCards.Remove(selectedEffect.EffectSourceCard);
                                selectedEffect.EffectSourceCard.PermanentJustBeforeRemoveField = null;
                                card.PermanentJustBeforeRemoveField = null;
                                */

                                yield return ContinuousController.instance.StartCoroutine(
                                    ((ActivateICardEffect)selectedEffect).Activate_Optional_Effect_Execute(effectHashtable));

                                selectedEffect.SetCanUseCondition(OldCanUseCondition);
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 red Digimon from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] You may play 1 red Digimon with [Avian], [Bird], [Beast], [Animal], or [Sovereign] in one of its traits and 3000 DP or less from your hand without paying the cost. For each red Tamer you have in play, add 2000 to the maximum DP of the card you can play by this effect.";
                }

                int maxDP()
                {
                    int maxDP = 3000;

                    maxDP += 2000 * card.Owner.GetBattleAreaPermanents().Count((permanent) => permanent.TopCard.CardColors.Contains(CardColor.Red) && permanent.IsTamer);

                    return maxDP;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardDP <= maxDP())
                    {
                        if (cardSource.CardColors.Contains(CardColor.Red))
                        {
                            if (cardSource.IsDigimon)
                            {
                                if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                                {
                                    if (cardSource.HasAvianBeastAnimalTraits)
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
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanActivateOnDeletion(card))
                    {
                        if (card.Owner.HandCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 1;

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

                        selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Hand, activateETB: true));
                    }
                }
            }

            return cardEffects;
        }
    }
}