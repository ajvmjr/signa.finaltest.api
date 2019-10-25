using System;

namespace Signa.TemplateCore.Api.Helpers
{
    public static class Extensions
    {
        public static bool IsNumeric(this string s)
        {
            return double.TryParse(s, out double output);
        }

        public static string[] SplitBy(this string text, int maxLenght, int count)
        {
            var @return = new string[count];

            if (text.Length > maxLenght)
            {
                int j = 0;
                for (int i = 0; i < text.Length; i += maxLenght)
                {
                    @return[j] = text.Substring(i, Math.Min(maxLenght, text.Length - i));
                    j++;
                }
            }
            else
            {
                @return[0] = text;
            }
            return @return;
        }
    }
}
