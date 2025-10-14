using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Asuna Shiroki
namespace DCGO.CardEffects.P
{
    public class P_212 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                cardEffects.Add(CardEffectFactory.Gain1MemoryTamerOpponentDigimonEffect(card));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1 and trash 1. if this trashed a [Three Musketeers]/[TS] card, delete 1 level 3 digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] draw 1 and trash 1 card in your hand. If this effect trashed a card with the [Three Musketeers] or [TS] trait, delete 1 of your opponent's level 3 Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasLevel && permanent.TopCard.IsLevel3;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool trashedTargetCard = false;
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DrawAndDiscardCards(
                        player: (card.Owner, card.Owner),
                        drawAmount: 1,
                        trashAmount: 1,
                        card: card,
                        activateClass: activateClass,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine
                    ));

                    IEnumerator AfterSelectPermanentCoroutine(List<CardSource> selectedCards)
                    {
                        if (selectedCards.Any() && selectedCards.Exists(x => x.HasThreeMusketeersTraits || x.HasTSTraits)) trashedTargetCard = true;
                        yield return null;
                    }

                    if (trashedTargetCard && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}