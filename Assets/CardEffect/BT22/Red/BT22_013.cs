using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR;

//Wargreymon
namespace DCGO.CardEffects.BT22
{
    public class BT22_013 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            //Level 5 CS trait/graymon in name
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel5 &&
                           (targetPermanent.TopCard.HasCSTraits || targetPermanent.TopCard.HasGreymonName);
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

            #region Warp Effect
            //Warp from Augumon
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve for a cost of 6", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Hand][Main] If you have [Nokia Shiramine], 1 of your [Agumon] digivolves into this card for a digivolution cost of 6, ignoring digivolution requirements.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnHand(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(HasNokia) &&
                           CardEffectCommons.HasMatchConditionPermanent(PermanentCondition);
                }

                bool HasNokia(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card) &&
                           targetPermanent.TopCard.EqualsCardName("Nokia Shiramine");
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(targetPermanent, card) && 
                           targetPermanent.TopCard.EqualsCardName("Agumon");
                }

                bool CanSelectHandCardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: PermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will digivolve.",
                        "The opponent is selecting 1 Digimon that will digivolve.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: selectedPermanent,
                            cardCondition: CanSelectHandCardCondition,
                            payCost: true,
                            reduceCostTuple: null,
                            fixedCostTuple:null,
                            ignoreDigivolutionRequirementFixedCost: 6,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null,
                            ignoreRequirements: CardEffectCommons.IgnoreRequirement.All));
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Choose 1 effect", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Activate 1 of the effects below:\r\n・1 of your [Gabumon] may digivolve into [MetalGarurumon] in the hand, ignoring digivolution requirements and without paying the cost.\r\n・Delete 1 of your opponent's Digimon with the lowest DP.";
                }

                #region Evo Conditions

                bool CanSelectOwnPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.EqualsCardName("Gabumon"))
                        {
                            if(card.Owner.HandCards.Any(x => CanSelectHandCardCondition(x) && x.CanPlayCardTargetFrame(permanent.PermanentFrame,false,activateClass,ignore:CardEffectCommons.IgnoreRequirement.All)))
                                return true;
                        }
                    }

                    return false;
                }

                bool CanSelectHandCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.EqualsCardName("MetalGarurumon");
                }

                #endregion

                bool CanSelectOpponentPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable,card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool CanDelete = CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentCondition);
                    bool CanEvo = CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnPermanentCondition);

                    

                    if(CanDelete || CanEvo)
                    {
                        yield return GManager.instance.photonWaitController.StartWait("WarGreymon_BT22-013");

                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new(
                                message: "1 of your [Gabumon] may digivolve into [MetalGarurumon] in your hand",
                                value: 0, spriteIndex: 0),
                            new(message: "Delete 1 of your opponent's Digimon with the lowest DP",
                                value: 1, spriteIndex: 0),
                        };

                        string selectPlayerMessage = "Which effect will you activate?";
                        string notSelectPlayerMessage = "The opponent is choosing which effect to activate.";

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements,
                                selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                                notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        int actionID = GManager.instance.userSelectionManager.SelectedIntValue;

                        switch (actionID)
                        {
                            case 0:
                                {
                                    if (CanEvo)
                                    {
                                        Permanent selectedPermanent = null;

                                        SelectPermanentEffect selectPermanentEffect =
                                            GManager.instance.GetComponent<SelectPermanentEffect>();

                                        selectPermanentEffect.SetUp(
                                            selectPlayer: card.Owner,
                                            canTargetCondition: CanSelectOwnPermanentCondition,
                                            canTargetCondition_ByPreSelecetedList: null,
                                            canEndSelectCondition: null,
                                            maxCount: 1,
                                            canNoSelect: true,
                                            canEndNotMax: false,
                                            selectPermanentCoroutine: SelectPermanentCoroutine,
                                            afterSelectPermanentCoroutine: null,
                                            mode: SelectPermanentEffect.Mode.Custom,
                                            cardEffect: activateClass);

                                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will digivolve.",
                                            "The opponent is selecting 1 Digimon that will digivolve.");

                                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                        {
                                            selectedPermanent = permanent;
                                            yield return null;
                                        }

                                        if (selectedPermanent != null)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                                targetPermanent: selectedPermanent,
                                                cardCondition: CanSelectHandCardCondition,
                                                payCost: false,
                                                reduceCostTuple: null,
                                                fixedCostTuple: null,
                                                ignoreDigivolutionRequirementFixedCost: 0,
                                                isHand: true,
                                                activateClass: activateClass,
                                                successProcess: null,
                                                ignoreRequirements: CardEffectCommons.IgnoreRequirement.All));
                                        }
                                    }

                                    break;
                                }

                            case 1:
                                {
                                    if (CanDelete)
                                    {
                                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                        selectPermanentEffect.SetUp(
                                            selectPlayer: card.Owner,
                                            canTargetCondition: CanSelectOpponentPermanentCondition,
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

                                    break;
                                }
                        }
                    }
                }
            }
            #endregion

            #region When Attacking - ESS
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash the top card of opponent's security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("TrashSecurity_BT22_013");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] If this Digimon has [Omnimon] in its name, trash your opponent's top security card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.PermanentOfThisCard().TopCard.ContainsCardName("Omnimon");
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}