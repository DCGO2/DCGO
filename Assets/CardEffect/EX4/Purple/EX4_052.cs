using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX4
{
    public class EX4_052 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 card from hand to Draw 2", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Draw2_EX4_052");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn][Once Per Turn] When an opponent's Digimon is deleted, by trashing 1 card of the same level in your hand, <Draw 2>. (Draw 2 cards from your deck.)";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition))
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
                        List<Hashtable> hashtables = CardEffectCommons.GetHashtablesFromHashtable(hashtable);

                        if (hashtables != null)
                        {
                            List<int> levels = hashtables
                            .Map(hashtable1 => CardEffectCommons.GetPermanentFromHashtable(hashtable1))
                            .Filter(permanent => permanent != null && permanent.LevelJustBeforeRemoveField > 0)
                            .Map(permanent => permanent.LevelJustBeforeRemoveField);

                            bool CanSelectCardCondition(CardSource cardSource)
                            {
                                return cardSource.HasLevel && levels.Contains(cardSource.Level);
                            }

                            if (card.Owner.HandCards.Some(CanSelectCardCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<Hashtable> hashtables = CardEffectCommons.GetHashtablesFromHashtable(_hashtable);

                    if (hashtables != null)
                    {
                        List<int> levels = hashtables
                        .Map(hashtable1 => CardEffectCommons.GetPermanentFromHashtable(hashtable1))
                        .Filter(permanent => permanent != null && permanent.LevelJustBeforeRemoveField > 0)
                        .Map(permanent => permanent.LevelJustBeforeRemoveField);

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            return cardSource.HasLevel && levels.Contains(cardSource.Level);
                        }

                        if (card.Owner.HandCards.Some(CanSelectCardCondition))
                        {
                            bool discarded = false;

                            int discardCount = 1;

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: discardCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                mode: SelectHandEffect.Mode.Discard,
                                cardEffect: activateClass);

                            yield return StartCoroutine(selectHandEffect.Activate());

                            IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                            {
                                if (cardSources.Count >= 1)
                                {
                                    discarded = true;

                                    yield return null;
                                }
                            }

                            if (discarded)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 2, activateClass).Draw());
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}