namespace XamlBlocksCompiler.Models;

public struct Color
{
    public byte R;
    public byte G;
    public byte B;
    public byte A;

    private Color (byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public static Color Transparent => new(0, 0, 0, 0);
    public static Color FromRgb(byte r, byte g, byte b) => new(r, g, b, 255);
    public readonly string ToHex() => $"#{A:X2}{R:X2}{G:X2}{B:X2}";
    public static Color FromHex(string hex)
    {
        if (hex.Length != 9 || hex[0] != '#')
            throw new FormatException("Expected #AARRGGBB.");

        return new Color(
            Convert.ToByte(hex.Substring(3, 2), 16), // R
            Convert.ToByte(hex.Substring(5, 2), 16), // G
            Convert.ToByte(hex.Substring(7, 2), 16), // B
            Convert.ToByte(hex.Substring(1, 2), 16)  // A
        );
    }
    public static Color Create(byte r, byte g, byte b, byte a)
    {
        return new(r, g, b, a);
    }
}
