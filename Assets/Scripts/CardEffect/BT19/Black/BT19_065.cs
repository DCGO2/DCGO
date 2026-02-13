using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT19
{
    public class BT19_065 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region DigiXros
            if (timing == EffectTiming.None)
            {
                AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
                addDigiXrosConditionClass.SetUpICardEffect($"DigiXros", CanUseCondition, card);
                addDigiXrosConditionClass.SetUpAddDigiXrosConditionClass(getDigiXrosCondition: GetDigiXros);
                addDigiXrosConditionClass.SetNotShowUI(true);
                cardEffects.Add(addDigiXrosConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }



                DigiXrosCondition GetDigiXros(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        DigiXrosConditionElement element = new DigiXrosConditionElement(CanSelectCardCondition, "5 Lv.5 or lower [Cyborg]/[Composite] trait Digimon cards w/different card numbers.");

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                if (cardSource.Owner == card.Owner)
                                {
                                    if (cardSource.IsDigimon)
                                    {
                                        if(cardSource.HasLevel && cardSource.Level <= 5)
                                        {
                                            if (cardSource.EqualsTraits("Cyborg"))
                                            {
                                                return true;
                                            }

                                            if (cardSource.EqualsTraits("Composite"))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>();

                        for (int i = 0; i < 5; i++)
                        {
                            elements.Add(element);
                        }

                        bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                        {
                            List<string> cardIDs = new List<string>();

                            foreach (CardSource cardSource1 in cardSources)
                            {
                                if (!cardIDs.Contains(cardSource1.CardID))
                                {
                                    cardIDs.Add(cardSource1.CardID);
                                }
                            }

                            if (cardIDs.Contains(cardSource.CardID))
                            {
                                return false;
                            }

                            return true;
                        }

                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, CanTargetCondition_ByPreSelecetedList, 1);

                        return digiXrosCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 level 5 or lower Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Delete 1 level 5 or lower Digimon.";
                }

                bool IsLevel5Digimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent) && 
                           permanent.TopCard.HasLevel && permanent.TopCard.Level <= 5;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(IsLevel5Digimon);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsLevel5Digimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.","The opponent is selecting 1 Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
            #endregion

            #region When Digiolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 level 5 or lower Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Delete 1 level 5 or lower Digimon.";
                }

                bool IsLevel5Digimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent) &&
                           permanent.TopCard.HasLevel && permanent.TopCard.Level <= 5;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(IsLevel5Digimon);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsLevel5Digimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 level 5 or lower digimon from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] You may play 1 level 5 or lower Digimon card with the [Cyborg] or [Composite] trait from your trash without paying the cost.";
                }

                bool IsTargetableDigimon(CardSource source)
                {
                    return source.IsDigimon &&
                           source.HasLevel && source.Level <= 5 &&
                           (source.EqualsTraits("Cyborg") || source.EqualsTraits("Composite")) &&
                           CardEffectCommons.CanPlayAsNewPermanent(source, false, activateClass,SelectCardEffect.Root.Trash);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card) &&
                           CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsTargetableDigimon);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource playedSource = null;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                    canTargetCondition: IsTargetableDigimon,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    canNoSelect: () => false,
                    selectCardCoroutine: SelectCardCoroutine,
                    afterSelectCardCoroutine: null,
                    message: "Select 1 Digimon to play.",
                    maxCount: 1,
                    canEndNotMax: false,
                    isShowOpponent: false,
                    mode: SelectCardEffect.Mode.Custom,
                    root: SelectCardEffect.Root.Trash,
                    customRootCardList: null,
                    canLookReverseCard: true,
                    selectPlayer: card.Owner,
                    cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource source)
                    {
                        PlayCardClass playCard = new PlayCardClass(
                                cardSources: new List<CardSource>() { source },
                                hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                payCost: false,
                                targetPermanent: null,
                                isTapped: false,
                                root: SelectCardEffect.Root.Trash,
                                activateETB: true);

                        yield return ContinuousController.instance.StartCoroutine(playCard.PlayCard());
                    }
                }
            }
            #endregion

            #region Opponents Turn - ESS
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Switch attack target", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("SwitchTarget_BT18-073");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Opponent's Turn] [Once Per Turn] When any of your opponent's Digimon attack, you may change the attack target to 1 of your Digimon with the [Composite] or [Wicked God] trait.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           (permanent.TopCard.EqualsTraits("Composite") || permanent.TopCard.EqualsTraits("Wicked God"));
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsOpponentPermanent(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, PermanentCondition))
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (GManager.instance.attackProcess.IsAttacking)
                        {
                            if (GManager.instance.attackProcess.AttackingPermanent.CanSwitchAttackTarget)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(GManager.instance.attackProcess.AttackingPermanent, card))
                        {
                            if (GManager.instance.attackProcess.IsAttacking)
                            {
                                if (GManager.instance.attackProcess.AttackingPermanent.CanSwitchAttackTarget)
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

                                    selectPermanentEffect.SetUpCustomMessage(
                                        "Select 1 Digimon to switch the attack to.",
                                        "The opponent is selecting 1 Digimon to switch the attack to.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SwitchDefender(
                                        activateClass,
                                        false,
                                        permanent));
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