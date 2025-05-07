using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT20
{
    //Regular Jesmon
    public class BT20_017 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                #region <OnPlay>
                {
                    ActivateClass activateClass = new ActivateClass();
                    activateClass.SetUpICardEffect("Play a token", CanUseCondition, card);
                    activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                    cardEffects.Add(activateClass);

                    string EffectDiscription()
                    {
                        return "[On Play] Play 1 [Atho, René & Por] Token. (Digimon/White/6000 DP/<Reboot>/<Blocker>/<Decoy Red/Black>)";
                    }

                    bool CanUseCondition(Hashtable hashtable)
                    {
                        return (CardEffectCommons.CanTriggerOnPlay(hashtable, card));
                    }

                    bool CanActivateCondition(Hashtable hashtable)
                    {
                        if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        {
                            if (card.Owner.fieldCardFrames.Count((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame()) >= 1)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    IEnumerator ActivateCoroutine(Hashtable hashtable)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayAthoRenePorToken(activateClass));
                    }
                }
                #endregion

                #region <WhenDigivovling>
                {
                    ActivateClass activateClass = new ActivateClass();
                    activateClass.SetUpICardEffect("Play token", CanUseCondition, card);
                    activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                    cardEffects.Add(activateClass);

                    string EffectDiscription()
                    {
                        return "[When Digivolving] Play 1 [Atho, René & Por] Token. (Digimon/White/6000 DP/<Reboot> <Blocker> <Decoy Red/Black>)";
                    }

                    bool CanUseCondition(Hashtable hashtable)
                    {
                        return (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card));
                    }

                    bool CanActivateCondition(Hashtable hashtable)
                    {
                        if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        {
                            if (card.Owner.fieldCardFrames.Count((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame()) >= 1)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    IEnumerator ActivateCoroutine(Hashtable hashtable)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayAthoRenePorToken(activateClass));
                    }
                }
                #endregion
            }


            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                #region Your Turn
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 8k DP or less, Then 1 Digimon may attack.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("PlayLevel6_BT20_017");
                cardEffects.Add(activateClass);

                string EffectDiscription ()
                {
                    return "[Your Turn] [Once Per Turn] When any of your other Digimon are played, delete 1 of your opponent's Digimon with 8000 DP or less. Then, 1 of you Digimon may attack.";
                }

                bool PermanentCondition (Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent != card.PermanentOfThisCard();
                }

                bool CanUseCondition (Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition (Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanSelectDeletePermanentCondition (Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(8000, activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectAttackPermanentCondition (Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.CanAttack(activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine (Hashtable hashtable)
                {
                    List<Permanent> selectedPermanents = new List<Permanent>();

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDeletePermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectDeletePermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDeletePermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "Opponent is selecting one Digimon to delete.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectAttackPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectAttackPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectAttackPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will attack.", "Opponent is selecting on Digimon that will attack.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine (List<Permanent> permanents)
                        {
                            selectedPermanents = permanents;
                            yield return null;
                        }

                        foreach (Permanent permanent in selectedPermanents)
                        {
                            Permanent selectedPermanent = permanent;

                            if (selectedPermanent != null)
                            {
                                if (selectedPermanent.CanAttack(activateClass))
                                {
                                    SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                    selectAttackEffect.SetUp(
                                        attacker: selectedPermanent,
                                        canAttackPlayerCondition: () => true,
                                        defenderCondition: (permanent) => true,
                                        cardEffect: activateClass);

                                    yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                                }
                            }
                        }

                    }
                }
                #endregion
            }

            return cardEffects;
        }
    }
}