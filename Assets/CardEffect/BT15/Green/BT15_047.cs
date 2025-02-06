using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT15
{
    public class BT15_047 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region All Turns

            if (timing == EffectTiming.None)
            {
                CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's Digimon's effects", CanUseCondition, card);
                canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                cardEffects.Add(canNotAffectedClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().IsSuspended)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        if (CardEffectCommons.IsExistOnBattleArea(card))
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
                    if (CardEffectCommons.IsOpponentEffect(cardEffect, card))
                    {
                        if (cardEffect.IsDigimonEffect)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }

            #endregion

            #region Inherited Effect

            if (timing == EffectTiming.OnEndBattle)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Memory+1_BT15_047");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon deletes an opponent's Digimon in battle, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        bool WinnerCondition(Permanent permanent) => permanent.cardSources.Contains(card);
                        bool LoserCondition(Permanent permanent) => CardEffectCommons.IsOpponentPermanent(permanent, card);

                        if (CardEffectCommons.CanTriggerWhenDeleteOpponentDigimonByBattle(hashtable: hashtable, winnerCondition: WinnerCondition, loserCondition: LoserCondition, isOnlyWinnerSurvive: false))
                            return true;
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}