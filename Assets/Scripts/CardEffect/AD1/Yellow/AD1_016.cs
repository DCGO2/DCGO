using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

// ShineGreymon
namespace DCGO.CardEffects.AD1
{
    public class AD1_016 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolve Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("RizeGreymon")
                            || targetPermanent.TopCard.EqualsTraits("DATA SQUAD");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 5, permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Alliance
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared WD / WA

            string SharedHashString = "AD1_016_WD_WA";

            string SharedEffectName = "You may play [Marcus Damon] from hand or trash for free. The 1 enemy gets -3k DP for each of your Digimon or Tamers";

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] You may play 1 [Marcus Damon] from your hand or trash without paying the cost. Then, to 1 of your opponent's Digimon, give -3000 DP for each of your Digimon and Tamers until their turn ends.";

            bool IsMarcusDamonCard(CardSource cardSource) => cardSource.EqualsCardName("Marcus Damon");

            bool IsDigimonOrTamerPermanent(Permanent permanent) => permanent.IsDigimon || permanent.IsTamer;

            bool IsOpponentsDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool validHandCard = CardEffectCommons.HasMatchConditionOwnersHand(card, IsMarcusDamonCard);
                bool validTrashCard = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsMarcusDamonCard);

                if (validHandCard || validTrashCard)
                {
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                    int index = 0;
                    if (validHandCard)
                    {
                        selectionElements.Add(new (message: $"Play Marcus Damon from hand", value : 1, spriteIndex: 0));
                        index++;
                    }
                    if (validTrashCard)
                    {
                        selectionElements.Add(new (message: $"Play Marcus Damon from trash", value : 2, spriteIndex: 0));
                        index++;
                    };
                    selectionElements.Add( new (message: $"Don't play", value: 3, spriteIndex: 1));

                    string selectPlayerMessage = "From where will you play a Marcus Damon?";
                    string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool doPlay = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                    bool fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;

                    if (doPlay)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        if (fromHand)
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsMarcusDamonCard,
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

                            yield return StartCoroutine(selectHandEffect.Activate());
                        }
                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                        canTargetCondition: IsMarcusDamonCard,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => true,
                                        selectCardCoroutine: null,
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

                if (card.Owner.Enemy.GetBattleAreaDigimons().Any())
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOpponentsDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        int changeAmount = -3000 * card.Owner.GetBattleAreaPermanents().Count(IsDigimonOrTamerPermanent); 
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: changeAmount, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                    }
                }
            }

            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), 1, false, SharedEffectDescription("When Digivolving"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card) &&
                           CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), 1, false, SharedEffectDescription("When Attacking"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card) &&
                           CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }
            #endregion

            #region All Turns

            string AllTurnsHashString = "AD1_016_AT";

            string AllTurnsEffectName = "Delete 1 enemy Digimon with s much or Less DP as this";

            string AllTurnsEffectDescription() 
                => "[All Turns] [Once Per Turn] When any of your [Marcus Damon]s are played or suspend, you may delete 1 of your opponent's Digimon with as much or less DP as this Digimon.";

            bool IsYourMarcusDamon(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                    && permanent.TopCard.EqualsCardName("Marcus Damon");
            }

            bool IsWeakerEnemyDigimon(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.HasDP
                    && permanent.DP <= card.PermanentOfThisCard().DP;
            }

            bool AllTurnsCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card)
                    && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsWeakerEnemyDigimon);
            }

            IEnumerator AllTurnsActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: IsWeakerEnemyDigimon,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: null,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Destroy,
                    cardEffect: activateClass);

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(AllTurnsEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(AllTurnsCanActivateCondition, hash => AllTurnsActivateCoroutine(hash, activateClass), 1, true, AllTurnsEffectDescription());
                activateClass.SetHashString(AllTurnsHashString);
                cardEffects.Add(activateClass);
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, IsYourMarcusDamon);
                }
            }

            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(AllTurnsEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(AllTurnsCanActivateCondition, hash => AllTurnsActivateCoroutine(hash, activateClass), 1, true, AllTurnsEffectDescription());
                activateClass.SetHashString(AllTurnsHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, IsYourMarcusDamon);
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
