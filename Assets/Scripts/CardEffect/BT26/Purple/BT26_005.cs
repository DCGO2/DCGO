using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Pinamon
namespace DCGO.CardEffects.BT26
{
    public class BT26_005 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Inherit
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash a Tamer's bottom face-down card to play [Avian]/[DATA SQUAD] from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Deletion] By trashing the bottom face-down card from under any of your Tamers, you may play 1 play cost 5 or lower [Avian] or [DATA SQUAD] trait card from your trash without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);

                bool TamerWithFaceDownCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.HasFaceDownDigivolutionCards;

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: TamerWithFaceDownCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: true,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer to trash 1 bottom face-down card from.", "The opponent is selecting 1 Tamer to trash 1 bottom face-down card from.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        if (permanents.Count == 1)
                        {
                            Permanent selectedPermanent = permanents[0];
                            CardSource bottomFaceDownCard = selectedPermanent.DigivolutionCards.LastOrDefault(cardSource => cardSource.IsFlipped);

                            if (bottomFaceDownCard != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsAndProcessAccordingToResult(
                                    targetPermanent: selectedPermanent,
                                    targetDigivolutionCards: new List<CardSource>() { bottomFaceDownCard },
                                    activateClass: activateClass,
                                    successProcess: SuccessProcess,
                                    failureProcess: null));
                            }
                        }
                    }

                    IEnumerator SuccessProcess(List<CardSource> trashedCards)
                    {
                        bool CanSelectCardCondition(CardSource cardSource)
                            => (cardSource.ContainsTraits("Avian") || cardSource.EqualsTraits("DATA SQUAD"))
                                && cardSource.HasPlayCost && cardSource.GetCostItself <= 5
                                && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass, root: SelectCardEffect.Root.Trash);

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
            }
            #endregion

            return cardEffects;
        }
    }
}
