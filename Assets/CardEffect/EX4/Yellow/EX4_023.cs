using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX4
{
    public class EX4_023 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal 1 card from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Reveal_EX4_023");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Opponent's Turn][Once Per Turn] When an opponent plays a Digimon, by revealing 1 card of the same level in your hand, place that card on top of your security stack face down.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.HasLevel)
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
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition))
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
                        List<Permanent> permanents = CardEffectCommons.GetPlayedPermanentsFromEnterFieldHashtable(
                        hashtable: hashtable,
                        rootCondition: null);

                        if (permanents != null)
                        {
                            List<int> levels = permanents
                            .Filter(permanent => permanent != null && permanent.LevelJustAfterPlayed >= 0)
                            .Map(permanent => permanent.LevelJustAfterPlayed);

                            bool CanSelectCardCondition(CardSource cardSource)
                            {
                                if (cardSource.HasLevel && levels.Contains(cardSource.Level))
                                {
                                    return true;
                                }

                                return false;
                            }

                            if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                            {
                                if (levels.Count == 1)
                                {
                                    activateClass.SetEffectName($"Reveal 1 level {levels[0]} card from hand");
                                }

                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<Permanent> permanents = CardEffectCommons.GetPlayedPermanentsFromEnterFieldHashtable(
                        hashtable: _hashtable,
                        rootCondition: null);

                    if (permanents != null)
                    {
                        List<int> levels = permanents
                            .Filter(permanent => permanent != null && permanent.LevelJustAfterPlayed >= 0)
                            .Map(permanent => permanent.LevelJustAfterPlayed);

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            if (cardSource.HasLevel && levels.Contains(cardSource.Level))
                            {
                                return true;
                            }

                            return false;
                        }

                        if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            int maxCount = 1;

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectHandEffect.SetUpCustomMessage("Select 1 card to reveal.", "The opponent is selecting 1 card to reveal.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Revealed Card");

                            yield return StartCoroutine(selectHandEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            if (selectedCards.Count >= 1)
                            {
                                foreach (CardSource selectedCard in selectedCards)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(selectedCard));
                                }
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}