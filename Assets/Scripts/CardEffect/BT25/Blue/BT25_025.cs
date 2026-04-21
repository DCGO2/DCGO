using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Aegiochusmon: Blue
namespace DCGO.CardEffects.BT25
{
    public class BT25_025 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Aegiomon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, false, card, null));
            }
            #endregion

            #region Decode <[Aegiomon]>

            if (timing == EffectTiming.WhenRemoveField)
            {
                bool SourceCondition(CardSource source)
                {
                    return source.EqualsCardName("Aegiomon");
                }

                string[] decodeStrings = { "(Aegiomon)", "Aegiomon" };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(card: card, isInheritedEffect: false, decodeStrings: decodeStrings, sourceCondition: SourceCondition, condition: null));
            }

            #endregion

            #region Shared OP / WD

            string SharedEffectName = "De-Digivovle(1) 1 opponent digimon, then if 3 or less security, 1 of your digimon unsuspends";

            string SharedEffectDescription(string tag) => $"[{tag}] <De-Digivolve 1> 1 of your opponent's Digimon. Then, if you have 3 or fewer security cards, 1 of your Digimon unsuspends.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Conditions

                bool isOpponentDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                bool isYourDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                #endregion

                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, isOpponentDigimon))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, isOpponentDigimon));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: isOpponentDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Degenerate,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 opponent's Digimon to De-Digivolve", "The opponent is selecting 1 Digimon to De-Digivolve");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

                if (card.Owner.SecurityCards.Count <= 3 && CardEffectCommons.HasMatchConditionOwnersPermanent(card, isYourDigimon))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, isYourDigimon));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: isYourDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.UnTap,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to unsuspend", "The opponent is selecting 1 Digimon to unsuspend");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    maxCountPerTurn: -1,
                    onPlay: true,
                    whenDigivolving: true);

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend 1 [Shaman] trait digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("BT25-025_AT");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns][Once Per Turn] When your security stack is removed from, 1 of your [Shaman] trait Digimon may unsuspend.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, player => player == card.Owner))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsShamanDigimon);
                }

                bool IsShamanDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasShamanTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool isUsed = false;
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsShamanDigimon))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, IsShamanDigimon));
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsShamanDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.UnTap,
                            cardEffect: activateClass);


                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> selectedPermanents)
                        {
                            if (selectedPermanents.Any()) isUsed = true;
                            yield return null;

                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 [Shaman] trait Digimon to unsuspend", "The opponent is selecting 1 [Shaman] trait Digimon to unsuspend");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                    if (!isUsed) activateClass.RemoveUse();
                }
            }

            #endregion

            return cardEffects;
        }
    }
}