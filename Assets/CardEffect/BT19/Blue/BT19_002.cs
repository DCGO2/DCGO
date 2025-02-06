using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT19
{
    public class BT19_002 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Opponent's Turn - ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                int bouncedLevel = 0;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Bottom deck this Digimon to return to hand an opponent's Digimon with the same level", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Opponent's Turn] When any of your opponent's Digimon attack, by returning this Digimon with [Aqua]/[Sea Animal] in one of its traits to the bottom of the deck, return 1 of your opponent's Digimon with as high or lower a level as the returned Digimon to the hand.";
                }

                bool AttackingPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOpponentTurn(card) &&
                           CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, AttackingPermanent);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.HasLevel && permanent.Level <= bouncedLevel;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.PermanentOfThisCard().TopCard.HasLevel &&
                           card.PermanentOfThisCard().TopCard.HasAquaTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bouncedLevel = card.PermanentOfThisCard().TopCard.Level;

                    yield return ContinuousController.instance.StartCoroutine(
                        new DeckBottomBounceClass(new List<Permanent> { card.PermanentOfThisCard() }, hashtable).DeckBounce());

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition_ByPreSelecetedList: null,
                            canTargetCondition: CanSelectPermanentCondition,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Bounce,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to return to hand.",
                            "The opponent is selecting 1 Digimon to return to hand.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}