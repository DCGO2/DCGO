using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Siriusmon_LM_001 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        //Alternate Digivolution Requirement
        if (timing == EffectTiming.None)
        {
            static bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent.TopCard.HasText("Gammamon") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 5;
            }

            cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
        }

        //Blast Digivolve
        if (timing == EffectTiming.OnCounterTiming)
        {
            cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
        }

        #region OnPlay/WhenDigivolving

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Place a Card from hand to digivolution cards to get effects", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] [When Digivolving] You may place 1 card with [Gammamon] in its text from your hand as this Digimon's bottom digivolution card. Then, delete 1 of your opponent's Digimon with 8000 DP or less. For each color in this Digimon's digivolution cards, add 1000 to this DP deletion effect's maximum.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.HasText("Gammamon");
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnPlay(hashtable, card) || CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
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

                    selectHandEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(selectedCards, activateClass));

                        ActivateClass thenActivateClass = new ActivateClass();
                        thenActivateClass.SetUpICardEffect("Delete 1 Digimon with 8000 DP, +1000 DP for each color in sources", CanUseCondition, card);
                        thenActivateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                        cardEffects.Add(thenActivateClass);

                        int MaxDP()
                        {
                            int DP = 8000;

                            List<CardColor> cardColors = new List<CardColor>();

                            foreach (CardSource source in card.PermanentOfThisCard().cardSources)
                            {
                                foreach (CardColor cardColor in source.CardColors)
                                {
                                    if (!cardColors.Contains(cardColor))
                                        cardColors.Add(cardColor);
                                }
                            }

                            cardColors = cardColors.Distinct().ToList();

                            DP += cardColors.Count * 1000;

                            return DP;
                        }

                        bool CanSelectPermanentCondition(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                            {
                                if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(MaxDP(), thenActivateClass))
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
                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
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
                                int maxCount = System.Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

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
                                    cardEffect: thenActivateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region All Turns
        if (timing == EffectTiming.OnDestroyedAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Gain Memory", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[All Turns] [Once Per Turn] When another Digimon is deleted, gain 1 memory.";
            }

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                {
                    if (permanent.IsDigimon)
                    {
                        if (permanent != card.PermanentOfThisCard())
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition))
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
                        List<Hashtable> hashtables = CardEffectCommons.GetHashtablesFromHashtable(hashtable);

                        if (hashtables != null)
                        {
                            if (hashtables.Count >= 1)
                            {
                                return true;
                            }
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

        return cardEffects;
    }
}
