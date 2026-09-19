using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX4
{
    public class EX4_062 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Also treated as [Kiriha Aonuma]", CanUseCondition, card);
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
                        CardNames.Add("Kiriha Aonuma");
                        CardNames.Add("KirihaAonuma");
                    }

                    return CardNames;
                }
            }

            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Also treated as [Nene Amano]", CanUseCondition, card);
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
                        CardNames.Add("Nene Amano");
                        CardNames.Add("NeneAmano");
                    }

                    return CardNames;
                }
            }

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] If there are 2 or more Digimon in play, gain 1 memory.";
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
                        if (card.Owner.GetBattleAreaDigimons().Count + card.Owner.Enemy.GetBattleAreaDigimons().Count >= 2)
                        {
                            if (card.Owner.CanAddMemory(activateClass))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Can select DigiXros cards from Tamer's digivolution cards from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("CanSelectDigiXrosFromTamer_EX4_062");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When you would play 1 Digimon card with the [Blue Flare] or [Twilight] trait with DigiXros requirements, by suspending this Tamer, you may place 1 card from under your Tamers and 1 card from your trash as digivolution cards for a DigiXros.";
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource.Owner == card.Owner)
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (cardSource.HasDigiXros)
                            {
                                if (cardSource.CardTraits.Contains("Blue Flare") || cardSource.CardTraits.Contains("BlueFlare"))
                                {
                                    return true;
                                }

                                if (cardSource.CardTraits.Contains("Twilight"))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition))
                        {
                            if (CardEffectCommons.IsOnly1CardPlayed(hashtable))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanActivateSuspendCostEffect(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (!card.PermanentOfThisCard().IsSuspended && card.PermanentOfThisCard().CanSuspend)
                        {
                            Hashtable hashtable = new Hashtable();
                            hashtable.Add("CardEffect", activateClass);

                            yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, hashtable).Tap());

                            ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                            AddMaxUnderTamerCountDigiXrosClass addMaxTamerCountDigiXrosClass = new AddMaxUnderTamerCountDigiXrosClass();
                            addMaxTamerCountDigiXrosClass.SetUpICardEffect("Can select DigiXros cards from Tamer's digivolution cards", CanUseCondition1, card);
                            addMaxTamerCountDigiXrosClass.SetUpAddMaxUnderTamerCountDigiXrosClass(getMaxUnderTamerCount: GetCount);
                            card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => addMaxTamerCountDigiXrosClass);

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                return true;
                            }

                            int GetCount(CardSource cardSource)
                            {
                                if (CardSourceCondition(cardSource))
                                {
                                    return 1;
                                }

                                return 0;
                            }

                            bool CardSourceCondition(CardSource cardSource)
                            {
                                if (cardSource != null)
                                {
                                    if (cardSource.Owner == card.Owner)
                                    {
                                        if (cardSource.HasDigiXros)
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
                            }

                            AddMaxTrashCountDigiXrosClass addMaxTrashCountDigiXrosClass = new AddMaxTrashCountDigiXrosClass();
                            addMaxTrashCountDigiXrosClass.SetUpICardEffect("Can select DigiXros cards from trash", CanUseCondition1, card);
                            addMaxTrashCountDigiXrosClass.SetUpAddMaxTrashCountDigiXrosClass(getMaxTrashCount: GetCount);
                            card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => addMaxTrashCountDigiXrosClass);
                        }
                    }
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            return cardEffects;
        }
    }
}