using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//WaruMonzaemon
namespace DCGO.CardEffects.P
{
    public class P_216 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Blocker

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play a Dark Master from Hand.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] You may play 1 [Dark Master] trait Digimon card from your hand without paying the cost. The Digimon this effect played can't digivolve and is deleted at turn end.";
                }

                bool CanSelectHandCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.EqualsTraits("Dark Masters")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectHandCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectCard = null;
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, CanSelectHandCondition));

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectHandCondition,
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

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectCard = cardSource;
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    if (selectCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: new List<CardSource> { selectCard },
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Security,
                            activateETB: true));

                        Permanent playedDigimon = null;

                        yield return new WaitForSeconds(0.2f);

                        playedDigimon = selectCard.PermanentOfThisCard();

                        #region Can't Digivolve
                        
                        CanNotDigivolveClass canNotEvolveClass = new CanNotDigivolveClass();
                        canNotEvolveClass.SetUpICardEffect("Can't digivolve", CanUseCantEvoCondition, card);
                        canNotEvolveClass.SetUpCanNotEvolveClass(permanentCondition: PermanentCondition, cardCondition: CardCondition);
                        playedDigimon.PermanentEffects.Add((_timing) => canNotEvolveClass);

                        bool CanUseCantEvoCondition(Hashtable hashtable)
                        {
                            return CardEffectCommons.IsPermanentExistsOnBattleArea(playedDigimon);
                        }

                        bool PermanentCondition(Permanent permanent)
                        {
                            return permanent == playedDigimon;
                        }

                        bool CardCondition(CardSource cardSource)
                        {
                            return true;
                        }
                        
                        #endregion
                        
                        #region Delete Played Digimon
                        Permanent selectedPermanent = selectCard.PermanentOfThisCard();

                        ActivateClass activateClass1 = new ActivateClass();
                        activateClass1.SetUpICardEffect("Delete this Digimon", CanUseCondition2, card);
                        activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, "");
                        activateClass1.SetEffectSourcePermanent(selectedPermanent);
                        CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilEachTurnEnd, card: card, cardEffect: activateClass1, timing: EffectTiming.OnEndTurn);

                        bool CanUseCondition2(Hashtable hashtable)
                        {
                            return true;
                        }

                        bool CanActivateCondition1(Hashtable hashtable)
                        {
                            return selectedPermanent.TopCard != null
                                && selectedPermanent.CanBeDestroyedBySkill(activateClass1)
                                && !selectedPermanent.TopCard.CanNotBeAffected(activateClass1);
                        }

                        IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(new List<Permanent>() { selectedPermanent }, CardEffectCommons.CardEffectHashtable(activateClass1)).Destroy());
                        }
                        #endregion
                    }
                }
            }

            #endregion

            #region On Deletion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play a Dark Master from Security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] You may play 1 face-up [Dark Masters] trait Digimon card from your security stack without paying the cost. At the end of your turn, delete the Digimon this effect played.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && !cardSource.IsFlipped &&
                           cardSource.EqualsTraits("Dark Masters") &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.Security);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card)
                        && card.Owner.SecurityCards.Any(CanSelectCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 card to play.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Security,
                        customRootCardList: null,
                        canLookReverseCard: false,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    if (selectedCards.Count > 0)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Security,
                            activateETB: true));

                        #region Delete Played Digimon
                        Permanent selectedPermanent = selectedCards[0].PermanentOfThisCard();

                        ActivateClass activateClass1 = new ActivateClass();
                        activateClass1.SetUpICardEffect("Delete this Digimon", CanUseCondition2, card);
                        activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, "");
                        activateClass1.SetEffectSourcePermanent(selectedPermanent);
                        CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilOwnerTurnEnd, card: card, cardEffect: activateClass1, timing: EffectTiming.OnEndTurn);

                        bool CanUseCondition2(Hashtable hashtable)
                        {
                            return CardEffectCommons.IsOwnerTurn(card);
                        }

                        bool CanActivateCondition1(Hashtable hashtable)
                        {
                            return selectedPermanent.TopCard != null
                                && selectedPermanent.CanBeDestroyedBySkill(activateClass1)
                                && !selectedPermanent.TopCard.CanNotBeAffected(activateClass1);
                        }

                        IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(new List<Permanent>() { selectedPermanent }, CardEffectCommons.CardEffectHashtable(activateClass1)).Destroy());
                        }
                        #endregion
                    }
                }
            }

            #endregion

            #region Blocker - ESS

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: true, card: card, condition: null));
            }

            #endregion
            return cardEffects;
        }
    }
}
