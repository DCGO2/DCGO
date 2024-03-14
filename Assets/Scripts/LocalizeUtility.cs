using System.Collections;
using System.Collections.Generic;

public static class LocalizeUtility
{
    public static string GetLocalizedString(string EngMessage, string JpnMessage)
    {
        if (ContinuousController.instance != null)
        {
            switch (ContinuousController.instance.language)
            {
                case Language.ENG:
                    return EngMessage;

                case Language.JPN:
                    return JpnMessage;
            }
        }

        return EngMessage;
    }
}
