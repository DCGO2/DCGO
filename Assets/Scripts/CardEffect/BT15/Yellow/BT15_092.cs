using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT15
{
    public class BT15_092 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region When trashed from security

            if (timing == EffectTiming.OnDiscardSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Activate security effect", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When an effect trashes this card from the security stack, activate this card's [Security] effect.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnTrashSelfSecurity(hashtable, cardEffect => cardEffect != null, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    ActivateClass mainActivateClass = CardEffectCommons.OptionSecurityEffect(card);

                    if (mainActivateClass != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(mainActivateClass.Activate(CardEffectCommons.OptionMainCheckHashtable(card)));
                    }
                }
            }

            #endregion

            #region Main

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Digimon from security", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] Search your security stack. You may play 1 level 4 or lower yellow Digimon card among it without paying the cost. Then, shuffle your security stack. If you have a Tamer with [Kari Kamiya] in its name, place this card on top of your security stack.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.CardColors.Contains(CardColor.Yellow) && cardSource.Level <= 4)
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (card.Owner.CanAddSecurity(activateClass))
                        {
                            if (permanent.IsTamer)
                            {
                                if (permanent.TopCard.ContainsCardName("Kari Kamiya"))
                                {
                                    return true;
                                }

                                if (permanent.TopCard.ContainsCardName("KariKamiya"))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        bool played = false;

                        int maxCount = Math.Min(1, card.Owner.SecurityCards.Count(CanSelectCardCondition));

                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            message: "Select 1 Digimon to play.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Security,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                                    player: card.Owner,
                                    refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Security, activateETB: true));
                        }
                    }

                    card.Owner.SecurityCards = RandomUtility.ShuffledDeckCards(card.Owner.SecurityCards);

                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, PermanentCondition))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(card, toTop: true));
                    }
                }
            }

            #endregion

            #region Security effect

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Security Attack -1", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] All of your opponent's Digimon and all of your opponent's Security Digimon get -5000 DP until the end of your turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool PermanentCondition(Permanent permanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDPPlayerEffect(
                        permanentCondition: PermanentCondition,
                        changeValue: -5000,
                        effectDuration: EffectDuration.UntilOwnerTurnEnd,
                        activateClass: activateClass));

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeSecurityDigimonCardDPPlayerEffect(
                            cardCondition: cardSource => cardSource.Owner == card.Owner.Enemy,
                            changeValue: -5000,
                            effectDuration: EffectDuration.UntilOwnerTurnEnd,
                            activateClass: activateClass));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}