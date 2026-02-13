using System.Collections.Generic;

namespace DCGO.CardEffects.EX3
{
    public class EX3_046 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent != card.PermanentOfThisCard())
                        {
                            if (permanent.TopCard.CardTraits.Contains("D-Brigade"))
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
                    return "<Decoy ([D-Brigade])> (When one of your other Digimon with [D-Brigade] in its traits would be deleted by an opponent's effect, you may delete this Digimon to prevent that deletion.)";
                }

                cardEffects.Add(CardEffectFactory.DecoySelfEffect(isInheritedEffect: false, card: card, condition: null, permanentCondition: CanSelectPermanentCondition, effectName: "Decoy ([D-Brigade])", effectDiscription: EffectDiscription()));
            }

            return cardEffects;
        }
    }
}