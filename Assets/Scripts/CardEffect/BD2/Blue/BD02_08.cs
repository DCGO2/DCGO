using System.Collections;
using System.Collections.Generic;

// Valner Dragnogh
namespace DCGO.CardEffects.BD02
{
    public class BD02_08 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared PermanentCondition
            bool PermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.HasPermanentColor(CardColor.Blue)
                    && permanent.TopCard.EqualsTraits("ALYSIAN");
            }
            #endregion

            #region Start of Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                cardEffects.Add(CardEffectFactory.Gain1MemoryTamerOwnerDigimonConditionalEffect("IF you have a blue Digimon with the [ALYSIAN] trait, gain 1 memory", PermanentCondition, null, card));
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspending this Tamer, 1 of your blue [ALYSIAN] Digimon gains <Piercing>", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT27_010_Main");
                cardEffects.Add(activateClass);

                string EffectDescription() => "[Main] By suspending this Tamer, 1 of your blue Digimon with the [ALYSION] trait gains <Piercing> for the turn. (When this Digimon deletes your opponent's Digimon in battle while attacking, it checks security before the attack ends.)";

                bool CanUseCondition(Hashtable hashtable) 
                    => CardEffectCommons.IsExistOnBattleArea(card)
                    && CardEffectCommons.CanActivateSuspendCostEffect(card)
                    && CardEffectCommons.IsOwnerTurn(card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                        new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass,
                        SuccessProcess,
                        null));

                    IEnumerator SuccessProcess(List<Permanent> suspendedPermaments)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: PermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will gain Pierce.", "The opponent is selecting 1 Digimon that will gain Pierce.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainPierce(targetPermanent: permanent, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                            }
                        }
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}
