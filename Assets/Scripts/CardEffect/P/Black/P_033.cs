using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class P_033 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            AddSkillClass addSkillClass = new AddSkillClass();
            addSkillClass.SetUpICardEffect("Your Digimons gain Pierce", CanUseCondition, card);
            addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
            cardEffects.Add(addSkillClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    if (permanent.DP >= 13000)
                    {
                        if (permanent.TopCard.CardColors.Contains(CardColor.Black))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CardSourceCondition(CardSource cardSource)
            {
                if (CardEffectCommons.IsExistOnBattleArea(cardSource))
                {
                    if (cardSource.Owner == card.Owner)
                    {
                        if (cardSource == cardSource.PermanentOfThisCard().TopCard)
                        {
                            if (PermanentCondition(cardSource.PermanentOfThisCard()))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
            {
                if (_timing == EffectTiming.OnDetermineDoSecurityCheck)
                {
                    bool Condition()
                    {
                        return CardSourceCondition(cardSource);
                    }

                    cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: cardSource, condition: Condition));
                }

                return cardEffects;
            }
        }

        if (timing == EffectTiming.None)
        {
            bool Condition()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (card.PermanentOfThisCard().TopCard.CardColors.Contains(CardColor.Black))
                        {
                            if (card.PermanentOfThisCard().DP >= 13000)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: true, card: card, condition: Condition));
        }

        return cardEffects;
    }
}
