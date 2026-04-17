using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Wolvermon
namespace DCGO.CardEffects.ST23
{
    public class ST23_11 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Condition            
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Glowing Dawn");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 3, permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing the bottom face down card under your tamer, Reduce the digivolution cost by 2", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);


                string EffectDescription()
                {
                    return "[Your Turn] When this Digimon would digivolve into a [Glowing Dawn] trait Digimon card, by trashing the bottom face-down card from under any of your Tamers, reduce the cost by 2.";
                }

                bool PermanentCondition(Permanent permanent)
                {

                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent == card.PermanentOfThisCard();
                }

                bool HasProperTrait(CardSource source)
                {
                    return source.IsDigimon 
                        && source.EqualsTraits("Glowing Dawn");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(hashtable, PermanentCondition, HasProperTrait)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, ValidTamerCondition);
                }

                bool FaceDownCards(CardSource cardSource) => cardSource.IsFlipped;

                bool ValidTamerCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.DigivolutionCards.Any(FaceDownCards);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool trash = false;

                    SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect1.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: ValidTamerCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect1.SetUpCustomMessage("Select 1 Tamer to trash 1 bottom face-down card from", "The opponent is selecting 1 Tamer to trash 1 bottom face-down card from");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanents[0], trashCount: 1, isFromTop: false, activateClass: activateClass, FaceDownCards));

                        trash = true;
                    }

                    if (trash)
                    {
                        Hashtable hashtable = new Hashtable();
                        hashtable.Add("CardEffect", activateClass);

                        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect("Digivolution Cost -2", CanUseCondition1, card);
                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                        card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            return true;
                        }

                        int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                        {
                            if (CardSourceCondition(cardSource)
                                && RootCondition(root)
                                && PermanentsCondition(targetPermanents))
                            {
                                Cost -= 2;
                            }

                            return Cost;
                        }

                        bool PermanentsCondition(List<Permanent> targetPermanents)
                        {
                            return targetPermanents != null
                                && targetPermanents.Count(PermanentCondition) >= 1;
                        }

                        bool PermanentCondition(Permanent targetPermanent)
                        {
                            return targetPermanent.TopCard != null
                                && targetPermanent.TopCard.Owner == card.Owner
                                && targetPermanent.TopCard.Owner.GetBattleAreaPermanents().Contains(targetPermanent);
                        }

                        bool CardSourceCondition(CardSource cardSource)
                        {
                            return cardSource != null
                                && cardSource.Owner == card.Owner
                                && cardSource.EqualsTraits("Glowing Dawn");
                        }

                        bool RootCondition(SelectCardEffect.Root root)
                        {
                            return true;
                        }

                        bool isUpDown()
                        {
                            return true;
                        }

                    }
                }
            }
            #endregion

            #region Blocker - ESS
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
