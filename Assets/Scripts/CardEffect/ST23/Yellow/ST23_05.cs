using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Gekkomon
namespace DCGO.CardEffects.ST23
{
    public class ST23_05 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Glowing Dawn");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 5, permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region shared WD / WA

            string SharedHashString = "ST23-05_WD_WA";

            string SharedEffectName = "Place 1 opponent's lowest DP Digimon on top of security. Then may trash top security from the player with most security to Recover +1";

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] Place 1 of your opponent's lowest DP Digimon as the top security card. Then, by trashing the top security card of 1 player with the most security cards, <Recovery +1>.";

            CardEffectFactory.ActivateClassesForSharedEffects(ref cardEffects, timing, card,
                                                              SharedEffectName,
                                                              SharedActivateCoroutine,
                                                              SharedEffectDescription,
                                                              false,
                                                              isOptionalFunction: IsOptional,
                                                              maxCountPerTurn: 1,
                                                              hashValue: SharedHashString,
                                                              whenDigivolving: true,
                                                              whenAttacking: true
                                                              );

            bool IsOptional(Hashtable hashtable) => !CardEffectCommons.HasMatchConditionPermanent(IsWeakestEnemyDigimon);

            bool IsWeakestEnemyDigimon(Permanent permanent) => CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (card.Owner.CanAddSecurity(activateClass))
                {
                    int validTargets = CardEffectCommons.MatchConditionOpponentsPermanentCount(card, IsWeakestEnemyDigimon);
                    if (validTargets == 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IPutSecurityPermanent(
                                card.Owner.Enemy.GetBattleAreaDigimons().Filter(IsWeakestEnemyDigimon).FirstOrDefault(), 
                                CardEffectCommons.CardEffectHashtable(activateClass), true, false).PutSecurity());
                    }
                    else if (validTargets > 1)
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
    
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsWeakestEnemyDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.PutSecurityTop,
                            cardEffect: activateClass);
    
                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place in security.", "The opponent is selecting 1 Digimon to place in security.");
    
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
                bool validOwnerSecurity = card.Owner.SecurityCards.Count > 0 && card.Owner.SecurityCards.Count >= card.Owner.Enemy.SecurityCards.Count;
                bool validEnemySecurity = card.Owner.Enemy.SecurityCards.Count > 0 && card.Owner.Enemy.SecurityCards.Count >= card.Owner.SecurityCards.Count;

                if (validOwnerSecurity || validEnemySecurity)
                {
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                    if (validOwnerSecurity)
                    {
                        selectionElements.Add(new (message: $"Trash your own top security card", value : 1, spriteIndex: 0));
                    }
                    if (validEnemySecurity)
                    {
                        selectionElements.Add(new (message: $"Trash your opponent's top security card", value : 2, spriteIndex: 0));
                    }
                    selectionElements.Add( new (message: $"Don't trash security", value: 3, spriteIndex: 1));

                    string selectPlayerMessage = "Will you trash 1 player's security to <Recover +1>?";
                    string notSelectPlayerMessage = "The opponent is choosing if they will trash a security.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool doTrash = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                    bool ownSecurity = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                    if (doTrash)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashSecurityAndProcessAccordingToResult(
                            player: ownSecurity ? card.Owner : card.Owner.Enemy,
                            trashAmount: 1,
                            activateClass: activateClass,
                            fromTop: true,
                            successProcess: SuccessProcess,
                            failureProcess: null
                        ));

                        IEnumerator SuccessProcess(List<CardSource> cardSources)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                        }
                    }
                }
            }
            #endregion

            #region All Turns Protect Glowing Dawn
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing top security, card doesn't leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("ST23_15_AT_Protect");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When any of your [Glowing Dawn] trait Digimon would leave the battle area, by trashing your top security card, they don't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, PermanentCondition);
                        
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.Owner.SecurityCards.Count > 0;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsTraits("Glowing Dawn");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource secCard = card.Owner.SecurityCards[0];

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashSecurityAndProcessAccordingToResult(
                        player: card.Owner,
                        trashAmount: 1,
                        activateClass: activateClass,
                        fromTop: true,
                        successProcess: SuccessProcess,
                        failureProcess: null
                    ));

                    IEnumerator SuccessProcess(List<CardSource> cardSources)
                    {
                        List<Permanent> protectedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable)
                                    .Filter(PermanentCondition);

                        foreach (Permanent permanent in protectedPermanents)
                        {
                            permanent.willBeRemoveField = false;
                            permanent.HideDeleteEffect();
                            permanent.HideHandBounceEffect();
                            permanent.HideDeckBounceEffect();
                            permanent.HideWillRemoveFieldEffect();
                        }

                        yield return null;
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
