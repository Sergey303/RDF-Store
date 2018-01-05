namespace RDFCommon.Interfaces
{
    public interface ILiteralNode
    {  
        string DataType { get; }
        dynamic Content { get; }
    }
}