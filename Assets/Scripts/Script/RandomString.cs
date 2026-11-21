public static class RandomString
{
    public static string Create_AlpahabetNum(int length)
    {
        string PASSWORD_CHARS =
        "0123456789abcdefghijklmnopqrstuvwxyz";

        var sb = new System.Text.StringBuilder(length);
        var r = new System.Random();

        for (int i = 0; i < length; i++)
        {
            int pos = r.Next(PASSWORD_CHARS.Length);
            char c = PASSWORD_CHARS[pos];
            sb.Append(c);
        }

        return sb.ToString();
    }

    public static string Create_Num(int length)
    {
        string PASSWORD_CHARS =
        "0123456789";

        var sb = new System.Text.StringBuilder(length);
        var r = new System.Random();

        for (int i = 0; i < length; i++)
        {
            int pos = r.Next(PASSWORD_CHARS.Length);
            char c = PASSWORD_CHARS[pos];
            sb.Append(c);
        }

        return sb.ToString();
    }
}
