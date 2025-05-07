using System.Collections;
using System.Collections.Generic;

//ST21 Metalgarurumon
namespace DCGO.CardEffects.ST21
{
    public class ST21_11 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution
            if(timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.Level == 5 && permanent.TopCard.HasAdventureTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, false, card, null));
            }
            #endregion

            #region Blast Digivolve
            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }
            #endregion

            #region On Play/When Digivolving shared

            bool CanActivateConditionShared(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentCondition);
            }

            bool CanGetCardColour(Permanent permanent) => permanent.IsTamer && permanent.TopCard.Owner == card.Owner;

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) && permanent.Level <= 4 + (CardEffectCommons.GetUniqueColourCountOnOwnerBattleArea(card, CanGetCardColour) / 2);
            }
            #endregion

            #region  On Play

            if(timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Bottom-deck level 4 + tamer colours/2", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] Return 1 of your opponent's level 4 or lower Digimon to the bottom of the deck. For every 2 colors your Tamers have, add 1 to this effect's level maximum.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectReturnEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectReturnEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                        cardEffect: activateClass);

                    selectReturnEffect.SetUpCustomMessage("Select 1 Digimon to return to bottom of deck.", "The opponent is selecting 1 Digimon to return to bottom of deck.");

                    yield return ContinuousController.instance.StartCoroutine(selectReturnEffect.Activate());
                }
            }

            #endregion

            #region  When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Bottom-deck level 4 + tamer colours/2", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] Return 1 of your opponent's level 4 or lower Digimon to the bottom of the deck. For every 2 colors your Tamers have, add 1 to this effect's level maximum.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectReturnEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectReturnEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                        cardEffect: activateClass);

                    selectReturnEffect.SetUpCustomMessage("Select 1 Digimon to return to bottom of deck.", "The opponent is selecting 1 Digimon to return to bottom of deck.");

                    yield return ContinuousController.instance.StartCoroutine(selectReturnEffect.Activate());
                }
            }

            #endregion

            #region When Attacking
            if(timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play level 4", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("WA_ST21-11");
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return "[When Attacking][Once Per Turn] You may play 1 level 4 or lower Digimon card from your trash without paying the cost.";
                }

                bool PlayableCardCondition(CardSource cardSource)
                {
                    return CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.Trash) 
                        && cardSource.IsDigimon && cardSource.Level <= 4;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnField(card) && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, PlayableCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: PlayableCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: ()=>true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: SelectCardCoroutine,
                        message: "Select 1 level 4 or lower Digimon card to play.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage(
                        "Select 1 level 4 or lower Digimon card to play.",
                        "The opponent is selecting 1 level 4 or lower Digimon card to play.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Play Card");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(List<CardSource> sources)
                    {
                        selectedCards = sources;
                        yield return null;
                    }

                    if (selectedCards.Count > 0)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: selectedCards,
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.Trash,
                                activateETB: true));
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}