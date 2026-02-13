using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT20
{
    public class BT20_045 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region DNA Digivolution

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
                        bool PermanentConditionA(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                                   permanent.TopCard.CardColors.Contains(CardColor.Green) &&
                                   permanent.Levels_ForJogress(card).Contains(6);
                        }

                        bool PermanentConditionB(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                                   permanent.TopCard.CardColors.Contains(CardColor.Blue) &&
                                   permanent.Levels_ForJogress(card).Contains(6);
                        }

                        JogressConditionElement[] elements =
                        {
                            new(PermanentConditionA, "a level 6 Green Digimon"),
                            new(PermanentConditionB, "a level 6 Blue Digimon")
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }

            #endregion

            #region Ace - Blast DNA Digivolve

            if (timing == EffectTiming.OnCounterTiming)
            {
                List<BlastDNACondition> blastDNAConditions = new List<BlastDNACondition>
                {
                    new("Breakdramon"),
                    new("Slayerdramon")
                };
                cardEffects.Add(CardEffectFactory.BlastDNADigivolveEffect(card: card,
                    blastDNAConditions: blastDNAConditions, condition: null));
            }

            #endregion

            #region Raid

            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #region Piercing

            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
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

            #region Evade

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.EvadeSelfEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return opponent's Digimon with the highest DP to the bottom of deck",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] If DNA digivolving, return all of your opponent's Digimon with the highest DP to the bottom of the deck.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsMaxDP(permanent, card.Owner.Enemy, null);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsJogress(hashtable) &&
                           card.Owner.Enemy.GetBattleAreaDigimons().Count > 0;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> bounceTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(CanSelectPermanentCondition);

                    yield return ContinuousController.instance.StartCoroutine(
                        new DeckBottomBounceClass(bounceTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass))
                            .DeckBounce());
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("Unsuspend_BT20_045");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] (Once Per Turn) When any Digimon suspend, this Digimon may unsuspend.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, PermanentCondition);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) && CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard());
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}