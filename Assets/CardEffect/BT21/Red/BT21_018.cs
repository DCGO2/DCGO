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

            #region Alternative Digivolution Condition - Stnd.
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Stnd.");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

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
                    bool linkCondition(Permanent permanent, CardSource source)
                    {
                        if (source != null && source != card)
                        {
                            if (permanent.TopCard.EqualsCardName("Gatchmon"))
                            {
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Navimon") || x.EqualsCardName("Tweetmon")))
                                {
                                    return true;
                                }
                            }

                            if (permanent.TopCard.EqualsCardName("Navimon"))
                            {
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Gatchmon") || x.EqualsCardName("Tweetmon")))
                                {
                                    return true;
                                }
                            }

                            if (permanent.TopCard.EqualsCardName("Tweetmon"))
                            {
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Gatchmon") || x.EqualsCardName("Navimon")))
                                {
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
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Navimon") || x.EqualsCardName("Tweetmon")))
                                {
                                    return true;
                                }
                            }

                            if (permanent.TopCard.EqualsCardName("Navimon"))
                            {
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Gatchmon") || x.EqualsCardName("Tweetmon")))
                                {
                                    return true;
                                }
                            }

                            if (permanent.TopCard.EqualsCardName("Tweetmon"))
                            {
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Gatchmon") || x.EqualsCardName("Navimon")))
                                {
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
                            digimonCondition,
                            0);

                        return AppFusionCondition;
                    }

                    return null;
                }
            }

            #endregion

            #region Link
            if (timing == EffectTiming.OnDeclaration)
            {
                /// <summary>
                /// Used to link a card
                /// </summary>
                /// <param name="card">Reference to this card</param>
                /// <param name="condition">OPTIONAL - Function to check for effect conditions</param>
                /// <author>Mike Bunch</author>
                cardEffects.Add(CardEffectFactory.LinkEffect(card));
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
                        if (CardEffectCommons.CanTriggerWhenLinked(hashtable, LinkPermanentCondition, null))
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

                bool LinkPermanentCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
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

            #endregion

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

            return cardEffects;
        }
    }
}