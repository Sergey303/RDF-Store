namespace RDFCommon.Interfaces
{
    public interface ILanguageLiteral  :ILiteralNode
    {
        string Lang { get; }
    }
}