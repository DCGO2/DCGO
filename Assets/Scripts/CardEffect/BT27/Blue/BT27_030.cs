using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// MachGaogamon
namespace DCGO.CardEffects.BT27
{
    public class BT27_030 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - Lv.4 w/[DATA SQUAD] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasDataSquadTraits;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 4));
            }
            #endregion

            #region Shared OP/WD/WA
            string SharedEffectName = "If opponent has 6+ hand cards, they trash 1, then by trashing 2 Tamer face-down cards, may play or use 1 cost 5 or lower [DATA SQUAD] card from hand";

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: true,
                maxCountPerTurn: 1,
                hashValue: "BT27_030_OP_WD_WA",
                onPlay: true,
                whenDigivolving: true,
                whenAttacking: true);

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] You may return 1 of your opponent's level 4 or lower Digimon to the hand. Then, by trashing 2 bottom face-down cards from under any of your Tamers, you may play or use 1 [DATA SQUAD] trait card with a play or use cost of 5 or less from your hand without paying the cost.";

            bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;

            bool TamerWithOneFaceDownSource(Permanent permanent, ICardEffect activateClass)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.DigivolutionCards.Any(FaceDownCards)
                    && !permanent.ImmuneFromStackTrashing(activateClass);

            bool CanSelectCardCondition1(CardSource cardSource, ActivateClass activateClass)
                => cardSource.HasDataSquadTraits
                    && cardSource.GetCostItself <= 5
                    && CardEffectCommons.CanPlayOrUse(cardSource, activateClass);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool isUsed = false;

                bool CanSelectPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.HasLevel
                    && permanent.TopCard.Level <= 4;

                #region Bounce 1 of opponent's level 4 or lower Digimon to hand
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();
                    selectPermanentEffect1.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine1,
                        mode: SelectPermanentEffect.Mode.Bounce,
                        cardEffect: activateClass);

                    selectPermanentEffect1.SetUpCustomMessage("Select 1 opponent's Digimon to return to hand", "The opponent is selecting 1 Digimon to return to hand");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                    IEnumerator AfterSelectPermanentCoroutine1(List<Permanent> permanents)
                    {
                        if (permanents.Count > 0) isUsed = true;

                        yield return null;
                    }
                }
                #endregion

                #region By trashing 2 Tamer face-down cards, may play or use 1 [DATA SQUAD] card
                yield return StartCoroutine(CardEffectCommons.TrashTamerBottomFaceDownSources(card, activateClass, 2, successProcess: SuccessProcess()));

                IEnumerator SuccessProcess()
                {
                    isUsed = true;

                    CardSource selectedCard = null;

                    SelectHandEffect selectPlayEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectPlayEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: cardSource => CanSelectCardCondition1(cardSource, activateClass),
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: SelectCardCoroutine2,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPlayEffect.SetUpCustomMessage("Select 1 card to play/use.", "The opponent is selecting 1 card to play/use.");
                    selectPlayEffect.SetUpCustomMessage_ShowCard("Played Card");

                    IEnumerator SelectCardCoroutine2(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectPlayEffect.Activate());

                    if (selectedCard == null) yield break;

                    if (selectedCard.IsOption)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                            cardSources: new List<CardSource> { selectedCard },
                            activateClass: activateClass,
                            payCost: false,
                            root: SelectCardEffect.Root.Hand));
                    }
                    else
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: new List<CardSource> { selectedCard },
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Hand,
                            activateETB: true));
                    }
                }
                #endregion

                if (!isUsed) activateClass.RemoveUse();
            }
            #endregion

            #region Inherited
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash the bottom face-down card from 1 Tamer to not leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT25_027_Inherited");
                activateClass.SetIsSkippable(true);
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon with [Gaogamon] in its name or the [DATA SQUAD] trait would leave the battle area, by trashing the bottom face-down card from under any of your Tamers, it doesn't leave";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && (card.PermanentOfThisCard().TopCard.ContainsCardName("Gaogamon")
                            || card.PermanentOfThisCard().TopCard.EqualsTraits("DATA SQUAD"))
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionPermanent(permanent => TamerWithOneFaceDownSource(permanent, activateClass));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return StartCoroutine(CardEffectCommons.TrashTamerBottomFaceDownSources(card, activateClass, 1, successProcess: SuccessProcess(), failureProcess: FailureProcess()));

                    IEnumerator SuccessProcess()
                    {
                        Permanent thisCardPermanent = card.PermanentOfThisCard();

                        thisCardPermanent.willBeRemoveField = false;

                        thisCardPermanent.HideDeleteEffect();
                        thisCardPermanent.HideHandBounceEffect();
                        thisCardPermanent.HideDeckBounceEffect();
                        thisCardPermanent.HideWillRemoveFieldEffect();

                        yield return null;
                    }

                    IEnumerator FailureProcess()
                    {
                        activateClass.RemoveUse();
                        yield return null;
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
