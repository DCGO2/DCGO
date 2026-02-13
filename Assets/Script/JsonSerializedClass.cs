using System.Collections.Generic;
using System;
using UnityEngine;

namespace DCGO.CardEntities
{
    [Serializable]
    public class RootObject
    {
        public List<CardData> cards;
    }

    [Serializable]
    public class CardData
    {
        public List<AlternateArt> AAs;
        public List<AlternateArt> JAAs;

        public string aceEffect;
        public string attribute;
        public List<string> block;
        public string burstDigivolve;
        public string cardImage;
        public string cardLv;
        public string cardNumber;
        public string cardType;
        public string color;
        public string digiXros;
        public List<DigivolveCondition> digivolveCondition;
        public string digivolveEffect;
        public string dnaDigivolve;
        public string dp;
        public string effect;
        public string form;
        public string id;
        public string illustrator;
        public string linkDP;
        public string linkEffect;
        public string linkRequirement;
        public CardName name;
        public string notes;
        public string playCost;
        public string rarity;
        public Restriction restrictions;
        public string rule;
        public string securityEffect;
        public string specialDigivolve;
        public string type;
        public string version;
    }

    [Serializable]
    public class AlternateArt
    {
        public string id;
        public string illustrator;
        public string note;
        public string type;
    }

    [Serializable]
    public class DigivolveCondition
    {
        public string color;
        public string cost;
        public string level;
    }

    [Serializable]
    public class CardName
    {
        public string english;
        public string japanese;
        public string korean;
        public string simplifiedChinese;
        public string traditionalChinese;
    }

    [Serializable]
    public class Restriction
    {
        public string english;
        public string japanese;
        public string korean;
        public string chinese;
    }
}