using System.Collections;
using System.Collections.Generic;

// Liollmon
namespace DCGO.CardEffects.BT26
{
    public class BT26_025 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Glowing Dawn");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 2));
            }
            #endregion

            #region Shared WM / OP

            string SharedEffectName()
                => "By placing top security under a [Glowing Dawn] Tamer, Recovery +1";

            string SharedEffectDescription(string tag)
                => $"[{tag}] By placing your top security card face down under any of your Digimon with the [Glowing Dawn] trait Tamers, <Recovery +1>.";

            bool CanSelectTamerCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.TopCard.EqualsTraits("Glowing Dawn");

            bool SharedAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => card.Owner.SecurityCards.Count >= 1
                    && CardEffectCommons.HasMatchConditionPermanent(CanSelectTamerCondition);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent selectedTamer = null;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectTamerCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    selectedTamer = permanent;
                    yield return null;
                }

                selectPermanentEffect.SetUpCustomMessage("Select 1 [Glowing Dawn] Tamer to place your top security card under.", "The opponent is selecting 1 [Glowing Dawn] Tamer to place their top security card under.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                if (selectedTamer != null)
                {
                    CardSource topSecurityCard = card.Owner.SecurityCards[0];

                    yield return ContinuousController.instance.StartCoroutine(selectedTamer.AddDigivolutionCardsBottom(new List<CardSource>() { topSecurityCard }, activateClass, isFacedown: true));

                    yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                        player: card.Owner,
                        refSkillInfos: ref ContinuousController.instance.nullSkillInfos,
                        activateClass).ReduceSecurity());

                    yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                }
            }

            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                isSkippable: true,
                additionalActivateCondition: SharedAdditionalActivateCondition,
                whenMoving: true,
                onPlay: true);

            #region Inherit
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Add top security to hand, then Recovery +1 if 0 security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippableFunction(IsSkippable);
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT26_025_Inherit");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] [Once Per Turn] You may add your top security card to the hand. Then, if you have 0 security cards, <Recovery +1>.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                bool IsSkippable(Hashtable hashtable) => card.Owner.SecurityCards.Count != 0;

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool isUsed = false;

                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Will you add your top security card to hand?";
                        string notSelectPlayerMessage = "The opponent is choosing whether or not to add their top security card to hand.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool willAdd = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (willAdd)
                        {
                            isUsed = true;
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { card.Owner.SecurityCards[0] }, false, activateClass));

                            yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                                player: card.Owner,
                                refSkillInfos: ref ContinuousController.instance.nullSkillInfos,
                                activateClass).ReduceSecurity());
                        }
                    }

                    if (card.Owner.SecurityCards.Count == 0)
                    {
                        isUsed = true;
                        yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                    }

                    if (!isUsed) activateClass.RemoveUse();
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
