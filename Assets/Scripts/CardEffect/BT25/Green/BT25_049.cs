using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Armalizamon
namespace DCGO.CardEffects.BT25
{
    public class BT25_049 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasGlowingDawnTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 3, permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP / WD

            string SharedEffectName = "You may suspend 1 enemy Digimon";

            string SharedEffectDescription(string tag) => $"[{tag}] You may suspend 1 of your opponent's Digimon.";

            bool OpponentsDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    isSkippable: true,
                    onPlay: true,
                    whenDigivolving: true);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: OpponentsDigimon,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: null,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Tap,
                    cardEffect: activateClass);

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
            }
            #endregion

            #region Your Turn - Cost Reduction

            string CostReductionEffectName = "Trash 1 Face Down under a tamer to reduce cost by 3";

            bool TamerWithOneFaceDownSource(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.DigivolutionCards.Any(CanTrashCard);
            }

            bool CanTrashCard(CardSource cardSource) => cardSource.IsFaceDown;

            bool CardCondition(CardSource cardSource)
            {
                return cardSource.HasUseCost
                    && cardSource.HasGlowingDawnTraits
                    && cardSource.Owner == card.Owner;
            }

            int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
            {
                if (CardCondition(cardSource))
                {
                    if (RootCondition(root))
                    {
                        if (PermanentsCondition(targetPermanents))
                        {
                            Cost -= 3;
                        }
                    }
                }

                return Cost;
            }

            bool PermanentsCondition(List<Permanent> targetPermanents)
            {
                if (targetPermanents == null)
                {
                    return true;
                }
                else
                {
                    if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool RootCondition(SelectCardEffect.Root root)
            {
                return true;
            }

            #region Before Pay Cost - Condition Effect
            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(CostReductionEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("PlayCost-1_BT25_049");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Your Turn] [Once Per Turn] When you would use an Option card with the [Glowing Dawn] trait, by trashing the bottom face-down card under any of your Tamers, reduce the cost by 3.";
                
                bool CanNoSelect(Hashtable hashtable)
                {
                    CardSource cardSource = CardEffectCommons.GetCardFromHashtable(hashtable);

                    SelectCardEffect.Root root = SelectCardEffect.Root.None;
                    if (CardEffectCommons.IsExistOnHand(cardSource))
                        root = SelectCardEffect.Root.Hand;
                    else if (CardEffectCommons.IsExistInAnyTrash(cardSource))
                        root = SelectCardEffect.Root.Trash;
                    else if (CardEffectCommons.IsExistDigivolutionCards(cardSource))
                        root = SelectCardEffect.Root.DigivolutionCards;
                    else if (CardEffectCommons.IsExistLinked(cardSource))
                        root = SelectCardEffect.Root.LinkedCards;
                    
                    return cardSource != null
                        && cardSource.PayingCost(root, null, checkAvailability: false) > cardSource.Owner.MaxMemoryCost;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, TamerWithOneFaceDownSource);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool trash = false;

                    SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect1.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: TamerWithOneFaceDownSource,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: CanNoSelect(hashtable),
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect1.SetUpCustomMessage("Select 1 Tamer to trash 1 bottom face-down card from", "The opponent is selecting 1 Tamer to trash 1 bottom face-down card from");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanents[0], trashCount: 1, isFromTop: false, activateClass: activateClass, CanTrashCard));

                        trash = true;
                    }

                    if (trash)
                    {
                        if (card.Owner.CanReduceCost(null, card))
                        {
                            ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                        }

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect("Play Cost -1", CanUseCondition1, card);
                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                        card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                        bool CanUseCondition1(Hashtable _hashtable)
                        {
                            return true;
                        }

                        bool isUpDown()
                        {
                            return true;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(hashtable));
                    }
                    else activateClass.RemoveUse();
                }
            }
            #endregion

            #region Reduce Play Cost - Not Shown
            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -1", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => true, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, TamerWithOneFaceDownSource))
                    {
                        ICardEffect activateClass = card.EffectList(EffectTiming.BeforePayCost).Find(cardEffect => cardEffect.EffectName == CostReductionEffectName);

                        return activateClass != null
                            && !card.cEntity_EffectController.isOverMaxCountPerTurn(activateClass, activateClass.MaxCountPerTurn);
                    }

                    return false;
                }

                bool isUpDown()
                {
                    return true;
                }
            }
            #endregion

            #endregion

            #region Piercing - ESS
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
