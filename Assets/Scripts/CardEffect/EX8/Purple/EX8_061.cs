using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX8
{
    public class EX8_061 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel4 && targetPermanent.TopCard.EqualsTraits("DS");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region Scapegoat
            
            if(timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {

                string EffectDiscription()
                {
                    return "<Scapegoat> (When this Digimon would be deleted other than by your own effects, by deleting 1 of your other Digimon, prevent that deletion.)";
                }

                cardEffects.Add(CardEffectFactory.ScapegoatSelfEffect(isInheritedEffect: false, card: card, condition: null, effectName: "<Scapegoat>", effectDiscription: EffectDiscription()));
            }
            
            #endregion

            #region When Attacking
            
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("You may play 1 level 4 or lower Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("PlayDigimon_EX8_061");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] [Once Per Turn] If you have 1 or more memory, you may play 1 level 4 or lower Digimon card with the [DS], [Mollusk], or [Crustacean] trait from your trash without paying the cost.";
                }

                bool DigimonToPlay(CardSource cardSource)
                {
                    return CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.Trash) &&
                           cardSource.IsDigimon &&
                           cardSource.HasLevel &&
                           cardSource.Level <= 4 &&
                           (cardSource.EqualsTraits("DS") || cardSource.EqualsTraits("Mollusk") || cardSource.EqualsTraits("Crustacean"));
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if(CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        return CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, DigimonToPlay) &&
                           card.Owner.MemoryForPlayer >= 1;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: DigimonToPlay,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
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

            #region On Deletion - ESS
            
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 level 4 or lower Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Deletion] You may play 1 level 4 or lower Digimon card with the [DS], [Mollusk], or [Crustacean] trait from your trash without paying the cost.";
                }

                bool HasCorrectTrait(CardSource cardSource)
                {
                    if (cardSource.EqualsTraits("DS") || cardSource.EqualsTraits("Mollusk") || cardSource.EqualsTraits("Crustacean"))
                    {
                        if (cardSource.HasLevel && cardSource.Level <= 4)
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            {
                                return true;
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
                    return CardEffectCommons.CanActivateOnDeletionInherited(hashtable, card) &&
                           CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, HasCorrectTrait);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: HasCorrectTrait,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
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
                    selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: selectedCards,
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Trash,
                        activateETB: true));
                }
            }
            
            #endregion
            
            return cardEffects;
        }
    }
}