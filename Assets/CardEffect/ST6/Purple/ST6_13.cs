using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;


public class ST6_13 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
        }

        if (timing == EffectTiming.OnDeclaration)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Play 1 Digimon from trash", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] <Digi-Burst 2> (Trash 2 of this Digimon's Digivolution cards to activate the effect below.) - Play 1 purple level 3 Digimon card from your trash without paying its memory cost.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource != null)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.IsDigimon)
                            {
                                if (cardSource.CardColors.Contains(CardColor.Purple))
                                {
                                    if (cardSource.Level == 3)
                                    {
                                        if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                                        {
                                            if (cardSource.HasLevel)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (new IDigiBurst(card.PermanentOfThisCard(), 2, activateClass).CanDigiBurst())
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (isExistOnField(card))
                {
                    if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                    {
                        if (new IDigiBurst(card.PermanentOfThisCard(), 2, activateClass).CanDigiBurst())
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IDigiBurst(card.PermanentOfThisCard(), 2, activateClass).DigiBurst());

                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition(cardSource)))
                            {
                                int maxCount = Math.Min(1, card.Owner.TrashCards.Count((cardSource) => CanSelectCardCondition(cardSource)));

                                List<CardSource> selectedCards = new List<CardSource>();

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                            canTargetCondition: CanSelectCardCondition,
                                            canTargetCondition_ByPreSelecetedList: null,
                                            canEndSelectCondition: null,
                                            canNoSelect: () => false,
                                            selectCardCoroutine: SelectCardCoroutine,
                                            afterSelectCardCoroutine: null,
                                            message: "Select 1 card to play.",
                                            maxCount: maxCount,
                                            canEndNotMax: false,
                                            isShowOpponent: true,
                                            mode: SelectCardEffect.Mode.Custom,
                                            root: SelectCardEffect.Root.Trash,
                                            customRootCardList: null,
                                            canLookReverseCard: true,
                                            selectPlayer: card.Owner,
                                            cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");

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
                }
            }
        }

        return cardEffects;
    }
}
