using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace DCGO.CardEffects.BT16
{
    public class Dinobeemon_BT16_077 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region DNA Digivolution
            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect($"DNA Digivolution", CanUseCondition, card);
                addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
                addJogressConditionClass.SetNotShowUI(true);
                cardEffects.Add(addJogressConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }



                JogressCondition GetJogress(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        bool PermanentCondition1(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.IsDigimon)
                                        {
                                            if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                            {
                                                if (permanent.TopCard.CardColors.Contains(CardColor.Purple))
                                                {
                                                    if (permanent.Levels_ForJogress(card).Contains(4))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        bool PermanentCondition2(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.IsDigimon)
                                        {
                                            if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                            {
                                                if (permanent.TopCard.CardColors.Contains(CardColor.Red))
                                                {
                                                    if (permanent.Levels_ForJogress(card).Contains(4))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "a level 4 purple Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 4 red Digimon"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Raid
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Partition & Partition Inherit

            if (timing == EffectTiming.WhenRemoveField)
            {

                bool CanSelectFirstSource(CardSource cardSource)
                {
                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardColors.Contains(CardColor.Purple) && cardSource.HasLevel && cardSource.Level == 4) >= 1)
                    {
                        return true;
                    }
                    return false;
                }

                bool CanSelectSecondSource(CardSource cardSource)
                {
                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardColors.Contains(CardColor.Red) && cardSource.HasLevel && cardSource.Level == 4) >= 1)
                    {
                        return true;
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.PartitionSelfEffect(false, card, null, CanSelectFirstSource, CanSelectSecondSource));

            }

            if (timing == EffectTiming.WhenRemoveField)
            {

                bool CanSelectFirstSource(CardSource cardSource)
                {
                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardColors.Contains(CardColor.Purple) && cardSource.HasLevel && cardSource.Level == 4) >= 1)
                    {
                        return true;
                    }
                    return false;
                }

                bool CanSelectSecondSource(CardSource cardSource)
                {
                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardColors.Contains(CardColor.Red) && cardSource.HasLevel && cardSource.Level == 4) >= 1)
                    {
                        return true;
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.PartitionSelfEffect(true, card, null, CanSelectFirstSource, CanSelectSecondSource));

            }
            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("You may play 1 level 5 or lower Digimon with the [Free] trait. Then 1 Digimon gains [Rush] and can attack.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If DNA digivolving, you may play 1 level 5 or lower Digimon with the [Free] trait from your trash without paying the cost. Then, 1 of your Digimon gains [Rush] for the turn and may attack the player.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.IsExistOnTrash(cardSource))
                    {
                        if (cardSource.Level <= 5 && cardSource.ContainsTraits("Free") && (cardSource.IsDigimon))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.IsDigimon)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    if (CardEffectCommons.IsJogress(hashtable))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            int maxCount1 = 1;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectCardCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => true,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 card to play.",
                                        maxCount: maxCount1,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Custom,
                                        customRootCardList: card.Owner.TrashCards,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 trash card to play.", "The opponent is selecting 1 trash card to play.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");


                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            yield return StartCoroutine(selectCardEffect.Activate());

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Trash,
                            activateETB: true));
                        }
                    }

                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                        };

                    string selectPlayerMessage = "Will you select a Digimon to gain Rush and must attack?";
                    string notSelectPlayerMessage = "The opponent is choosing effects.";

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                bool gainRush = GManager.instance.userSelectionManager.SelectedBoolValue;

                if (gainRush)
                {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get Rush.", "The opponent is selecting 1 Digimon that will get Rush.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                Permanent selectedPermanent = permanent;

                                if (selectedPermanent != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRush(
                                        targetPermanent: selectedPermanent,
                                        effectDuration: EffectDuration.UntilEachTurnEnd,
                                        activateClass: activateClass));

                                    if (selectedPermanent.CanAttack(activateClass))
                                    {
                                        SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                        selectAttackEffect.SetUp(
                                            attacker: selectedPermanent,
                                            canAttackPlayerCondition: () => true,
                                            defenderCondition: (permanent) => false,
                                            cardEffect: activateClass);

                                        yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                                    }
                                }


                            }
                        }
                }
            }
        }
            #endregion

            return cardEffects;
        }
    }
}