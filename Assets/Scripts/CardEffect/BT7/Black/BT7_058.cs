using System.Collections;
using System.Collections.Generic;

// SkullKnightmon
public class BT7_058 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        Permanent thisCardPermanent = card.PermanentOfThisCard();

        #region When Attacking
        if (timing == EffectTiming.OnAllyAttack)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Place 1 of your [DeadlyAxemon] under to digivolve into [DarkKnightmon] for free", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
            activateClass.SetIsSkippable(true);
            cardEffects.Add(activateClass);

            string EffectDescription()
            {
                return "[When Attacking] By placing 1 of your [DeadlyAxemon] as this Digimon's bottom digivolution card, this Digimon may digivolve into [DarkKnightmon] in the hand without paying the cost.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.EqualsCardName("DeadlyAxemon");
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.EqualsCardName("DarkKnightmon");
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                    && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                    && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                Permanent selectedPermanent = null;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place in digivolution cards.", "The opponent is selecting 1 Digimon to place in digivolution cards.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    selectedPermanent = permanent;

                    yield return null;
                }

                if (selectedPermanent != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(new List<Permanent[]>() { new Permanent[] { selectedPermanent, card.PermanentOfThisCard() } }, false, activateClass).PlacePermanentToDigivolutionCards());

                    if (thisCardPermanent.DigivolutionCards.Contains(selectedPermanent.TopCard))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: thisCardPermanent,
                            cardCondition: CanSelectCardCondition,
                            payCost: false,
                            reduceCostTuple: null,
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null));
                    }
                }
            }
        }
        #endregion

        #region Inherited
        if (timing == EffectTiming.None)
        {
            bool Condition()
            {
                return CardEffectCommons.IsExistOnBattleArea(card)
                    && CardEffectCommons.IsOwnerTurn(card)
                    && (thisCardPermanent.TopCard.ContainsCardName("Knightmon")
                        || thisCardPermanent.TopCard.ContainsCardName("Bagramon"));
            }

            cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: true, card: card, condition: Condition));
        }
        #endregion

        return cardEffects;
    }
}
