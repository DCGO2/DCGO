using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Abyss Sanctuary: Throne Room
namespace DCGO.CardEffects.BT24
{
    public class BT24_090 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Ignore Color Requirement

            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);
                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return card.Owner.SecurityCards.Count(cardSource => !cardSource.IsFlipped) == 0;
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }
            }

            #endregion

            #region All Turns - Security

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.CardColors.Contains(CardColor.Blue) || permanent.TopCard.CardColors.Contains(CardColor.Yellow))
                        && permanent.TopCard.HasTSTraits;
                }

                #region Blocker
                bool CanUseCondition()
                {
                    return CardEffectCommons.IsExistInSecurity(card, false);
                }

                cardEffects.Add(CardEffectFactory.BlockerStaticEffect(permanentCondition: PermanentCondition, isInheritedEffect: false, card: card, condition: CanUseCondition));
                #endregion

                #region Alliance
                AddSkillClass addSkillClass1 = new AddSkillClass();
                addSkillClass1.SetUpICardEffect("Your Digimon gain <Alliance>", CanUseCondition1, card);
                addSkillClass1.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects1);
                cardEffects.Add(addSkillClass1);

                bool CardSourceCondition(CardSource cardSource)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(cardSource) &&
                           cardSource == cardSource.PermanentOfThisCard().TopCard &&
                           PermanentCondition(cardSource.PermanentOfThisCard());
                }

                bool CanUseCondition1(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistInSecurity(card, false) &&
                           CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasOXII);
                }

                bool HasOXII(Permanent permanent)
                {
                    return permanent.TopCard.EqualsCardName("Neptunemon")
                        || permanent.TopCard.EqualsCardName("Venusmon");
                }

                List<ICardEffect> GetEffects1(CardSource cardSource, List<ICardEffect> effects, EffectTiming effectTiming)
                {
                    if (effectTiming == EffectTiming.OnAllyAttack)
                    {
                        bool Condition()
                        {
                            return CardSourceCondition(cardSource);
                        }

                        effects.Add(CardEffectFactory.AllianceSelfEffect(false, cardSource, Condition));
                    }

                    return effects;
                }
                #endregion
            }

            #endregion

            #region Main Effect

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Replace your bottom sec with this face-up card, play a [TS] Digimon for -3", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] Add your bottom security card to the hand and place this card face up as the bottom security card. Then, you may play 1 blue or yellow [TS] trait Digimon card from your hand with the play cost reduced by 3.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && (cardSource.CardColors.Contains(CardColor.Blue) || cardSource.CardColors.Contains(CardColor.Yellow))
                        && cardSource.HasTSTraits
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, true, activateClass, fixedCost: cardSource.GetCostItself - 3);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectFactory.ReplaceBottomSecurityWithFaceUpOptionEffect(card, activateClass));

                    #region Hand Card Selection

                    CardSource selectedCard = null;
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                    int maxCount = Math.Min(1, card.Owner.HandCards.Count(CanSelectCardCondition));

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


                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    if (selectedCard != null)
                    {
                        #region reduce play cost

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect($"Play Cost -3", CanUseCondition1, card);
                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                        Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                        card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                        ICardEffect GetCardEffect(EffectTiming _timing)
                        {
                            if (_timing == EffectTiming.None)
                            {
                                return changeCostClass;
                            }

                            return null;
                        }

                        bool CanUseCondition1(Hashtable hashtable) => true;

                        int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                        {
                            if (CardSourceCondition(cardSource)
                                && RootCondition(root)
                                && PermanentsCondition(targetPermanents))
                            {
                                Cost -= 3;
                            }

                            return Cost;
                        }

                        bool PermanentsCondition(List<Permanent> targetPermanents)
                        {
                            return targetPermanents == null || targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0;
                        }

                        bool CardSourceCondition(CardSource cardSource)
                        {
                            return cardSource.IsDigimon
                                && (cardSource.CardColors.Contains(CardColor.Blue)
                                    || cardSource.CardColors.Contains(CardColor.Yellow))
                                && cardSource.HasTSTraits;
                        }

                        bool RootCondition(SelectCardEffect.Root root) => true;

                        bool isUpDown() => true;

                        #endregion

                        #region Play card

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: new List<CardSource>() { selectedCard },
                            activateClass: activateClass,
                            payCost: true,
                            isTapped: false,
                            root: SelectCardEffect.Root.Hand,
                            activateETB: true));

                        #endregion

                        #region release effect reducing play cost

                        card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);

                        #endregion

                    }

                    #endregion

                }
            }

            #endregion

            #region Security Effect

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Play 1 lvl 4- Blue or Yellow [TS] Digimon card from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                 => "[Security] You may play 1 level 4 or lower blue or yellow [TS] trait Digimon card from your hand or trash without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                bool CanPlayCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.HasLevel && cardSource.Level <= 4
                        && (cardSource.CardColors.Contains(CardColor.Blue) || cardSource.CardColors.Contains(CardColor.Yellow))
                        && cardSource.HasTSTraits
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanPlayCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanPlayCondition);

                    if (canSelectHand || canSelectTrash)
                    {
                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<int>> selectionElements1 = new List<SelectionElement<int>>()
                        {
                            new (message: $"From hand", value : 1, spriteIndex: 0),
                            new (message: $"From trash", value : 2, spriteIndex: 1),
                            new (message: $"Don't play", value: 3, spriteIndex: 2)
                        };

                            string selectPlayerMessage1 = "From which area will you play a card?";
                            string notSelectPlayerMessage1 = "The opponent is choosing from which area to select a card.";

                            GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage1, notSelectPlayerMessage: notSelectPlayerMessage1);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetInt(canSelectHand ? 1 : 2);
                        }
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                        bool fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                        bool fromTrash = GManager.instance.userSelectionManager.SelectedIntValue == 2;

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
                                canTargetCondition: CanPlayCondition,
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

                            selectHandEffect.SetUpCustomMessage("Select 1 digimon to play", "The opponent is selecting 1 digimon to play");

                            yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                            if (selectedCards.Count > 0) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                selectedCards,
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.Hand,
                                activateETB: true));
                        }
                        if (fromTrash)
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanPlayCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 digimon to play",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 digimon to play", "The opponent is selecting 1 digimon to play");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Selected Digimon");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                            if (selectedCards.Count > 0) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                selectedCards,
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.Trash,
                                activateETB: true));
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
