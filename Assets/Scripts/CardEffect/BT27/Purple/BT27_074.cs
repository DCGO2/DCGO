using System.Collections;
using System.Collections.Generic;

// Falcomon
namespace DCGO.CardEffects.BT27
{
    public class BT27_074 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - Lv.2 w/[DATA SQUAD] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasDataSquadTraits;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 2));
            }
            #endregion

            #region Shared When Moving / On Play
            string SharedEffectName = "By placing 1 hand card face down under a [Keenan Crier], <Draw 1> and trash 1 card in your hand";

            string SharedEffectDescription(string tag)
                => $"[{tag}] By placing 1 card from your hand face down under any of your [Keenan Crier]s, <Draw 1> and trash 1 card in your hand.";

            bool CanSelectKeenanCrierCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                    && permanent.TopCard.EqualsCardName("Keenan Crier");

            bool SharedCanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => card.Owner.HandCards.Count >= 1
                    && CardEffectCommons.HasMatchConditionPermanent(CanSelectKeenanCrierCondition);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Cost - Place 1 hand card face down under a [Keenan Crier]
                Permanent selectedTamer = null;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectKeenanCrierCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 [Keenan Crier] to place a card under.", "The opponent is selecting 1 [Keenan Crier] to place a card under.");

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    selectedTamer = permanent;
                    yield return null;
                }

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                if (selectedTamer == null) yield break;

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

                selectHandEffect.SetUpCustomMessage("Select 1 card to place face down under [Keenan Crier].", "The opponent is selecting 1 card to place face down.");

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCardToPlace = cardSource;
                    yield return null;
                }

                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                if (selectedCardToPlace == null) yield break;

                yield return ContinuousController.instance.StartCoroutine(selectedTamer.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCardToPlace }, activateClass, isFacedown: true));

                if (!selectedTamer.DigivolutionCards.Contains(selectedCardToPlace)) yield break;
                #endregion

                #region <Draw 1> and trash 1 card in your hand
                yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                if (card.Owner.HandCards.Count >= 1)
                {
                    SelectHandEffect selectDiscardEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectDiscardEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: _ => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectDiscardEffect.Activate());
                }
                #endregion
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                isSkippable: true,
                additionalActivateCondition: SharedCanActivateCondition,
                whenMoving: true,
                onPlay: true);
            #endregion

            #region When Attacking - OPT - ESS
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<Draw 1> and trash 1 card in your hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT27_074_Inherited");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] [Once Per Turn] <Draw 1> and trash 1 card in your hand.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                    if (card.Owner.HandCards.Count >= 1)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: _ => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
