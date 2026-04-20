using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Monarchlizamon/Final Judgment
namespace DCGO.CardEffects.BT25
{
    public class BT25_057 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Effects

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.HasGlowingDawnTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, false, card, null, level: 4));
            }
            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("battle 1 of your opponent's Digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] This Digimon may battle 1 of your opponent's Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanSelectPermanentCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);


                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentCondition))
                    {
                        Permanent selectedPermanent = null;
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
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
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to battle.", "The opponent is selecting 1 Digimon to battle.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(new IBattle(card.PermanentOfThisCard(), selectedPermanent, null, true).Battle());
                    }

                }
            }

            #endregion

            #region WD/WA Shared

            string SharedHashString = "BT25_WD_WA";

            string SharedEffectName = "By trashing bottom face down card under your tamer, De-Digivolve(1) 1 opponent digimon";

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] By trashing the bottom face-down card under any of your Tamers, <De-Digivolve 1> 1 of your opponent's Digimon.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Conditions
                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.HasFaceDownDigivolutionCards;
                }

                bool CanSelectTrashSourceCardCondition(CardSource cardSource)
                {
                    return cardSource.IsFlipped
                        && !cardSource.CanNotTrashFromDigivolutionCards(activateClass);
                }

                bool CanSelectOpponentPermanentCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                #endregion

                bool isUsed = false;

                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanentCondition))
                {
                    Permanent selectedPermanent = null;

                    #region Select Tamer
                    if (CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition) > 1)
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
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
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 tamer to trash a face down card from.", "The opponent is selecting 1 tamer to trash a face down card from.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    }
                    else selectedPermanent = card.Owner.GetBattleAreaPermanents().FirstOrDefault(CanSelectPermanentCondition);
                    #endregion

                    #region Trash Bottom Card
                    List<CardSource> selectedCards = new List<CardSource>();
                    CardSource trashTargetCard = selectedPermanent.DigivolutionCards.Filter(CanSelectTrashSourceCardCondition)[^1];
                    selectedCards.Add(trashTargetCard);

                    yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(selectedPermanent, selectedCards, activateClass).TrashDigivolutionCards());
                    isUsed = true; // Declare true since cost was paid, so OPT will be used up here.
                    #endregion

                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectOpponentPermanentCondition))
                    {
                        #region De-Digivolve 1 Opponent Digimon
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOpponentPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Degenerate,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve(1)", "The opponent is selecting 1 Digimon to De-Digivolve(1).");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        #endregion
                    }
                }
                if (!isUsed) activateClass.RemoveUse();
            }

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: 1,
                hashValue: SharedHashString,
                whenDigivolving: true,
                whenAttacking: true);

            #endregion

            #endregion

            #region Option Effects

            #region Ignore Colour Requirement
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.HasGlowingDawnTraits;
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 of your digimon gains Rush, Sec Atk (+1), and 5K DP, then it may attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[Main] 1 of your Digimon gains <Rush>, <Security A. +1> and +5000 DP for the turn. Then, it may attack.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectPermamentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermamentCondition))
                    {
                        Permanent selectedPermanent = null;

                        #region Select 1 of your Digimon
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, CanSelectPermamentCondition));
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermamentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to gain rush, sec atk(+1) and 5k DP", "The opponent is selecting 1 Digimon to gain rush, sec atk(+1) and 5k DP");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        #endregion

                        if (selectedPermanent != null)
                        {
                            #region Gain Rush, Sec Atk +1, and 5K DP
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRush(
                                    targetPermanent: selectedPermanent,
                                    effectDuration: EffectDuration.UntilEachTurnEnd,
                                    activateClass: activateClass
                            ));

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(
                                    targetPermanent: selectedPermanent,
                                    changeValue: 1,
                                    effectDuration: EffectDuration.UntilEachTurnEnd,
                                    activateClass: activateClass
                            ));

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                    targetPermanent: selectedPermanent,
                                    changeValue: 5000,
                                    effectDuration: EffectDuration.UntilEachTurnEnd,
                                    activateClass: activateClass
                            ));

                            #endregion

                            #region Digimon Attack
                            if (selectedPermanent.CanAttack(activateClass))
                            {
                                SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();
                                selectAttackEffect.SetUp(
                                    attacker: selectedPermanent,
                                    canAttackPlayerCondition: () => true,
                                    defenderCondition: _ => true,
                                    cardEffect: activateClass);
                                selectAttackEffect.SetCanNotSelectNotAttack();
                                yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                            }
                            #endregion
                        }
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