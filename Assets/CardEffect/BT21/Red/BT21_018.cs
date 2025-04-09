using System.Collections;
using System.Collections.Generic;

// DoGatchmon
namespace DCGO.CardEffects.BT21
{
    public class BT21_018 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static effects

            #region Raid

            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region Rush

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RushSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region Link Condition

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasAppmonTraits;
                }
                cardEffects.Add(CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: PermanentCondition, linkCost: 2, card: card));
            }

            #endregion

            #region App Fusion (Gatchmon, Navimon, Tweetmon)

            if (timing == EffectTiming.None)
            {
                AddAppFusionConditionClass addAppFusionConditionClass = new AddAppFusionConditionClass();
                addAppFusionConditionClass.SetUpICardEffect($"App Fusion", (hashtable) => true, card);
                addAppFusionConditionClass.SetUpAddAppFusionConditionClass(getAppFusionCondition: GetAppFusion);
                addAppFusionConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAppFusionConditionClass);

                AppFusionCondition GetAppFusion(CardSource cardSource)
                {
                    string selectLinkMessage = "";
                    string selectDigimonMessage = "";
                    bool linkCondition(CardSource source)
                    {
                        if (source != null)
                        {
                            if (source.PermanentOfThisCard().TopCard.EqualsCardName("Gatchmon"))
                            {
                                if (source.PermanentOfThisCard().LinkedCards.Find(x => x.EqualsCardName("Navimon")))
                                {
                                    selectLinkMessage = "1 [Navimon]";
                                    return true;
                                }
                                if (source.PermanentOfThisCard().LinkedCards.Find(x => x.EqualsCardName("Tweetmon")))
                                {
                                    selectLinkMessage = "1 [Tweetmon]";
                                    return true;
                                }
                            }

                            if (source.PermanentOfThisCard().TopCard.EqualsCardName("Navimon"))
                            {
                                if (source.PermanentOfThisCard().LinkedCards.Find(x => x.EqualsCardName("Gatchmon")))
                                {
                                    selectLinkMessage = "1 [Gatchmon]";
                                    return true;
                                }
                                if (source.PermanentOfThisCard().LinkedCards.Find(x => x.EqualsCardName("Tweetmon")))
                                {
                                    selectLinkMessage = "1 [Tweetmon]";
                                    return true;
                                }
                            }

                            if (source.PermanentOfThisCard().TopCard.EqualsCardName("Tweetmon"))
                            {
                                if (source.PermanentOfThisCard().LinkedCards.Find(x => x.EqualsCardName("Gatchmon")))
                                {
                                    selectLinkMessage = "1 [Gatchmon]";
                                    return true;
                                }
                                if (source.PermanentOfThisCard().LinkedCards.Find(x => x.EqualsCardName("Navimon")))
                                {
                                    selectLinkMessage = "1 [Navimon]";
                                    return true;
                                }
                            }
                        }

                        return false;
                    }
                    bool digimonCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                        {
                            if (permanent.TopCard.EqualsCardName("Gatchmon"))
                            {
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Navimon")))
                                {
                                    selectLinkMessage = "1 [Navimon]";
                                    return true;
                                }
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Tweetmon")))
                                {
                                    selectLinkMessage = "1 [Tweetmon]";
                                    return true;
                                }
                            }

                            if (permanent.TopCard.EqualsCardName("Navimon"))
                            {
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Gatchmon")))
                                {
                                    selectLinkMessage = "1 [Gatchmon]";
                                    return true;
                                }
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Tweetmon")))
                                {
                                    selectLinkMessage = "1 [Tweetmon]";
                                    return true;
                                }
                            }

                            if (permanent.TopCard.EqualsCardName("Tweetmon"))
                            {
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Gatchmon")))
                                {
                                    selectLinkMessage = "1 [Gatchmon]";
                                    return true;
                                }
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Navimon")))
                                {
                                    selectLinkMessage = "1 [Navimon]";
                                    return true;
                                }
                            }
                        }

                        return false;
                    }

                    if (cardSource == card)
                    {
                        AppFusionCondition AppFusionCondition = new AppFusionCondition(
                            linkCondition,
                            selectLinkMessage,
                            digimonCondition,
                            selectDigimonMessage,
                            0);

                        return AppFusionCondition;
                    }

                    return null;
                }
            }

            #endregion

            #endregion

            #region Your Turn

            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This digimon may attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("WhenLinked_BT21_018");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[Your Turn] [Once Per Turn] When this Digimon gets linked, it may attack.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenLinking(hashtable, PermanentCondition, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (card.PermanentOfThisCard().CanAttack(activateClass))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().CanAttack(activateClass))
                        {
                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                            selectAttackEffect.SetUp(
                                attacker: card.PermanentOfThisCard(),
                                canAttackPlayerCondition: () => true,
                                defenderCondition: (permanent) => true,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                        }
                    }
                }
            }

            #endregion

            #region Link ESS

            #region When Linked

            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This digimon may attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsLinkedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[When Linking] this Digimon may attack.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenLinking(hashtable, PermanentCondition, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (card.PermanentOfThisCard().TopCard.PermanentOfThisCard().CanAttack(activateClass))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard().TopCard.PermanentOfThisCard();
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().TopCard.PermanentOfThisCard().CanAttack(activateClass))
                        {
                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                            selectAttackEffect.SetUp(
                                attacker: card.PermanentOfThisCard().TopCard.PermanentOfThisCard(),
                                canAttackPlayerCondition: () => true,
                                defenderCondition: (permanent) => true,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                        }
                    }
                }
            }

            #endregion

            #region +3k DP

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 3000, isInheritedEffect: false, card: card, condition: null, isLinkedEffect: true));
            }

            #endregion

            #endregion

            return cardEffects;
        }
    }
}