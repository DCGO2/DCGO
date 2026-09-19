using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Takumi Aiba
namespace DCGO.CardEffects.BT23
{
    public class BT23_089 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] If your opponent has a Digimon, gain 1 memory.";
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
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.Enemy.GetBattleAreaDigimons().Count >= 1)
                        {
                            if (card.Owner.CanAddMemory(activateClass))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Prevent Digimon from leaving play", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("Substitute_BT23_089");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When any of your Digimon with the [CS] trait would leave the battle area, by suspending this Tamer and trash 2 same-level cards from 1 of your [CS] trait Digimon's digivolution cards, they don't leave.";
                }

                bool RemovedPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.HasCSTraits;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if(CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.HasCSTraits)
                        {
                            if(permanent.DigivolutionCards.Count(CanSelectCardCondition) >= 2)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (!cardSource.CanNotTrashFromDigivolutionCards(activateClass))
                    {
                        foreach (CardSource cardSource1 in cardSource.PermanentOfThisCard().DigivolutionCards)
                        {
                            if (cardSource != cardSource1)
                            {
                                if (cardSource.Level == cardSource1.Level)
                                {
                                    if (!cardSource1.CanNotTrashFromDigivolutionCards(activateClass))
                                    {
                                        if (cardSource.HasLevel && cardSource1.HasLevel)
                                        {
                                            return true;
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
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, RemovedPermanent);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.CanActivateSuspendCostEffect(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = null;

                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, _hashtable).Tap());

                    #region Select Permanent to trash from
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }
                    #endregion

                    if(selectedPermanent != null)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 2;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                    canEndSelectCondition: CanEndSelectCondition,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select cards to discard.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: selectedPermanent.DigivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetNotShowCard();
                        yield return StartCoroutine(selectCardEffect.Activate());

                        bool CanEndSelectCondition(List<CardSource> cardSources)
                        {
                            if (CardEffectCommons.HasNoElement(cardSources))
                            {
                                return false;
                            }

                            List<int> levels = cardSources
                            .Map(cardSource1 => cardSource1.Level)
                            .Distinct()
                            .ToList();

                            if (levels.Count > 1)
                            {
                                return false;
                            }

                            return true;
                        }

                        bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                        {
                            List<int> levels = cardSources
                            .Map(cardSource1 => cardSource1.Level)
                            .Concat(new List<int>() { cardSource.Level })
                            .Distinct()
                            .ToList();

                            if (levels.Count > 1)
                            {
                                return false;
                            }

                            return true;
                        }

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        if (selectedCards.Count == 2)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(
                            selectedPermanent,
                            selectedCards,
                            activateClass).TrashDigivolutionCards());

                            List<Permanent> protectedPermanents = CardEffectCommons.GetPermanentsFromHashtable(_hashtable)
                                    .Filter(RemovedPermanent);

                            foreach (Permanent removed in protectedPermanents)
                            {
                                removed.willBeRemoveField = false;
                                removed.HideDeleteEffect();
                                removed.HideHandBounceEffect();
                                removed.HideDeckBounceEffect();
                            }                            
                        }
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}