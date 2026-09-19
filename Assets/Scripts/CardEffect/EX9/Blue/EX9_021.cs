using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

//Omnimon Alter-S
namespace DCGO.CardEffects.EX9
{
    public class EX9_021 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            //Lv.6 w / [DM] trait: Cost 5
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasDMTraits && targetPermanent.TopCard.IsLevel6;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 5, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region DNA Digivolution

            //Blue Lv.6 + Red Lv.6 : Cost 0
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
                                if (permanent.TopCard.CardColors.Contains(CardColor.Blue))
                                {
                                    if (permanent.Levels_ForJogress(card).Contains(6))
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
                                if (permanent.TopCard.CardColors.Contains(CardColor.Red))
                                {
                                    if (permanent.Levels_ForJogress(card).Contains(6))
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "a level 6 blue Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 6 red Digimon"),
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
                activateClass.SetUpICardEffect("If DNA, unaffected by opponents effect, Delete Digimon with highest level.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If DNA digivolving, your opponent's effects don't affect this Digimon for the turn. Then, delete all of their Digimon with the highest level.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsMaxLevel(permanent, card.Owner.Enemy);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsJogress(_hashtable))
                    {
                        Permanent selectedPermanent = card.PermanentOfThisCard();

                        CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                        canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's effect", CanUseCondition1, card);
                        canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                        selectedPermanent.UntilEachTurnEndEffects.Add((_timing) => canNotAffectedClass);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(selectedPermanent));

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            if (selectedPermanent.TopCard != null)
                            {
                                return true;
                            }

                            return false;
                        }

                        bool CardCondition(CardSource cardSource)
                        {
                            if (selectedPermanent.TopCard != null)
                            {
                                if (selectedPermanent.TopCard.Owner.GetBattleAreaPermanents().Contains(selectedPermanent))
                                {
                                    if (cardSource == selectedPermanent.TopCard)
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

                        bool SkillCondition(ICardEffect cardEffect)
                        {
                            if (cardEffect != null)
                            {
                                if (cardEffect.EffectSourceCard != null)
                                {
                                    if (cardEffect.EffectSourceCard.Owner == card.Owner.Enemy)
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }
                    }                    

                    List<Permanent> destroyTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(PermanentCondition);

                    if (card.Owner.Enemy.GetBattleAreaDigimons().Count > 0)
                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(destroyTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                }
            }
            #endregion

            #region End of Attack
            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 2 sources, place on top of security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Attack] You may play 1 card with [Greymon] in its name or the [Ver.1] trait and 1 card with [Garurumon] in its name or the [Ver.2] trait from this Digimon's digivolution cards without paying the costs. If this effect played, place this Digimon as your top security card.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (cardSource.HasGreymonName || cardSource.EqualsTraits("Ver.1"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (cardSource.HasGarurumonName || cardSource.EqualsTraits("Ver.2"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnEndAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => CanSelectCardCondition(cardSource)) >= 1)
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: (cardSource) => CanSelectCardCondition(cardSource),
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 Digimon to play.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                            canLookReverseCard: false,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }
                    }

                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => CanSelectCardCondition1(cardSource)) >= 1)
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: (cardSource) => CanSelectCardCondition1(cardSource),
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 Digimon to play.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                            canLookReverseCard: false,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.DigivolutionCards,
                            activateETB: true));

                    if(selectedCards.Count > 0 && selectedCards.Any(x => CardEffectCommons.IsExistOnBattleAreaDigimon(x)))
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IPutSecurityPermanent(
                            card.PermanentOfThisCard(),
                            CardEffectCommons.CardEffectHashtable(activateClass),
                            toTop: true).PutSecurity());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}