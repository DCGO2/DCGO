using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCGO.CardEffects.BT19
{
    public class BT19_075 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Millenniummon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region On Play/When Digivolving Shared
            bool CardTrashed(CardSource source)
            {
                return true;
            }

            bool IsTamer(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card) &&
                       permanent.IsTamer;
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Your opponent trashes cards in their hand so that 5 remain", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Your opponent trashes cards in their hand so that 5 remain. For every 2 trashed by this effect, delete 1 of their Tamers.";
                }               

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> trashed = new List<CardSource>();

                    if(card.Owner.Enemy.HandCards.Count > 5)
                    {
                        int maxCount = card.Owner.Enemy.HandCards.Count - 5;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner.Enemy,
                            canTargetCondition: CardTrashed,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AllSelectedCards,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage($"Select {maxCount} card(s) to trash.", $"The opponent is selecting {maxCount} card(s) to trash.");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator AllSelectedCards(List<CardSource> sources)
                        {
                            trashed = sources;
                            yield return null;
                        }

                        if(trashed.Count >= 2)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(IsTamer))
                            {
                                int maxDeletes = Math.Min(Mathf.FloorToInt(trashed.Count/2), CardEffectCommons.MatchConditionPermanentCount(IsTamer));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: IsTamer,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxDeletes,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Destroy,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage($"Select {maxDeletes} Tamer(s) to delete.", $"The opponent is selecting {maxDeletes} Tamer(s) to delete.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                            }
                        }
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Your opponent trashes cards in their hand so that 5 remain", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Your opponent trashes cards in their hand so that 5 remain. For every 2 trashed by this effect, delete 1 of their Tamers.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> trashed = new List<CardSource>();

                    if (card.Owner.Enemy.HandCards.Count > 5)
                    {
                        int maxCount = card.Owner.Enemy.HandCards.Count - 5;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner.Enemy,
                            canTargetCondition: CardTrashed,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AllSelectedCards,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage($"Select {maxCount} card(s) to trash.", $"The opponent is selecting {maxCount} card(s) to trash.");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator AllSelectedCards(List<CardSource> sources)
                        {
                            trashed = sources;
                            yield return null;
                        }

                        if (trashed.Count >= 2)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(IsTamer))
                            {
                                int maxDeletes = Math.Min(Mathf.FloorToInt(trashed.Count / 2), CardEffectCommons.MatchConditionPermanentCount(IsTamer));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: IsTamer,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxDeletes,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Destroy,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage($"Select {maxDeletes} Tamer(s) to delete.", $"The opponent is selecting {maxDeletes} Tamer(s) to delete.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                            }
                        }
                    }
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete [Composite] trait digimon to prevent removal", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When this Digimon would leave the battle area, by deleting 1 of your Digimon with the [Composite] trait, it doesn't leave.";
                }

                bool HasCompositeTrait(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.EqualsTraits("Composite");                           
                           
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasCompositeTrait);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool deleted = false;
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    if (CardEffectCommons.HasMatchConditionPermanent(HasCompositeTrait))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: HasCompositeTrait,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectCompositeDigimon,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectCompositeDigimon(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                targetPermanents: new List<Permanent>() { permanent },
                                activateClass: activateClass,
                                successProcess: permanents => SuccessProcess(),
                                failureProcess: null));
                        }

                        IEnumerator SuccessProcess()
                        {
                            deleted = true;

                            yield return null;
                        }

                        if (deleted)
                        {
                            selectedPermanent.willBeRemoveField = false;

                            selectedPermanent.HideDeleteEffect();
                            selectedPermanent.HideHandBounceEffect();
                            selectedPermanent.HideDeckBounceEffect();
                            selectedPermanent.HideWillRemoveFieldEffect();
                        }
                    }
                    
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash your opponent's top security card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("TrashSecurity_BT19-075");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When other Digimon or Tamers are deleted, trash your opponent's top security card.";
                }

                bool OtherPermanentDeleted(Permanent permanent)
                {
                    return permanent.IsDigimon || permanent.IsTamer && 
                           permanent != card.PermanentOfThisCard();
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) && 
                           CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, OtherPermanentDeleted);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.Owner.Enemy.SecurityCards.Count > 0;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                    player: card.Owner.Enemy,
                    destroySecurityCount: 1,
                    cardEffect: activateClass,
                    fromTop: true).DestroySecurity());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}