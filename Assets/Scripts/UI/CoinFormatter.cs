using UnityEngine;

public static class CoinFormatter
{
    public static string FormatNumber(float number)
    {
        if (number < 1000f)
            return Mathf.FloorToInt(number).ToString();

        if (number < 1000000f)
            return FormatShort(number / 1000f) + "K";

        if (number < 1000000000f)
            return FormatShort(number / 1000000f) + "M";

        return FormatShort(number / 1000000000f) + "B";
    }

    private static string FormatShort(float value)
    {
        if (value >= 100f)
            return Mathf.FloorToInt(value).ToString();

        if (value >= 10f)
            return value.ToString("0.#");

        return value.ToString("0.##");
    }
}
