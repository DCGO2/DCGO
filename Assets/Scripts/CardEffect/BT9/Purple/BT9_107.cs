using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT9_107 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OptionSkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] Trash up to 3 cards in your hand. For each card trashed by this effect, <De-Digivolve 1> 1 of your opponent's Digimon. (Trash 1 card from the top of one of your opponent's Digimon. If it has no digivolution cards, or becomes a level 3 Digimon, you can't trash any more cards.) Then, delete 1 of your opponentÅf level 4 or lower Digimon.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanSelectPermanentCondition1(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.Level <= 4)
                    {
                        if (permanent.TopCard.HasLevel)
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

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                int De_DigivolveCount = 0;

                if (card.Owner.HandCards.Count >= 1)
                {
                    int discardCount = 3;

                    if (card.Owner.HandCards.Count < discardCount)
                    {
                        discardCount = card.Owner.HandCards.Count;
                    }

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: (cardSource) => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: CanEndSelectCondition,
                        maxCount: discardCount,
                        canNoSelect: false,
                        canEndNotMax: true,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    yield return StartCoroutine(selectHandEffect.Activate());

                    bool CanEndSelectCondition(List<CardSource> cardSources)
                    {
                        if (discardCount >= 1)
                        {
                            if (cardSources.Count <= 0)
                            {
                                return false;
                            }
                        }

                        return true;
                    }

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        De_DigivolveCount = cardSources.Count;
                        yield return null;
                    }
                }

                if (De_DigivolveCount >= 1)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        List<Permanent> selectedPermanents = new List<Permanent>();

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve.", "The opponent is selecting 1 Digimon to De-Digivolve.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanents.Add(permanent);

                            yield return null;
                        }

                        if (selectedPermanents.Count >= 1)
                        {
                            foreach (Permanent selectedPermanent in selectedPermanents)
                            {
                                if (selectedPermanent != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new IDegeneration(selectedPermanent, De_DigivolveCount, activateClass).Degeneration());
                                }
                            }
                        }
                    }
                }

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition1));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition1,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects, effectName: $"Trash cards from hand to De-Digivolve to 1 Digimon and delete 1 level 4 or lower Digimon");
        }

        return cardEffects;
    }
}
