using System.Text;

namespace IFP_reader;

public sealed partial class IfpReader
{
    public const char N0 = '\0';
    public static readonly Encoding IfpEncoding = Encoding.UTF8;
    
    private static readonly IOException HeaderException = new($"{nameof(IfpReader)}: File header not match with .ifp extension");
}