using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX10
{
    public class EX10_003 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("by trashing 3 digivolution sources, end an attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("ESS_EX10-003");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Opponent's Turn] [Once Per Turn] When one of your opponent's Digimon attacks, by trashing 3 [Mineral] or [Rock] trait cards from this Digimon's digivolution cards, end that attack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, OpponentsDigimon);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOpponentTurn(card) &&
                           card.PermanentOfThisCard().DigivolutionCards.Count(ProperSources) >= 3;
                }

                bool OpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool ProperSources(CardSource source)
                {
                    return !source.CanNotTrashFromDigivolutionCards(activateClass) &&
                           (source.EqualsTraits("Rock") ||
                           source.EqualsTraits("Mineral"));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent permanent = card.PermanentOfThisCard();
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                    selectCardEffect.SetUp(
                                canTargetCondition: ProperSources,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: SelectCardCoroutine,
                                message: "Select digivolution cards to trash",
                                maxCount: 3,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: permanent.DigivolutionCards,
                                canLookReverseCard: false,
                                selectPlayer: card.Owner,
                                cardEffect: null);

                    selectCardEffect.SetUpCustomMessage("Select digivolution cards to trash.", "The opponent is selecting digivolution cards to return to trash.");
                    selectCardEffect.SetNotShowCard();

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(List<CardSource> cardSources)
                    {
                        selectedCards = cardSources;
                        yield return null;
                    }

                    if (selectedCards.Count >= 3)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(permanent, selectedCards, activateClass).TrashDigivolutionCards());

                        GManager.instance.attackProcess.IsEndAttack = true;
                    }                        
                }
            }

            #endregion

            return cardEffects;
        }
    }
}