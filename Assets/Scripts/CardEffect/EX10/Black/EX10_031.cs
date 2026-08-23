using System.Collections;
using System.Collections.Generic;
using System.Linq;

//DarkKnightmon
namespace DCGO.CardEffects.EX10
{
    public class EX10_031 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasText("Knightmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 4));
            }
            #endregion

            #region Digixros
            if (timing == EffectTiming.None)
            {
                AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
                addDigiXrosConditionClass.SetUpICardEffect($"DigiXros", CanUseCondition, card);
                addDigiXrosConditionClass.SetUpAddDigiXrosConditionClass(getDigiXrosCondition: GetDigiXros);
                addDigiXrosConditionClass.SetNotShowUI(true);
                cardEffects.Add(addDigiXrosConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                DigiXrosCondition GetDigiXros(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        DigiXrosConditionElement element = new DigiXrosConditionElement(CanSelectCardCondition, "SkullKnightmon");

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            return cardSource != null
                                && cardSource.Owner == card.Owner
                                && cardSource.CardNames_DigiXros.Contains("SkullKnightmon");
                        }

                        DigiXrosConditionElement element1 = new DigiXrosConditionElement(CanSelectCardCondition1, "DeadlyAxemon");

                        bool CanSelectCardCondition1(CardSource cardSource)
                        {
                            return cardSource != null
                                && cardSource.Owner == card.Owner
                                && cardSource.CardNames_DigiXros.Contains("DeadlyAxemon");
                        }

                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>() { element, element1 };

                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, null, 1);

                        return digiXrosCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "Gain immunity to De-Digivolve & gain 3k DP";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] Until your opponent's turn ends, their <De-Digivolve> effects don't affect 1 of your Digimon, and it gets +3000 DP.";
            }
            bool SharedPermamentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent selectedPermament = null;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: SharedPermamentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    selectedPermament = permanent;
                    yield return null;
                }

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to gain De-Digivolve immunity & 3K DP", "The opponent is selecting 1 Digimon to gain De-Digivolve immunity & 3K DP");
                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                if (selectedPermament != null)
                {
                    bool CanUseImmunityCondition(Hashtable hashtable1)
                    {
                        return selectedPermament.TopCard != null;
                    }

                    bool PermanentImmunityCondition(Permanent permanent)
                    {
                        return permanent == selectedPermament;
                    }

                    ImmuneFromDeDigivolveClass immuneFromDeDigivolveClass = new ImmuneFromDeDigivolveClass();
                    immuneFromDeDigivolveClass.SetUpICardEffect("Isn't affected by <De-Digivolve>", CanUseImmunityCondition, selectedPermament.TopCard);
                    immuneFromDeDigivolveClass.SetUpImmuneFromDeDigivolveClass(PermanentCondition: PermanentImmunityCondition);
                    selectedPermament.UntilOpponentTurnEndEffects.Add((_timing) => immuneFromDeDigivolveClass);

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                        targetPermanent: selectedPermament,
                        changeValue: 3000,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass
                    ));
                }
            }
            #endregion

            #region All Turns - OPT
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play a Digimon from digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("PlayDigimon_EX10_031");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon would leave the battle area, you may play 1 play cost 4 or lower card from its digivolution cards without paying the cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 4
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.DigivolutionCards);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                        canTargetCondition: CanSelectCardCondition,
                        SelectCardEffect.Root.DigivolutionCards,
                        activateClass,
                        payCost: false,
                        targetPermanent: card.PermanentOfThisCard()
                    ));
                }
            }
            #endregion

            #region ESS
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Change attack target to this card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT22_061_ChangeAttackTarget");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Opponent's Turn] [Once Per Turn] When one of your opponent's Digimon attacks, you may change the attack target to this Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOpponentTurn(card)
                        && CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, IsOpponentDigimon);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CanBeSwitched();
                }

                bool IsOpponentDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool CanBeSwitched()
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(GManager.instance.attackProcess.AttackingPermanent, card)
                        && GManager.instance.attackProcess.IsAttacking
                        && GManager.instance.attackProcess.AttackingPermanent.CanSwitchAttackTarget;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SwitchDefender(activateClass, false, card.PermanentOfThisCard()));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}