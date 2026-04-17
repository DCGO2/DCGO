using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Gear Forest Village
namespace DCGO.CardEffects.BT25
{
    public class BT25_099 : CEntity_Effect
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

            #region Alliance
            if (timing == EffectTiming.OnAllyAttack)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.CardColors.Contains(CardColor.Green) || permanent.TopCard.CardColors.Contains(CardColor.Black))
                        && permanent.TopCard.HasTSTraits;
                }

                bool CanUseCondition()
                {
                    return CardEffectCommons.IsExistInSecurity(card, false);
                }

                cardEffects.Add(CardEffectFactory.AllianceStaticEffect(PermanentCondition, false, card, CanUseCondition));
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.CardColors.Contains(CardColor.Green) || permanent.TopCard.CardColors.Contains(CardColor.Black))
                        && permanent.TopCard.HasTSTraits;
                }

                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("Your Digimon gain Piercing", CanUseCondition, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects, limitTiming: EffectTiming.OnDetermineDoSecurityCheck);
                cardEffects.Add(addSkillClass);

                bool HasOXII(Permanent permanent)
                {
                    return permanent.TopCard.EqualsCardName("Bacchusmon")
                        || permanent.TopCard.EqualsCardName("Ceresmon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistInSecurity(card, false)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasOXII);
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(cardSource))
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource == cardSource.PermanentOfThisCard().TopCard)
                            {
                                if (PermanentCondition(cardSource.PermanentOfThisCard()))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                {
                    if (_timing == EffectTiming.OnDetermineDoSecurityCheck)
                    {
                        bool Condition()
                        {
                            return true;
                        }

                        cardEffects.Add(CardEffectFactory.PierceSelfEffect(false, cardSource, Condition));
                    }

                    return cardEffects;
                }
            }

            #endregion

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
                    return "[Main] Add your bottom security card to the hand and place this card face up as the bottom security card. Then, you may play 1 green or black [TS] trait Digimon card from your hand with the play cost reduced by 3.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && (cardSource.HasCardColor(CardColor.Green) || cardSource.HasCardColor(CardColor.Black))
                        && cardSource.HasTSTraits
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, true, activateClass, fixedCost: cardSource.GetCostItself - 3);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectFactory.ReplaceBottomSecurityWithFaceUpOptionEffect(card, activateClass));

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
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.PlayForCost,
                        cardEffect: activateClass);

                    selectHandEffect.SetReducedCostTuple((3, null));
                    selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                }
            }

            #endregion

            #region Security Effect

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Play 1 lvl 4- Green or Black [TS] Digimon card from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                 => "[Security] You may play 1 level 4 or lower green or black [TS] trait Digimon card from your hand or trash without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                bool CanPlayCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.HasLevel && cardSource.Level <= 4
                        && (cardSource.HasCardColor(CardColor.Green) || cardSource.HasCardColor(CardColor.Black))
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
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.PlayForFree,
                                cardEffect: activateClass);

                            selectHandEffect.SetUpCustomMessage("Select 1 digimon to play", "The opponent is selecting 1 digimon to play");

                            yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                        }
                        if (fromTrash)
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanPlayCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 digimon to play",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.PlayForFree,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 digimon to play", "The opponent is selecting 1 digimon to play");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Selected Digimon");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
