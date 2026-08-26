using System.Collections;
using System.Collections.Generic;
using System;

// RagnaLoardmon
namespace DCGO.CardEffects.ST13
{
    public class ST13_06 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region DNA Requirements
            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect($"DNA Digivolution", CanUseCondition, card);
                addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
                addJogressConditionClass.SetNotShowUI(true);
                cardEffects.Add(addJogressConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                JogressCondition GetJogress(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        bool PermanentCondition1(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && permanent.TopCard.CardColors.Contains(CardColor.Red)
                                && permanent.Levels_ForJogress(card).Contains(6);
                        }

                        bool PermanentCondition2(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && permanent.TopCard.CardColors.Contains(CardColor.Black)
                                && permanent.Levels_ForJogress(card).Contains(6);
                        }

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "a level 6 red Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 6 black Digimon"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Blitz, delete Digimon and trash opponent's Security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] <Blitz> (This Digimon can attack when your opponent has 1 or more memory.) When DNA digivolving, for every 4 cards in this Digimon's digivolution cards, delete 1 of your opponent's Digimon with a play cost of 20 or less and trash the top card of your opponent's security stack.";
                }              

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.GetCostItself <= 20
                        && permanent.TopCard.HasPlayCost;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card)
                        && CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.CanActivateBlitz(card, activateClass))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.BlitzProcess(card, activateClass));

                        int count = card.PermanentOfThisCard().DigivolutionCards.Count / 4;

                        if (CardEffectCommons.IsJogress(_hashtable)
                        && count > 0)
                        {
                            int maxCount = Math.Min(count, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                player: card.Owner.Enemy,
                                destroySecurityCount: count,
                                cardEffect: activateClass,
                                fromTop: true).DestroySecurity());
                        }
                    }            
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("Unsuspend_ ST13_06");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns][Once Per Turn] When a card is removed from a player's security stack, unsuspend this Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, player => true);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass);
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
