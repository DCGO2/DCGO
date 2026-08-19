using System.Collections;
using System.Collections.Generic;
using System.Linq;

// ScourgeChiropmon // Despair Blast
namespace DCGO.CardEffects.BT26
{
    public class BT26_075 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Effects
            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsTraits("Glowing Dawn");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 4));
            }
            #endregion

            #region Execute
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.ExecuteSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Ascension
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.AscensionSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared Security / On Deletion

            string SharedEffectName() => "By trashing 1 Tamer's bottom face-down card, use 1 [Glowing Dawn] card cost 5 or less from trash";

            string SharedEffectDescription(string tag)
                => $"[{tag}] By trashing the bottom face-down card from under any of your Tamers, you may play 1 [Glowing Dawn] trait card with a play cost of 5 or less from your trash without paying the cost.";

            bool IsTamerWithFaceDownCard(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.DigivolutionCards.Count(cs => cs.IsFaceDown) >= 1;

            bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;

            bool CanSelectPlayCardCondition(CardSource cardSource, ICardEffect activateClass)
                => cardSource.ContainsTraits("Glowing Dawn")
                    && cardSource.HasCost
                    && cardSource.GetCostItself <= 5
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(IsTamerWithFaceDownCard))
                {
                    Permanent selectedTamer = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsTamerWithFaceDownCard,
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

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer to trash the bottom face-down card from.", "The opponent is selecting 1 Tamer to trash the bottom face-down card from.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    if (selectedTamer != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedTamer, trashCount: 1, isFromTop: false, activateClass: activateClass, cardCondition: FaceDownCards));

                        bool CanSelectPlayCardConditionBound(CardSource cardSource) => CanSelectPlayCardCondition(cardSource, activateClass);

                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectPlayCardConditionBound))
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                                canTargetCondition: CanSelectPlayCardConditionBound,
                                root: SelectCardEffect.Root.Trash,
                                cardEffect: activateClass,
                                payCost: false));
                        }
                    }
                }
            }

            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(null, (hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("Security"));
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }
            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, (hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Deletion"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.CanActivateOnDeletion(card, activateClass);
            }
            #endregion
            #endregion

            #region Option Effects
            #region Use Req.
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource) => cardSource.ContainsTraits("Glowing Dawn");
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 opponent's lowest level Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] Delete 1 of your opponent's Digimon with the lowest level.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
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
            }
            #endregion

            #region Arts Digivolution
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }
            #endregion
            #endregion

            return cardEffects;
        }
    }
}
