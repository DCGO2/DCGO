using System.Collections;
using System.Collections.Generic;

// The Thunder Emperor Awakens
namespace DCGO.CardEffects.BT26
{
    public class BT26_097 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Use Cost +1 per security card
            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Use Cost +1 per security card", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(
                    changeCostFunc: ChangeCost,
                    cardSourceCondition: CardSourceCondition,
                    rootCondition: _ => true,
                    isUpDown: () => true,
                    isCheckAvailability: () => false,
                    isChangePayingCost: () => true);

                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable) => true;

                bool CardSourceCondition(CardSource cardSource) => cardSource == card;

                int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                    => CardSourceCondition(cardSource) ? cost + cardSource.Owner.SecurityCards.Count : cost;
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 1 [Dan Yuki]/[Kanan Yuki] Tamer under an [Aegiomon], it may digivolve into [Jupitermon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] By placing 1 of your Tamers with [Dan Yuki] or [Kanan Yuki] in their names as any of your [Aegiomon]'s bottom digivolution card, it may digivolve into [Jupitermon] in the hand or trash, ignoring digivolution requirements and without paying the cost. After, you may place 1 card with [Aegiochusmon] in its name in your trash as any of your [Jupitermon]'s top digivolution card.";

                bool CanSelectTamerCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && (permanent.TopCard.ContainsCardName("Dan Yuki") || permanent.TopCard.ContainsCardName("Kanan Yuki"));

                bool CanSelectAegiomonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && permanent.TopCard.EqualsCardName("Aegiomon");

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectJupitermonCondition(CardSource cardSource)
                    => cardSource.EqualsCardName("Jupitermon");

                bool CanSelectAegiochusmonCondition(CardSource cardSource)
                    => cardSource.ContainsCardName("Aegiochusmon");

                bool CanSelectJupitermonPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && permanent.TopCard.EqualsCardName("Jupitermon");

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTamerCondition)
                    && CardEffectCommons.HasMatchConditionPermanent(CanSelectAegiomonCondition))
                    {
                        Permanent selectedTamer = null;
                        Permanent selectedAegiomon = null;

                        SelectPermanentEffect selectTamerEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectTamerEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectTamerCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectTamerCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectTamerCoroutine(Permanent permanent)
                        {
                            selectedTamer = permanent;
                            yield return null;
                        }

                        selectTamerEffect.SetUpCustomMessage("Select 1 [Dan Yuki]/[Kanan Yuki] Tamer to place as an [Aegiomon]'s bottom digivolution card.", "The opponent is selecting 1 Tamer.");

                        yield return ContinuousController.instance.StartCoroutine(selectTamerEffect.Activate());

                        if (selectedTamer == null) yield break;

                        SelectPermanentEffect selectAegiomonEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectAegiomonEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectAegiomonCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectAegiomonCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectAegiomonCoroutine(Permanent permanent)
                        {
                            selectedAegiomon = permanent;
                            yield return null;
                        }

                        selectAegiomonEffect.SetUpCustomMessage("Select 1 [Aegiomon].", "The opponent is selecting 1 [Aegiomon].");

                        yield return ContinuousController.instance.StartCoroutine(selectAegiomonEffect.Activate());

                        if (selectedAegiomon == null) yield break;

                        yield return ContinuousController.instance.StartCoroutine(selectedAegiomon.AddDigivolutionCardsBottom(new List<CardSource>() { selectedTamer.TopCard }, activateClass, isFacedown: false));

                        bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectJupitermonCondition);
                        bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectJupitermonCondition);

                        if (canSelectHand || canSelectTrash)
                        {
                            bool isHand = canSelectHand;

                            if (canSelectHand && canSelectTrash)
                            {
                                List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                                {
                                    new(message: "From hand", value: 1, spriteIndex: 0),
                                    new(message: "From trash", value: 2, spriteIndex: 0),
                                    new(message: "Don't digivolve", value: 3, spriteIndex: 1),
                                };

                                GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: "From which area will [Jupitermon] digivolve?", notSelectPlayerMessage: "The opponent is choosing.");
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                                if (GManager.instance.userSelectionManager.SelectedIntValue == 3) yield break;

                                isHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                selectedAegiomon,
                                CanSelectJupitermonCondition,
                                payCost: false,
                                reduceCostTuple: null,
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: 1,
                                isHand: isHand,
                                activateClass: activateClass,
                                ignoreRequirements: CardEffectCommons.IgnoreRequirement.All,
                                successProcess: null));
                        }

                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectJupitermonPermanentCondition)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectAegiochusmonCondition))
                        {
                            CardSource selectedCard = null;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectAegiochusmonCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card with [Aegiochusmon] in its name to place as [Jupitermon]'s top digivolution card.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCard = cardSource;
                                yield return null;
                            }

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            if (selectedCard == null) yield break;

                            Permanent selectedJupitermon = null;

                            SelectPermanentEffect selectJupitermonEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectJupitermonEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectJupitermonPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectJupitermonCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            IEnumerator SelectJupitermonCoroutine(Permanent permanent)
                            {
                                selectedJupitermon = permanent;
                                yield return null;
                            }

                            selectJupitermonEffect.SetUpCustomMessage("Select 1 [Jupitermon].", "The opponent is selecting 1 [Jupitermon].");

                            yield return ContinuousController.instance.StartCoroutine(selectJupitermonEffect.Activate());

                            if (selectedCard != null && selectedJupitermon != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(selectedJupitermon.AddDigivolutionCardsTop(new List<CardSource>() { selectedCard }, activateClass));
                            }
                        }
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 play cost 5 or lower [TS] card from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Security] You may play 1 play cost 5 or lower [TS] trait card from your hand without paying the cost. Then, add this card to the hand.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.HasPlayCost && cardSource.GetCostItself <= 5 && cardSource.HasTSTraits
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, isPlayOption: true);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                            canTargetCondition: CanSelectCardCondition,
                            root: SelectCardEffect.Root.Hand,
                            cardEffect: activateClass,
                            payCost: false));
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
