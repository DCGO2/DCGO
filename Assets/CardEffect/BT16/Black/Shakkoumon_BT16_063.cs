using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class Shakkoumon_BT16_063 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Rule - Trait: Also has [Angel]
            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Trait: Also has [Angel]", CanUseCondition, card);
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
                        cardSource.CardTraits.Add("Angel");
                    }

                    return CardNames;
                }
            }
            #endregion

            #region DNA Digivolution - Black Lv.4 + Yellow Lv.4: Cost 0
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
                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                            {
                                if (permanent.TopCard.CardColors.Contains(CardColor.Black))
                                {
                                    if (permanent.Levels_ForJogress(card).Contains(4))
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

                        bool PermanentCondition2(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                            {
                                if (permanent.TopCard.CardColors.Contains(CardColor.Yellow))
                                {
                                    if (permanent.Levels_ForJogress(card).Contains(4))
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "a level 4 black Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 4 yellow Digimon"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region When Digivolved
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unaffected by effects of your opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] This Digimon isn't affected by the effects of your opponent's Digimon until the end of their turn. Then, if DNA digivolving, place 1 of your opponent's Digimon whose level is less than or equal to the number of cards in yours or your opponent's security stack at the bottom of your opponent's security stack.";
                }

                #region Unaffected
                bool CardUnaffectedCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card == card.PermanentOfThisCard().TopCard)
                        {
                            if (cardSource == card.PermanentOfThisCard().TopCard)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool SkillCondition(ICardEffect cardEffect)
                {
                    return CardEffectCommons.IsOpponentEffect(cardEffect, card);
                }
                #endregion

                #region Place in security
                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.HasLevel)
                        {
                            if(permanent.Level <= card.Owner.Enemy.SecurityCards.Count)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
                #endregion

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable,card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    if(selectedPermanent != null)
                    {
                        CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                        canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's Digimon's effect", CanUseUnaffectedCondition, card);
                        canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardUnaffectedCondition, SkillCondition: SkillCondition);
                        selectedPermanent.UntilOpponentTurnEndEffects.Add((_timing) => canNotAffectedClass);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(selectedPermanent));

                        bool CanUseUnaffectedCondition(Hashtable hashtable)
                        {
                            return CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent);
                        }
                    }

                    if (CardEffectCommons.IsJogress(hashtable))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place to security.", "The opponent is selecting 1 Digimon to place to security.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    }

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        if (permanent != null)
                        {
                            if (!permanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IPutSecurityPermanent(
                                    permanent,
                                    CardEffectCommons.CardEffectHashtable(activateClass),
                                    true).PutSecurity());
                            }
                        }
                    }
                }
            }
            #endregion

            #region Partition
            if (timing == EffectTiming.None)
            {
                bool CanSelectFirstSourceCondition(CardSource cardSource)
                {
                    return true;
                }

                bool CanSelectSecondSourceCondition(CardSource cardSource)
                {
                    return true;
                }

                cardEffects.Add(CardEffectFactory.PartitionSelfEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null,
                    canSelectFirstSourceCondition: CanSelectFirstSourceCondition,
                    canSelectSecondSourceCondition: CanSelectSecondSourceCondition)
                );
            }
            #endregion

            #region Partition - Inherited
            if (timing == EffectTiming.None)
            {
                bool CanSelectFirstSourceCondition(CardSource cardSource)
                {
                    return true;
                }

                bool CanSelectSecondSourceCondition(CardSource cardSource)
                {
                    return true;
                }

                cardEffects.Add(CardEffectFactory.PartitionSelfEffect(
                    isInheritedEffect: true,
                    card: card,
                    condition: null,
                    canSelectFirstSourceCondition: CanSelectFirstSourceCondition,
                    canSelectSecondSourceCondition: CanSelectSecondSourceCondition)
                );
            }
            #endregion

            return cardEffects;
        }
    }
}