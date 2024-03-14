using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;

public class CardDistribution : MonoBehaviour
{
    [SerializeField]
    List<CardDistributionTab> cardDistributionTabs
    {
        get
        {
            return new List<CardDistributionTab>()
            {
                digitama,
                digimon,
                tamer,
                option,
                level2,
                level3,
                level4,
                level5,
                level6,
                level7Over,
            };
        }
    }

    [SerializeField] CardDistributionTab digitama;
    [SerializeField] CardDistributionTab digimon;
    [SerializeField] CardDistributionTab tamer;
    [SerializeField] CardDistributionTab option;
    [SerializeField] CardDistributionTab level2;
    [SerializeField] CardDistributionTab level3;
    [SerializeField] CardDistributionTab level4;
    [SerializeField] CardDistributionTab level5;
    [SerializeField] CardDistributionTab level6;
    [SerializeField] CardDistributionTab level7Over;

    public void Init()
    {
        digitama.CardCondition = (cEntity_Base) => cEntity_Base.cardKind == CardKind.DigiEgg;
        digimon.CardCondition = (cEntity_Base) => cEntity_Base.cardKind == CardKind.Digimon;
        tamer.CardCondition = (cEntity_Base) => cEntity_Base.cardKind == CardKind.Tamer;
        option.CardCondition = (cEntity_Base) => cEntity_Base.cardKind == CardKind.Option;
        level2.CardCondition = (cEntity_Base) => cEntity_Base.Level == 2;
        level3.CardCondition = (cEntity_Base) => cEntity_Base.Level == 3;
        level4.CardCondition = (cEntity_Base) => cEntity_Base.Level == 4;
        level5.CardCondition = (cEntity_Base) => cEntity_Base.Level == 5;
        level6.CardCondition = (cEntity_Base) => cEntity_Base.Level == 6;
        level7Over.CardCondition = (cEntity_Base) => cEntity_Base.Level >= 7;
    }

    public void SetCardDistribution(DeckData deckData)
    {
        foreach (CardDistributionTab cardDistributionTab in cardDistributionTabs)
        {
            cardDistributionTab.SetCardDistributionTab(deckData);
        }
    }
}
