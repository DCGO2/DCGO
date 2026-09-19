using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.Examples
{
    public class Link_Examples : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Link Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasAppmonTraits;
                }

                /// <summary>
                /// Used to add a link condition to itself
                /// </summary>
                /// <param name="permanentCondition">Function to check for target permanent conditions</param>
                /// <param name="linkCost">Cost to perform link</param>
                /// <param name="card">Reference to this card</param>
                /// <param name="condition">OPTIONAL - Function to check for effect conditions</param>
                /// <param name="cardCondition">OPTIONAL - Function to check for cards conditions</param>
                /// <param name="effectName">OPTIONAL - name to show for effect (default "Link")</param>
                /// <author>Mike Bunch</author>
                cardEffects.Add(CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: PermanentCondition, linkCost: 0, card: card));
            }

            #endregion

            #region Basic link effect keyword
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

            #region When Linked Example, Specifically When THIS card is added as a linked card
            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Testing When Linking", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsLinkedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When linked] Testing When linking";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenLinking(hashtable, null, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return isExistOnField(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    UnityEngine.Debug.Log("CARD SUCCESSFULLY LINKED");
                    yield return null;
                }
            }
            #endregion

            #region When Linked Example, When A card is linked at all
            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Testing When Linked", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When this Digimon gets linked";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                bool CardCondition(CardSource source)
                {
                    return true;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenLinked(hashtable, PermanentCondition, CardCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return isExistOnField(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    UnityEngine.Debug.Log("CARD LINKED");
                    yield return null;
                }
            }
            #endregion

            #region Trash 1 Link card
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent))
                    {
                        if (permanent.LinkedCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return true;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
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
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashLinkedCards(
                            permanentCondition: CanSelectPermanentCondition,
                            cardCondition: CanSelectCardCondition,
                            maxCount: 1,
                            canNoTrash: false,
                            isFromOnly1Permanent: true,
                            activateClass: activateClass
                        ));
                }
            }
            #endregion

            #region Change Link Max Count
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfLinkMaxStaticEffect(
                    changeValue: 1,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition));
            }
            #endregion

            #region App Fusion

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
                        if (source != null)
                        {
                            if (source != card)
                            {
                                if (permanent.TopCard.EqualsCardName("First Name"))
                                {
                                    if (permanent.LinkedCards.Find(x => x.EqualsCardName("Second Name")))
                                    {
                                        return true;
                                    }
                                }
                                if (permanent.TopCard.EqualsCardName("Second Name"))
                                {
                                    if (permanent.LinkedCards.Find(x => x.EqualsCardName("First Name")))
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

                        return false;
                    }
                    bool digimonCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                        {
                            if (permanent.TopCard.EqualsCardName("First Name"))
                            {
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Second Name")))
                                {
                                    return true;
                                }
                            }
                            if (permanent.TopCard.EqualsCardName("Second Name"))
                            {
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("First Name")))
                                {
                                    return true;
                                }
                            }
                            return false;
                        }

                        return false;
                    }

                    if (cardSource == card)
                    {
                        AppFusionCondition AppFusionCondition = new AppFusionCondition(
                            linkedCondition: linkCondition,
                            digimonCondition: digimonCondition,
                            cost: 0);

                        return AppFusionCondition;
                    }

                    return null;
                }
            }

            #endregion

            return cardEffects;
        }
    }
}