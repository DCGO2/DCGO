using System.Collections;
using System.Collections.Generic;

// Giromon
namespace DCGO.CardEffects.BT26
{
    public class BT26_055 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasDMTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 4));
            }
            #endregion

            #region Fragment
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                string EffectDescription()
                    => "<Fragment <2>> (When this Digimon would be deleted, by trashing any 2 of its digivolution cards, it isn't deleted.)";

                cardEffects.Add(CardEffectFactory.FragmentSelfEffect(isInheritedEffect: false, card: card, condition: null, trashValue: 2, effectName: "Fragment <2>", effectDiscription: EffectDescription()));
            }
            #endregion

            #region Shared On Play / When Digivolving / Counter

            string SharedEffectName() => "May place 1 hand card face down as source, then may delete 1 [Ver.3] Digimon and opponent's lowest cost Digimon";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] You may place 1 card in your hand face down as this Digimon's bottom digivolution card. Then, you may delete 1 of your Digimon with the [Ver.3] trait and all of your opponent's Digimon with the lowest play cost.";

            bool CanSelectOwnVer3Condition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.EqualsTraits("Ver.3");

            bool IsOpponentDigimon(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && CardEffectCommons.IsMinCost(permanent, card.Owner.Enemy, true);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool isUsed = false;

                if (card.Owner.HandCards.Count >= 1)
                {
                    CardSource selectedCard = null;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: _ => true,
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
                        selectedCard = cardSource;
                        yield return null;
                    }

                    selectHandEffect.SetUpCustomMessage("Select 1 card to place face down as this Digimon's bottom digivolution card.", "The opponent is selecting 1 card to place as this Digimon's bottom digivolution card.");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    if (selectedCard != null)
                    {
                        isUsed = true;

                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass, isFacedown: true));
                    }
                }

                bool hasOwnVer3 = CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnVer3Condition);
                bool hasOpponentDigimon = CardEffectCommons.HasMatchConditionPermanent(IsOpponentDigimon);

                if (hasOwnVer3 || hasOpponentDigimon)
                {
                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                    {
                        new SelectionElement<bool>(message: "Yes", value: true, spriteIndex: 0),
                        new SelectionElement<bool>(message: "No", value: false, spriteIndex: 1),
                    };

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: "Delete 1 of your [Ver.3] trait Digimon and all of your opponent's Digimon with the lowest play cost?", notSelectPlayerMessage: "The opponent is choosing whether to delete Digimon.");

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    if (GManager.instance.userSelectionManager.SelectedBoolValue)
                    {
                        if (hasOwnVer3)
                        {
                            SelectPermanentEffect selectVer3Effect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectVer3Effect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectOwnVer3Condition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            selectVer3Effect.SetUpCustomMessage("Select 1 [Ver.3] trait Digimon to delete.", "The opponent is selecting 1 [Ver.3] trait Digimon to delete.");

                            yield return ContinuousController.instance.StartCoroutine(selectVer3Effect.Activate());

                            IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                            {
                                if (permanents != null && permanents.Count > 0)
                                {
                                    isUsed = true;

                                    yield return null;
                                }
                            }
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentDigimon))
                        {
                            List<Permanent> targetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(IsOpponentDigimon);

                            if (targetPermanents.Count >= 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(targetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                            }
                        }

                        if (!isUsed) activateClass.RemoveUse();
                    }
                }
            }

            #endregion

            #region On Play / When Digivolving / Counter
            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                isSkippable: true,
                maxCountPerTurn: 1,
                hashValue: "BT26_055_Shared",
                onPlay: true,
                whenDigivolving: true,
                counter: true);
            #endregion

            #region Inherit - All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash opponent's top security card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When this Digimon would leave the battle area, trash your opponent's top security card.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

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
