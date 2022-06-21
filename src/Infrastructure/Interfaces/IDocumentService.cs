namespace Infrastructure.Interfaces
{
    public interface IDocumentService<TDocument>
    {
        T ComposePdf<T>(object model = null) where T : TDocument;

        byte[] GeneratePdf<T>(object model = null) where T : TDocument;
    }
}
