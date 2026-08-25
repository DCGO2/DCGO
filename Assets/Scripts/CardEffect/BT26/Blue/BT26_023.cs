using System.Collections;
using System.Collections.Generic;

// Mojyamon
namespace DCGO.CardEffects.BT26
{
    public class BT26_023 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("DM");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 3));
            }
            #endregion

            #region Training
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.TrainingEffect(card));
            }
            #endregion

            #region Jamming
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared On Play / When Attacking

            string SharedEffectName()
                => "By placing 1 hand card as bottom digivolution card, bottom deck 1 opponent's level 4 or lower Digimon";

            string SharedEffectDescription(string tag)
                => $"[{tag}] By placing 1 card in your hand face down as this Digimon's bottom digivolution card, return 1 of your opponent's level 4 or lower Digimon to the bottom of the deck.";

            bool CanSelectDeckBottomTargetCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.HasLevel && permanent.TopCard.Level <= 4;

            bool SharedAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => card.Owner.HandCards.Count >= 1;

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                CardSource selectedCardToPlace = null;

                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: _ => true,
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

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCardToPlace = cardSource;
                    yield return null;
                }

                selectHandEffect.SetUpCustomMessage("Select 1 card to place face down as this Digimon's bottom digivolution card.", "The opponent is selecting 1 card to place face down as this Digimon's bottom digivolution card.");

                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                if (selectedCardToPlace != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { selectedCardToPlace }, activateClass, isFacedown: true));

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDeckBottomTargetCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDeckBottomTargetCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 level 4 or lower Digimon to return to the bottom of the deck.", "The opponent is selecting 1 level 4 or lower Digimon to return to the bottom of the deck.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                isSkippable: true,
                additionalActivateCondition: SharedAdditionalActivateCondition,
                onPlay: true,
                whenAttacking: true);

            #region Inherit
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] If your hand has 7 or fewer cards, <Draw 1>.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Count <= 7)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
