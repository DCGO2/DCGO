using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Beowolfmon
namespace DCGO.CardEffects.AD1
{
    public class AD1_015 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Koji Minamoto")
                        && targetPermanent.DigivolutionCards.Count(cardSource => cardSource.EqualsTraits("Hybrid")) >= 2;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }
            #endregion

            #region Jamming
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region DPReduction WD / WA / WA ESS

            string DPReductionEffectName = "Give 1 Opponent's digimon -4k DP";

            string DPReductionEffectDescription(string tag) => $"[{tag}] 1 of your opponent's Digimon gets -4000 DP for the turn.";

            bool DPReductionCanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleArea(card);

            bool IsEnemyDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            IEnumerator DPReductionActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(IsEnemyDigimon))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsEnemyDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -4000.", "The opponent is selecting 1 Digimon that will get DP -4000.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: -4000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(DPReductionEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(DPReductionCanActivateCondition, hash => DPReductionActivateCoroutine(hash, activateClass), -1, false, DPReductionEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(DPReductionEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(DPReductionCanActivateCondition, hash => DPReductionActivateCoroutine(hash, activateClass), -1, false, DPReductionEffectDescription("When Attacking"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
            }
            #endregion

            #region When Attacking - ESS
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(DPReductionEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(DPReductionCanActivateCondition, hash => DPReductionActivateCoroutine(hash, activateClass), -1, false, DPReductionEffectDescription("When Attacking"));
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
            }
            #endregion

            #region PlayTamer EoA / OD

            string PlayTamerEffectName = "You may play 1 Yellow / Black / Purple Tamer with inherited effects, then by placing a hybrid card under a tamer: draw 2";

            string PlayTamerEffectDescription(string tag) => $"[{tag}] You may play 1 yellow, black or purple Tamer card with inherited effects from your hand or trash without paying the cost. Then, by placing 1 [Hybrid] or [Ten Warriors] trait card from your hand under this Digimon or your Tamers, <Draw 2>.";

            bool CanPlayTamerCondition(CardSource cardSource)
            {
                return cardSource.IsTamer
                    && (cardSource.CardColors.Contains(CardColor.Yellow)
                        || cardSource.CardColors.Contains(CardColor.Black)
                        || cardSource.CardColors.Contains(CardColor.Purple))
                    && cardSource.HasInheritedEffect;
            }

            bool CanPlaceCardCondition(CardSource cardSource) => cardSource.HasHybridTenWarriorsTraits;

            bool CanPlaceUnderTamerCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                    && permanent.IsTamer;
            }

            bool PlayTamerCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card)
                    && (CardEffectCommons.HasMatchConditionOwnersHand(card, CanPlayTamerCondition)
                        || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanPlayTamerCondition)
                        || CardEffectCommons.HasMatchConditionOwnersHand(card, CanPlaceCardCondition));
            }

            IEnumerator PlayTamerActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool validHandCard = CardEffectCommons.HasMatchConditionOwnersHand(card, CanPlayTamerCondition);
                bool validTrashCard = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanPlayTamerCondition);

                if (validHandCard || validTrashCard)
                {
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                    if (validHandCard)
                    {
                        selectionElements.Add(new (message: $"Play Tamer from hand", value : 1, spriteIndex: 0));
                    }
                    if (validTrashCard)
                    {
                        selectionElements.Add(new (message: $"Play Tamer from trash", value : 2, spriteIndex: 0));
                    };
                    selectionElements.Add( new (message: $"Don't play", value: 3, spriteIndex: 1));

                    string selectPlayerMessage = "From where will you play a Tamer?";
                    string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool doPlay = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                    bool fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;

                    if (doPlay)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        if (fromHand)
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanPlayTamerCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.PlayForFree,
                                cardEffect: activateClass);

                            yield return StartCoroutine(selectHandEffect.Activate());
                        }
                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanPlayTamerCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to play.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.PlayForFree,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            yield return StartCoroutine(selectCardEffect.Activate());
                        }
                    }
                }

                if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanPlaceCardCondition))
                {
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                    selectionElements.Add(new (message: $"Place under this Digimon", value : 1, spriteIndex: 0));
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanPlaceUnderTamerCondition))
                    {
                        selectionElements.Add(new (message: $"Place under a Tamer", value : 2, spriteIndex: 0));
                    };
                    selectionElements.Add( new (message: $"Don't place", value: 3, spriteIndex: 1));

                    string selectPlayerMessage = "Will you place a card under this Digimon or a tamer?";
                    string notSelectPlayerMessage = "The opponent is choosing to place a card.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool doPlace = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                    bool underThis = GManager.instance.userSelectionManager.SelectedIntValue == 1;

                    if (doPlace)
                    {
                        Permanent selectedPermanent = null;

                        if (underThis)
                        {
                            selectedPermanent = card.PermanentOfThisCard();
                        }
                        else
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanPlaceUnderTamerCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 tamer to place a card under.", "The opponent is selecting 1 tamer to place a card under.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;

                                yield return null;
                            }
                        }

                        if (selectedPermanent != null)
                        {
                            List<CardSource> selectedCards1 = new List<CardSource>();

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanPlaceCardCondition,
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

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards1.Add(cardSource);

                                yield return null;
                            }

                            selectHandEffect.SetUpCustomMessage("Select 1 card to place.", "Opponent is selecting 1 card to place under.");

                            yield return StartCoroutine(selectHandEffect.Activate());

                            if (selectedCards1.Count > 0)
                            {
                                yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(selectedCards1, activateClass));

                                yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 2, activateClass).Draw());
                            }
                        }
                    }
                }
            }
            #endregion

            #region End of Attack
            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(PlayTamerEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(PlayTamerCanActivateCondition, hash => PlayTamerActivateCoroutine(hash, activateClass), -1, false, PlayTamerEffectDescription("End of Attack"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
            }
            #endregion

            #region On deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(PlayTamerEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(PlayTamerCanActivateCondition, hash => PlayTamerActivateCoroutine(hash, activateClass), -1, false, PlayTamerEffectDescription("On Deletion"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable) => CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
            }
            #endregion

            return cardEffects;
        }
    }
}
