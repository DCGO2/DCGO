using System.Collections;
using System.Collections.Generic;

// Shaujinmon
namespace DCGO.CardEffects.BT27
{
    public class BT27_028 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution - Lv.4 [DATA SQUAD], cost 3
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("DATA SQUAD");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 4));
            }
            #endregion

            #region Decode (main)
            if (timing == EffectTiming.WhenRemoveField)
            {
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(
                    card: card,
                    isInheritedEffect: false,
                    decodeStrings: DecodeStrings(),
                    sourceCondition: CanDecodeSource,
                    condition: null));
            }
            #endregion

            #region On Play / When Digivolving - opponent cannot suspend
            string SharedEffectName = "1 opponent Digimon or Tamer can't suspend";

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects,
                timing,
                card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] 1 of your opponent's Digimon or Tamers can't suspend until their turn ends.";
            }

            bool CanSelectOpponentPermanent(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanent))
                    yield break;

                Permanent selectedPermanent = null;
                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectOpponentPermanent,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage(
                    "Select 1 opponent Digimon or Tamer that can't suspend.",
                    "The opponent is selecting a Digimon or Tamer that can't suspend.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    selectedPermanent = permanent;
                    yield return null;
                }

                if (selectedPermanent != null)
                {
                    CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                    canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCondition, card);
                    canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCondition);
                    selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => canNotSuspendClass);

                    yield return ContinuousController.instance.StartCoroutine(
                        GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));

                    bool CanUseCondition(Hashtable effectHashtable)
                    {
                        return selectedPermanent.TopCard != null
                            && !selectedPermanent.TopCard.CanNotBeAffected(activateClass);
                    }

                    bool PermanentCondition(Permanent permanent) => permanent == selectedPermanent;
                }
            }
            #endregion

            #region Decode (inherited)
            if (timing == EffectTiming.WhenRemoveField)
            {
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(
                    card: card,
                    isInheritedEffect: true,
                    decodeStrings: DecodeStrings(),
                    sourceCondition: CanDecodeSource,
                    condition: null));
            }
            #endregion

            return cardEffects;
        }

        private static bool CanDecodeSource(CardSource source)
        {
            return source.IsDigimon
                && source.HasLevel
                && source.Level <= 4
                && (source.EqualsTraits("Aquatic") || source.EqualsTraits("DATA SQUAD"));
        }

        private static string[] DecodeStrings()
        {
            return new[] {
                "(Lv.4 or lower w/[Aquatic]/[DATA SQUAD] trait)",
                "Level 4 or lower Digimon card with [Aquatic] or [DATA SQUAD] trait"
            };
        }
    }
}
