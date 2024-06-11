using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.BT16
{
    public class PhoenixmonXAntibody_BT16_015 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("Phoenixmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            # region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                cardEffects.Add(CardEffectFactory.BlitzSelfEffect(isInheritedEffect: false, card: card, condition: null, isWhenDigivolving: true));
            }
            #endregion

            #region Your Turn

            if (timing == EffectTiming.AfterEffectsActivate || timing == EffectTiming.OnStartTurn || timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Add [End of Attack] to all of this Digimon's [On Deletion] effects.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsBackgroundProcess(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] While [Phoenixmon] or [X Antibody] is in this Digimon's digivolution cards, attach [End of Attack] to all of this Digimon's [On Deletion] effects.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardNames.Contains("Phoenixmon") || cardSource.CardNames.Contains("X Antibody") || cardSource.CardNames.Contains("XAntibody") || cardSource.CardNames.Contains("X Antibody Proto Form")) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }


                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if(CardEffectCommons.IsOwnerTurn(card))
                    {
                        List<ICardEffect> onDeletionEffects = card.PermanentOfThisCard().EffectList(EffectTiming.OnDestroyedAnyone).Where(x => x.IsOnDeletion && !x.IsSecurityEffect).ToList();

                        foreach(Func<EffectTiming, ICardEffect> cardEffect in card.PermanentOfThisCard().UntilOwnerTurnEndEffects)
                        {
                            if(cardEffect(EffectTiming.OnEndAttack).HashString.Contains("EndOfAttack_BT16_015"))
                            {
                                card.PermanentOfThisCard().UntilOwnerTurnEndEffects.Remove(CardEffectCommons.GetCardEffectByEffectTiming(EffectTiming.OnEndAttack,cardEffect(EffectTiming.OnEndAttack)));
                            }
                        }
                       
                        foreach (ICardEffect cardEffect1 in onDeletionEffects)
                        {
                            ActivateClass activateEndofAttack = new ActivateClass();
                            activateEndofAttack.SetUpICardEffect(cardEffect1.EffectName, CanUseEndOfAttackCondition, card);
                            activateEndofAttack.SetUpActivateClass(CanActivateEndOfAttackCondition, ActivateEndOfAttackCoroutine, -1, false, cardEffect1.EffectDiscription);
                            activateEndofAttack.SetIsInheritedEffect(cardEffect1.IsInheritedEffect);
                            activateEndofAttack.SetHashString("EndOfAttack_BT16_015");
                            activateEndofAttack.SetEffectSourcePermanent(card.PermanentOfThisCard());

                            bool CanUseEndOfAttackCondition(Hashtable hashtable1)
                            {
                                if (card.PermanentOfThisCard().TopCard != card)
                                    return false;

                                return CardEffectCommons.CanTriggerOnEndAttack(hashtable1, card);
                            }

                            bool CanActivateEndOfAttackCondition(Hashtable hashtable1)
                            {
                                return CardEffectCommons.IsExistOnBattleArea(card);
                            }

                            IEnumerator ActivateEndOfAttackCoroutine(Hashtable hashtable)
                            {
                                yield return ContinuousController.instance.StartCoroutine(((ActivateICardEffect)cardEffect1).Activate(hashtable));
                            }

                            CardEffectCommons.AddEffectToPermanent(
                                       targetPermanent: card.PermanentOfThisCard(),
                                       effectDuration: EffectDuration.UntilOwnerTurnEnd,
                                       card: card,
                                       cardEffect: activateEndofAttack,
                                       timing: EffectTiming.OnEndAttack);

                        }
                    }
                    yield return null;
                }
            }

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetHashString("");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolveOfCard(hashtable, null, card))
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
                    foreach (Func<EffectTiming, ICardEffect> cardEffect in card.PermanentOfThisCard().UntilOwnerTurnEndEffects)
                    {
                        if (cardEffect(EffectTiming.OnEndAttack).HashString.Contains("EndOfAttack_BT16_015"))
                        {
                            card.PermanentOfThisCard().UntilOwnerTurnEndEffects.Remove(CardEffectCommons.GetCardEffectByEffectTiming(EffectTiming.OnEndAttack, cardEffect(EffectTiming.OnEndAttack)));
                        }
                    }

                    card.PermanentOfThisCard().UntilOwnerTurnEndEffects.Clear();
                    yield return null;
                }
            }
            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 digimon, delete 1 digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] You may play 1 11000 DP or lower red Digimon card with [Avian], [Bird], [Beast], [Animal], or [Sovereign], other than [Sea Animal] in one of its traits from your hand without paying the cost. Delete 1 of your opponent's Digimon with as much or less DP as the Digimon this effect played.";
                }

                bool CanPlayTargetCondition(CardSource cardSource)
                {
                    if (cardSource.CardKind == CardKind.Digimon)
                    {
                        if (cardSource.HasDP && cardSource.CardDP <= 11000)
                        {
                            if (cardSource.CardColors.Contains(CardColor.Red))
                            {
                                if (cardSource.HasAvianBeastAnimalTraits)
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
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    CardSource cardToPlay = null;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanPlayTargetCondition,
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

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        cardToPlay = cardSource;
                        yield return null;
                    }

                    if(cardToPlay != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: new List<CardSource> { cardToPlay },
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Hand, 
                            activateETB: true));

                        if (CardEffectCommons.HasMatchConditionPermanent(CanDestroyTargetCondition))
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanDestroyTargetCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                        
                        bool CanDestroyTargetCondition(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                            {
                                if(permanent.TopCard.HasDP && permanent.TopCard.CardDP <= cardToPlay.CardDP)
                                {
                                    return true;
                                }
                            }

                            return false;
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}