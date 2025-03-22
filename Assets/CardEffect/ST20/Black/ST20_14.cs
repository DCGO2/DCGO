using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//ST20 Our Courage United
namespace DCGO.CardEffects.ST20
{
    public class ST20_14 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region ignoring colours
            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);

                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionPermanent(permanent => 
                        CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && permanent.TopCard.EqualsTraits("ADVENTURE")
                        && (permanent.IsTamer || permanent.IsDigimon), true);
                }

                bool CardCondition(CardSource cardSource)
                {
                    return (cardSource == card);
                }
            }
            #endregion

            #region Main effect
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 2", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] <Draw 2>. (Draw 2 cards from your deck.) Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 2, activateClass).Draw());

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }
            #endregion

            #region delay
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 2", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When any of your level 5 or higher Digimon would leave the battle area, <Delay> (By trashing this card after the placing turn, activate the effect.)\r\n• You may play 1 level 5 or lower Digimon card with the [ADVENTURE] trait from your hand without paying the cost.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsOwnerPermanent(permanent, card))
                    {
                        return permanent.TopCard.Level >= 5;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, PermanentCondition))
                        {
                            if (CardEffectCommons.CanDeclareOptionDelayEffect(card))
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
                        if (card.PermanentOfThisCard().CanBeDestroyedBySkill(activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool deleted = false;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass,
                        successProcess: permanents => SuccessProcess(),
                        failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        deleted = true;

                        yield return null;
                    }

                    if (deleted)
                    {
                        List<Permanent> selectedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable);

                        bool CanPlayAdventureCondition(CardSource cardSource)
                        {
                            
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            {
                                return cardSource.IsDigimon &&
                                        cardSource.EqualsTraits("ADVENTURE") &&
                                        cardSource.Level <= 5;
                            }

                            return false;
                        }

                        if (selectedPermanents.Count > 0)
                        {
                            if (card.Owner.HandCards.Count(CanPlayAdventureCondition) >= 1)
                            {
                                List<CardSource> selectedCards = new List<CardSource>();

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCards.Add(cardSource);

                                    yield return null;
                                }
                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanPlayAdventureCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectHandEffect.SetUpCustomMessage(
                                    "Select 1 level 5 or lower [ADVENTURE] trait Digimon card to play.",
                                    "The opponent is selecting 1 level 5 or lower [ADVENTURE] trait Digimon card to play.");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: selectedCards,
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Hand,
                                    activateETB: true));
                            }
                        }
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaceSelfDelayOptionSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}