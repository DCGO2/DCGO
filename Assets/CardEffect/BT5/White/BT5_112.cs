using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT5_112 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            DontBattleSecurityDigimonClass dontBattleSecurityDigimonClass = new DontBattleSecurityDigimonClass();
            dontBattleSecurityDigimonClass.SetUpICardEffect("Ignore Battle", CanUseCondition, card);
            dontBattleSecurityDigimonClass.SetUpDontBattleSecurityDigimonClass(CardSourceCondition: CardSourceCondition);
            dontBattleSecurityDigimonClass.SetIsSecurityEffect(true);
            dontBattleSecurityDigimonClass.SetNotShowUI(true);
            cardEffects.Add(dontBattleSecurityDigimonClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanUseIgnoreBattle(hashtable, card);
            }

            bool CardSourceCondition(CardSource cardSource)
            {
                return cardSource == card;
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Play this card without battling", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] Play this card without battling and without paying its memory cost.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }
            bool CanActivateCondition(Hashtable hashtable)
            {
                if (card.Owner.ExecutingCards.Contains(card))
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: card, payCost: false, cardEffect: activateClass))
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: new List<CardSource>() { card }, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Execution, activateETB: true));
            }
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Delete 1 Tamer", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Delete 1 of your opponent's Tamers.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (permanent != null)
                {
                    if (permanent.TopCard != null)
                    {
                        if (permanent.IsTamer)
                        {
                            if (permanent.TopCard.Owner == card.Owner.Enemy)
                            {
                                if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
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
                    if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
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
                        if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                        {
                            int maxCount = Math.Min(1, card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.OnDestroyedAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Delete 1 Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Deletion] Delete 1 of your opponent's Digimon.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnTrash(card))
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
                if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                {
                    int maxCount = Math.Min(1, card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
        }

        return cardEffects;
    }
}

