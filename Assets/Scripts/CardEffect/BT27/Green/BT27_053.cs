using System.Collections;
using System.Collections.Generic;
using System.Linq;

// BanchoLeomon
namespace DCGO.CardEffects.BT27
{
    public class BT27_053 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.ContainsCardName("Leomon")
                        || targetPermanent.TopCard.HasDataSquadTraits;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Play Cost Reduction

            bool OpponentHas11000DPDigimon()
                => CardEffectCommons.HasMatchConditionOpponentsPermanent(card, permanent =>
                    CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.DP >= 11000);

            int ChangePlayCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
            {
                if (cardSource == card
                    && (targetPermanents == null || targetPermanents.Count(targetPermanent => targetPermanent != null) == 0))
                {
                    cost -= 6;
                }

                return cost;
            }

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reduce play cost (6)", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "When this card would be played, if your opponent has a Digimon with 11000 DP or more, reduce the play cost by 6.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, cardSource => cardSource == card);

                bool CanActivateCondition(Hashtable hashtable)
                    => OpponentHas11000DPDigimon();

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.CanReduceCost(null, card))
                    {
                        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                    }

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect("Play Cost -6", hashtable => true, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangePlayCost, cardSourceCondition: cardSource => cardSource == card, rootCondition: _ => true, isUpDown: () => true, isCheckAvailability: () => false, isChangePayingCost: () => true);
                    card.Owner.UntilCalculateFixedCostEffect.Add(_ => changeCostClass);

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));
                }
            }

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -6", hashtable => OpponentHas11000DPDigimon(), card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangePlayCost, cardSourceCondition: cardSource => cardSource == card, rootCondition: _ => true, isUpDown: () => true, isCheckAvailability: () => true, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);
            }

            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Fortitude
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.FortitudeSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared On Play / When Digivolving / When Attacking - OPT

            string SharedEffectName = "This Digimon may battle 1 opponent's Digimon";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] This Digimon may battle 1 of your opponent's Digimon.";

            bool IsDefenderCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            bool SharedCanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionPermanent(IsDefenderCondition);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent selectedDefender = null;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: IsDefenderCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectDefenderCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 of your opponent's Digimon to battle.", "The opponent is selecting 1 Digimon to battle.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectDefenderCoroutine(Permanent permanent)
                {
                    selectedDefender = permanent;
                    yield return null;
                }

                Permanent thisPermanent = card.PermanentOfThisCard();

                if (selectedDefender != null && thisPermanent != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IBattle(thisPermanent, selectedDefender, null, true).Battle());
                }
                else
                {
                    activateClass.RemoveUse();
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                isSkippable: true,
                additionalActivateCondition: SharedCanActivateCondition,
                maxCountPerTurn: 1,
                hashValue: "BT27_053_Battle",
                onPlay: true,
                whenDigivolving: true,
                whenAttacking: true);

            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May play or use 1 [Boss]/[DATA SQUAD] Tamer or Option from hand for free", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Deletion] You may play or use 1 Tamer or Option card with the [Boss] or [DATA SQUAD] trait from your hand without paying the cost.";

                bool CanSelectCardCondition(CardSource cardSource)
                    => (cardSource.IsTamer || cardSource.IsOption)
                        && (cardSource.EqualsTraits("Boss") || cardSource.HasDataSquadTraits)
                        && CardEffectCommons.CanPlayOrUse(cardSource, activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.CanActivateOnDeletion(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedCard = null;

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

                    selectHandEffect.SetUpCustomMessage("Select 1 card to play or use.", "The opponent is selecting 1 card to play or use.");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (selectedCard == null) yield break;

                    if (selectedCard.IsOption)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                            cardSources: new List<CardSource>() { selectedCard },
                            activateClass: activateClass,
                            payCost: false,
                            root: SelectCardEffect.Root.Hand));
                    }
                    else
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: new List<CardSource>() { selectedCard },
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Hand,
                            activateETB: true));
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
