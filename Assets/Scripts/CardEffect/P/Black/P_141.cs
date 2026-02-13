using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DCGO.CardEffects.P
{
    public class P_141 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Rule - Name
            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Treated as having [Mamemon]/[Tyrannomon] in its name", CanUseCondition, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: changeCardNames);

                cardEffects.Add(changeCardNamesClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                List<string> changeCardNames(CardSource cardSource, List<string> CardNames)
                {
                    if (cardSource == card)
                    {
                        CardNames.Add("Mamemon");
                        CardNames.Add("Tyrannomon");
                    }

                    return CardNames;
                }
            }
            #endregion

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if ((targetPermanent.TopCard.ContainsCardName("Mamemon") && targetPermanent.Level == 4) || (targetPermanent.TopCard.ContainsCardName("Tyrannomon") && targetPermanent.Level == 4))
                    {
                        return true;
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region Collision
            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.CollisionSelfStaticEffect(false, card, null));
            }
            #endregion

            #region All turns
            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Unsuspend_P_141");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When an opponent's Digimon becomes suspended, you may unsuspend this Digimon.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        if (permanent.IsDigimon)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, PermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard()))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());
                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("UnsuspendESS_P_141");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When an opponent's Digimon becomes suspended, you may unsuspend this Digimon.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        if (permanent.IsDigimon)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, PermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard()))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}