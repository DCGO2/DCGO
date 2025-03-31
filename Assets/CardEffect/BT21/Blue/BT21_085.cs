using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT21
{
    public class BT21_085 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Start of Your Main Phase] If your opponent has a Digimon, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           card.Owner.Enemy.GetBattleAreaDigimons().Count >= 1 &&
                           card.Owner.CanAddMemory(activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            #endregion

            #region Main

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash top card of [Armor Form] Digimon to <Draw 1> and gain 1 memory", CanUseCondition,
                    card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Main] By suspending this Tamer and trashing the top stacked card of 1 of your [Armor Form] trait Digimon, <Draw 1> and gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.DigivolutionCards.Count >= 1 &&
                           (permanent.TopCard.EqualsTraits("Armor Form") || permanent.TopCard.EqualsTraits("ArmorForm"));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.CanActivateSuspendCostEffect(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(
                        new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to trash the top stacked card of.",
                        "The opponent is selecting 1 Digimon to trash the top stacked card of.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            new IDegeneration(selectedPermanent, 1, activateClass).Degeneration());

                        yield return ContinuousController.instance.StartCoroutine(
                            new DrawClass(card.Owner, 1, activateClass).Draw());

                        yield return ContinuousController.instance.StartCoroutine(
                            card.Owner.AddMemory(1, activateClass));
                    }
                }
            }

            #endregion

            #region Security Effect

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}