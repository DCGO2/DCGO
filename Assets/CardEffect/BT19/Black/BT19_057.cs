using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT19
{
    public class BT19_057 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 2 &&
                           (targetPermanent.TopCard.EqualsTraits("Twilight") || targetPermanent.TopCard.EqualsTraits("Xros Heart"));
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition,
                    digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [RaptorSparrowmon] from under your Tamers", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Attacking] This Digimon may digivolve into [RaptorSparrowmon] under your Tamers without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool DigivolveCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.EqualsCardName("RaptorSparrowmon") &&
                           cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, false, activateClass);
                }

                bool TamerHasDigimonCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card) &&
                           permanent.IsTamer && permanent.DigivolutionCards.Count(DigivolveCardCondition) >= 1;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(TamerHasDigimonCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: TamerHasDigimonCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select a Tamer.", "The opponent is selecting a Tamer.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: DigivolveCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to digivolve into.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: selectedPermanent.DigivolutionCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to digivolve into.",
                            "The opponent is selecting 1 card to digivolve into.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Digivolve Card");

                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(new PlayCardClass(
                            cardSources: selectedCards,
                            hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                            payCost: false,
                            targetPermanent: card.PermanentOfThisCard(),
                            isTapped: false,
                            root: SelectCardEffect.Root.DigivolutionCards,
                            activateETB: true).PlayCard());
                    }
                }
            }

            #endregion

            #region On Deletion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.SaveEffect(card: card));
            }

            #endregion

            #region Your Turn - ESS

            if (timing == EffectTiming.OnCounterTiming)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) && CardEffectCommons.IsOwnerTurn(card) &&
                           card.PermanentOfThisCard().TopCard.EqualsTraits("Xros Heart");
                }

                cardEffects.Add(CardEffectFactory.CollisionSelfStaticEffect(isInheritedEffect: true, card: card, condition: Condition));
            }

            #endregion

            return cardEffects;
        }
    }
}