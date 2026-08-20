using System.Collections;
using System.Collections.Generic;

// Reapermon
namespace DCGO.CardEffects.BT26
{
    public class BT26_077 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 5 && targetPermanent.TopCard.HasDMTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Security A. +1
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Execute
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.ExecuteSelfEffect(isInheritedEffect: false, card: card, condition: null));
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

            #region Shared On Play / When Digivolving / When Attacking

            string SharedEffectName() => "May play 1 [Ver.3] Digimon cost 6 (+1 per own face-down source) or lower from trash";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] You may play 1 play cost 6 or lower [Ver.3] trait Digimon card from your trash without paying the cost. For each of this Digimon's face-down digivolution cards, add 1 to the play cost maximum.";

            bool SharedCanActivateCondition(Hashtable hashtable, ICardEffect activateClass)
                => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

            bool FaceDownCondition(CardSource cardSource) => cardSource.IsFaceDown;

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    int maxCost = 6 + card.PermanentOfThisCard().DigivolutionCards.Filter(FaceDownCondition).Count;

                    bool CanSelectCardCondition(CardSource cardSource)
                        => cardSource.IsDigimon
                            && cardSource.ContainsTraits("Ver.3")
                            && cardSource.HasCost && cardSource.GetCostItself <= maxCost
                            && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, root: SelectCardEffect.Root.Trash);

                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                            canTargetCondition: CanSelectCardCondition,
                            root: SelectCardEffect.Root.Trash,
                            cardEffect: activateClass,
                            payCost: false));
                    }
                }
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateCondition(hash, activateClass), (hash) => SharedActivateCoroutine(hash, activateClass), 1, false, SharedEffectDescription("On Play"));
                activateClass.SetHashString("BT26_077_Shared");
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateCondition(hash, activateClass), (hash) => SharedActivateCoroutine(hash, activateClass), 1, false, SharedEffectDescription("When Digivolving"));
                activateClass.SetHashString("BT26_077_Shared");
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateCondition(hash, activateClass), (hash) => SharedActivateCoroutine(hash, activateClass), 1, false, SharedEffectDescription("When Attacking"));
                activateClass.SetHashString("BT26_077_Shared");
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
            }
            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 opponent's highest play cost Digimon or Tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Deletion] Delete 1 of your opponent's Digimon or Tamers with the highest play cost.";

                bool CanSelectDeleteTargetCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                        && (permanent.IsDigimon || permanent.IsTamer)
                        && CardEffectCommons.IsMaxCost(permanent, card.Owner.Enemy, false);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.CanActivateOnDeletion(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteTargetCondition);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteTargetCondition))
                    {
                        SelectPermanentEffect selectDeleteEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectDeleteEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDeleteTargetCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectDeleteEffect.SetUpCustomMessage("Select 1 Digimon or Tamer to delete.", "The opponent is selecting 1 Digimon or Tamer to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectDeleteEffect.Activate());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
