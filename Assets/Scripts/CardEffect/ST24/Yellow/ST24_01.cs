using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Koromon
namespace DCGO.CardEffects.ST24
{
    public class ST24_01 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Inherit

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [DATA SQUAD] for 2 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("ST24_01_WA");
                cardEffects.Add(activateClass);

                string EffectDescription() => "[When Attacking] [Once Per Turn] By trashing the bottom face-down card from under any of your Tamers, this Digimon may digivolve into a [DATA SQUAD] trait Digimon card in the hand with the cost reduced by 2.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanentCondition);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("DATA SQUAD");
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && permanent.IsTamer
                        && permanent.HasFaceDownDigivolutionCards;
                }

                bool CanSelectTrashSourceCardCondition(CardSource cardSource)
                {
                    return cardSource.IsFlipped 
                        && !cardSource.CanNotTrashFromDigivolutionCards(activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;

                    if (CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition) > 1)
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
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 tamer to trash a face down card from.", "The opponent is selecting 1 tamer to trash a face down card from.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }
                    }
                    else selectedPermanent = card.Owner.GetBattleAreaPermanents().FirstOrDefault();

                    List<CardSource> selectedCards = new List<CardSource>();

                    CardSource trashTargetCard = selectedPermanent.DigivolutionCards.Filter(CanSelectTrashSourceCardCondition)[^1];

                    selectedCards.Add(trashTargetCard);

                    yield return ContinuousController.instance.StartCoroutine(
                        new ITrashDigivolutionCards(selectedPermanent, selectedCards, activateClass).TrashDigivolutionCards());

                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: card.PermanentOfThisCard(),
                            cardCondition: CanSelectCardCondition,
                            payCost: true,
                            reduceCostTuple: (reduceCost: 2, reduceCostCardCondition: null),
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null));
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
