using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class P_045 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            AddSkillClass addSkillClass = new AddSkillClass();
            addSkillClass.SetUpICardEffect("Your Digimons gain Decoy", CanUseCondition, card);
            addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
            addSkillClass.SetIsInheritedEffect(true);
            cardEffects.Add(addSkillClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    return true;
                }

                return false;
            }

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    if (permanent != card.PermanentOfThisCard())
                    {
                        if (permanent.TopCard.HasSameCardName(card.PermanentOfThisCard().TopCard))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CardSourceCondition(CardSource cardSource)
            {
                if (CardEffectCommons.IsExistOnBattleAreaDigimon(cardSource))
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
                if (_timing == EffectTiming.WhenPermanentWouldBeDeleted)
                {
                    bool Condition()
                    {
                        return CardSourceCondition(cardSource);
                    }

                    bool CanSelectPermanentCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, cardSource))
                        {
                            if (permanent != cardSource.PermanentOfThisCard())
                            {
                                if (permanent.TopCard.CardColors.Contains(CardColor.Black) || permanent.TopCard.CardColors.Contains(CardColor.White))
                                {
                                    if (permanent.willBeRemoveField)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }

                        return false;
                    }

                    string EffectDiscription()
                    {
                        return "<Decoy (Black/White)> (When one of your other black or white Digimon would be deleted by an opponent's effect, you may delete this Digimon to prevent that deletion.)";
                    }

                    cardEffects.Add(CardEffectFactory.DecoySelfEffect(isInheritedEffect: false, card: cardSource, condition: Condition, permanentCondition: CanSelectPermanentCondition, effectName: "Decoy (Black/White)", effectDiscription: EffectDiscription(), rootCardEffect: addSkillClass));
                }

                return cardEffects;
            }
        }

        return cardEffects;
    }
}
