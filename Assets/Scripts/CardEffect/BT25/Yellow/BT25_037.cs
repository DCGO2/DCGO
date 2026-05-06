using System.Collections;
using System.Collections.Generic;

// Pegasusmon
namespace DCGO.CardEffects.BT25
{
    public class BT25_037 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolutions
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Patamon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 2, false, card, null, level: 3));
            }
            #endregion

            #region Armor Purge
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.ArmorPurgeEffect(card: card));
            }
            #endregion

            #region Shared OP / WD
            string SharedEffectName = "Add top sec to hand, then may place 1 traited card to top/bot sec";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    onPlay: true,
                    whenDigivolving: true);

            string SharedEffectDescription(string tag) => $"[{tag}] Add your top security card to the hand. Then, you may place 1 [Angel], [Archangel] [Three Great Angels] or [Iliad] trait Digimon card or 1 [TS] trait Tamer card from your hand as the top or bottom security card.";

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return (cardSource.IsDigimon
                    && (cardSource.EqualsTraits("Angel")
                        || cardSource.EqualsTraits("Archangel")
                        || cardSource.EqualsTraits("Three Great Angels")
                        || cardSource.EqualsTraits("Iliad")))
                        || (cardSource.IsTamer
                    && cardSource.HasTSTraits);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (card.Owner.SecurityCards.Count > 0)
                {
                    CardSource topCard = card.Owner.SecurityCards[0];

                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { topCard }, false, activateClass));

                    yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                        player: card.Owner,
                        refSkillInfos: ref ContinuousController.instance.nullSkillInfos,
                        activateClass).ReduceSecurity());
                }

                if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectCardCondition,
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

                    selectHandEffect.SetUpCustomMessage("Select 1 card to place at the top or bottom of security.", "The opponent is selecting 1 card to place at the top or bottom of security.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        if (cardSource != null)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Security Top", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Security Bottom", value : false, spriteIndex: 1),
                        };

                            string selectPlayerMessage = "Choose security position?";
                            string notSelectPlayerMessage = "The opponent is choosing security position";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                            bool position = GManager.instance.userSelectionManager.SelectedBoolValue;

                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(cardSource, toTop: position));
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
