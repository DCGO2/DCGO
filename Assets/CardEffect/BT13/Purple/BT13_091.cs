using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT13
{
    public class BT13_091 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete opponent's all level 5 or lower Digimons and this Digimon gets effects", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] Delete all of your opponent's level 5 or lower Digimon. Then, if you have 6 or fewer cards in your hand, this Digimon gets +3000 DP and gains <Security Attack +1> for the turn.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (permanent != null)
                    {
                        if (permanent.TopCard != null)
                        {
                            if (permanent.IsDigimon)
                            {
                                if (permanent.TopCard.Owner == card.Owner.Enemy)
                                {
                                    if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                    {
                                        if (permanent.Level <= 5)
                                        {
                                            if (permanent.TopCard.HasLevel)
                                            {
                                                if (permanent.CanBeDestroyedBySkill(activateClass))
                                                {
                                                    if (!permanent.TopCard.CanNotBeAffected(activateClass))
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
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                return true;
                            }

                            if (card.Owner.HandCards.Count <= 6)
                            {
                                return true;
                            }
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
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                List<Permanent> destroyedPermanetns = new List<Permanent>();

                                foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaDigimons())
                                {
                                    if (CanSelectPermanentCondition(permanent))
                                    {
                                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                        {
                                            destroyedPermanetns.Add(permanent);
                                        }
                                    }
                                }

                                if (destroyedPermanetns.Count >= 1)
                                {
                                    Hashtable hashtable = new Hashtable();
                                    hashtable.Add("CardEffect", activateClass);

                                    yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(destroyedPermanetns, hashtable).Destroy());
                                }
                            }

                            if (card.Owner.HandCards.Count <= 6)
                            {
                                if (isExistOnField(card))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: card.PermanentOfThisCard(), changeValue: 3000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(targetPermanent: card.PermanentOfThisCard(), changeValue: 1, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete your another Digimon to unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Unsuspend_BT13_091");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Attack][Once Per Turn] By deleting 1 of your other Digimon, unsuspend this Digimon.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent != card.PermanentOfThisCard())
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerOnAttack(hashtable, card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.CanAddMemory(activateClass))
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
                        int maxCount = Math.Min(1, card.Owner.GetBattleAreaDigimons().Count(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent thisCardPermanent = card.PermanentOfThisCard();

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { permanent }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                            IEnumerator SuccessProcess()
                            {
                                if (thisCardPermanent.TopCard != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { thisCardPermanent }, activateClass).Unsuspend());
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash the top card of this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Opponentf Turn] If this Digimon is [Belphemon: Sleep Mode], trash the top card of this Digimon. ";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            if (CardEffectCommons.IsOpponentTurn(card))
                            {
                                if (card.PermanentOfThisCard().DigivolutionCards.Count >= 1)
                                {
                                    if (card.PermanentOfThisCard().TopCard.CardNames.Contains("Belphemon: Sleep Mode"))
                                    {
                                        return true;
                                    }

                                    if (card.PermanentOfThisCard().TopCard.CardNames.Contains("Belphemon:SleepMode"))
                                    {
                                        return true;
                                    }
                                }
                            }
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
                            if (card.PermanentOfThisCard().DigivolutionCards.Count >= 1)
                            {
                                Permanent permanent = card.PermanentOfThisCard();

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(permanent));

                                CardSource cardSource = permanent.TopCard;

                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(cardSource));

                                if (!cardSource.IsToken)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(cardSource));
                                }

                                permanent.ShowingPermanentCard.ShowPermanentData(true);
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().RemoveDigivolveRootEffect(cardSource, permanent));
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}