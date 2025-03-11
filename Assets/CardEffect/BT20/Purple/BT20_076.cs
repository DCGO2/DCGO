using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.BT20
{
    public class BT20_076 : CEntity_Effect
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
                                                    if (permanent.Levels_ForJogress(card).Contains(5))
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
                                                    if (permanent.Levels_ForJogress(card).Contains(5))
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
                        new JogressConditionElement(PermanentCondition1, "a level 5 purple Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 5 red Digimon"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }

            #endregion

            #region Ace - Blast DNA Digivolve

            if (timing == EffectTiming.OnCounterTiming)
                {
                    List<BlastDNACondition> blastDNAConditions = new List<BlastDNACondition>
                    {
                        new("Dinobeemon"),
                        new("Paildramon")
                    };
                    cardEffects.Add(CardEffectFactory.BlastDNADigivolveEffect(card: card,
                        blastDNAConditions: blastDNAConditions, condition: null));
                }

            #endregion

            #region On Play 
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {                

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Delete 1 of your opponent's Digimon with 11000 DP or less. Then, if DNA digivolving, this Digimon may digivolve into [Imperialdramon: Fighter Mode] in the hand or trash without paying the cost.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(11000, activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectDigivolveCondition(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Imperialdramon: Fighter Mode") &&
                           cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, false, activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    if (CardEffectCommons.IsJogress(_hashtable))
                    {
                        bool canSelectHand = card.Owner.HandCards.Count(CanSelectDigivolveCondition) >= 1;
                        bool canSelectTrash = card.Owner.TrashCards.Count(CanSelectDigivolveCondition) >= 1;

                        if (canSelectHand || canSelectTrash)
                        {
                            if (canSelectHand && canSelectTrash)
                            {
                                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                        };

                                string selectPlayerMessage = "From which area do you want to digivolve from?";
                                string notSelectPlayerMessage = "The opponent is choosing where to digivolve from.";

                                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                            }
                            else
                            {
                                GManager.instance.userSelectionManager.SetBool(canSelectHand);
                            }

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                            if (fromHand)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: card.PermanentOfThisCard(),
                                cardCondition: CanSelectDigivolveCondition,
                                payCost: false,
                                reduceCostTuple: null,
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: true,
                                activateClass: activateClass,
                                successProcess: null));
                            }
                            else
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: card.PermanentOfThisCard(),
                                cardCondition: CanSelectDigivolveCondition,
                                payCost: false,
                                reduceCostTuple: null,
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: false,
                                activateClass: activateClass,
                                successProcess: null));
                            }
                        }
                    }
                }
            }
            #endregion
        
            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {       
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Delete 1 of your opponent's Digimon with 11000 DP or less. Then, if DNA digivolving, this Digimon may digivolve into [Imperialdramon: Fighter Mode] in the hand or trash without paying the cost.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(11000, activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectDigivolveCondition(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Imperialdramon: Fighter Mode") &&
                           cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame,false,activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card); ;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    if (CardEffectCommons.IsJogress(_hashtable))
                    {
                        bool canSelectHand = card.Owner.HandCards.Count(CanSelectDigivolveCondition) >= 1;
                        bool canSelectTrash = card.Owner.TrashCards.Count(CanSelectDigivolveCondition) >= 1;

                        if (canSelectHand || canSelectTrash)
                        {
                            if (canSelectHand && canSelectTrash)
                            {
                                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                        };

                                string selectPlayerMessage = "From which area do you want to digivolve from?";
                                string notSelectPlayerMessage = "The opponent is choosing where to digivolve from.";

                                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                            }
                            else
                            {
                                GManager.instance.userSelectionManager.SetBool(canSelectHand);
                            }

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                            if (fromHand)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: card.PermanentOfThisCard(),
                                cardCondition: CanSelectDigivolveCondition,
                                payCost: false,
                                reduceCostTuple: null,
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: true,
                                activateClass: activateClass,
                                successProcess: null));
                            }
                            else
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: card.PermanentOfThisCard(),
                                cardCondition: CanSelectDigivolveCondition,
                                payCost: false,
                                reduceCostTuple: null,
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: false,
                                activateClass: activateClass,
                                successProcess: null));
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
