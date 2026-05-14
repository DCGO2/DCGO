using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Marsmon
namespace DCGO.CardEffects.BT25
{
    public class BT25_020 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.EqualsTraits("TS");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, true, card, null, level: 5));
            }
            #endregion

            #region Reduce Play Cost
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.HasMatchConditionPermanent(WouldPlayCondition);
                }

                bool WouldPlayCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleArea(permanent)
                        && permanent.HasDP
                        && permanent.DP >= 13000;
                }

                cardEffects.Add(CardEffectFactory.MandatorySelfPlayCostReduction(5, card, Condition));
            }
            #endregion

            #region Shared OP / WD / WA

            string SharedEffectName = "+3K DP to one of your digimon, 1 of your Digimon may battle an opponent's Digimon";

            string SharedEffectDescription(string tag) => $"[{tag}] 1 of your Digimon gets +3000 DP for the turn. Then, 1 of your Digimon may battle 1 of your opponent's Digimon.";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    onPlay: true,
                    whenDigivolving: true,
                    whenAttacking: true);

            bool IsYourDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

            bool IsEnemyDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsYourDigimon))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsYourDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP +3000.", "The opponent is selecting 1 Digimon that will get DP +3000.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: 3000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                    }

                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsEnemyDigimon))
                    {
                        //Select attacker
                        Permanent selectedAttacker = null;

                        SelectPermanentEffect selectAttackerEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectAttackerEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsYourDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectAttackerCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectAttackerEffect.SetUpCustomMessage("Select 1 of your Digimon to battle.", "The opponent is selecting Digimon to battle.");

                        yield return ContinuousController.instance.StartCoroutine(selectAttackerEffect.Activate());

                        IEnumerator SelectAttackerCoroutine(Permanent permanent)
                        {
                            selectedAttacker = permanent;

                            yield return null;
                        }

                        if (selectedAttacker != null)
                        {
                            //select defender
                            Permanent selectedDefender = null;

                            SelectPermanentEffect selectDefenderEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectDefenderEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsEnemyDigimon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectDefenderCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectDefenderEffect.SetUpCustomMessage("Select 1 enemy Digimon to battle.", "The opponent is selecting Digimon to battle.");

                            yield return ContinuousController.instance.StartCoroutine(selectDefenderEffect.Activate());

                            IEnumerator SelectDefenderCoroutine(Permanent permanent)
                            {
                                selectedDefender = permanent;

                                yield return null;
                            }

                            if (selectedDefender != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IBattle(selectedAttacker, selectedDefender, null, true).Battle());
                            }
                        }
                    }
                }
            }

            #endregion

            #region All Turns
            if (timing == EffectTiming.OnEndBattle)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash opponent's top security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT25_020_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When any of your [TS] trait Digimon win a battle, trash your opponent's top security card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        bool WinnerCondition(Permanent permanent) => permanent.TopCard.Owner == card.Owner && permanent.TopCard.HasTSTraits;

                        if (CardEffectCommons.CanTriggerWhenWinBattle(
                            hashtable: hashtable,
                            winnerCondition: WinnerCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
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
