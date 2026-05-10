using System.Collections;
using System.Collections.Generic;

public class P_109 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        #region Alternative Digivolution Requirement
        if (timing == EffectTiming.None)
        {
            bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent.TopCard.CardNames.Contains("Paildramon")
                        || targetPermanent.TopCard.CardNames.Contains("Dinobeemon");
            }

            cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
        }
        #endregion

        #region Blast Digivolve
        if (timing == EffectTiming.OnCounterTiming)
        {
            cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
        }
        #endregion

        #region Shared OP/WD

        string SharedEffectName = "Suspend 1 Digimon and unsuspend 1 Digimon";

        CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

        string SharedEffectDescription(string tag) => $"[{tag}] Suspend 1 Digimon, then unsuspend 1 Digimon.";

        bool CanSelectPermanentCondition(Permanent permanent)
        {
            return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);
        }

        IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
        {        
            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
            {
                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: null,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Tap,
                    cardEffect: activateClass);
                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
            }

            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
            {
                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: null,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.UnTap,
                    cardEffect: activateClass);
                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
            }
        }
        #endregion

        #region All Turns
        if (timing == EffectTiming.OnTappedAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Play 1 Tamer or Digimon from hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
            activateClass.SetHashString("Unsuspen_EX3_074");
            cardEffects.Add(activateClass);

            string EffectDescription()
            {
                return "[All Turns] [Once Per Turn] When this Digimon becomes suspended, you may play a Tamer card or a Digimon card with 4000 DP or less from your hand without paying the cost.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.IsTamer
                        || (cardSource.IsDigimon
                    && cardSource.CardDP <= 4000)
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card)
                    && CardEffectCommons.CanTriggerWhenSelfPermanentSuspends(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card)
                    && card.Owner.HandCards.Count >= 1;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
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

                selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                yield return StartCoroutine(selectHandEffect.Activate());

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCards.Add(cardSource);

                    yield return null;
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Hand, activateETB: true));
            }
        }
        #endregion

        return cardEffects;
    }
}
