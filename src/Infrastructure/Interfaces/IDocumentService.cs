namespace Infrastructure.Interfaces
{
	public interface IDocument<T>
	{
		string Title { get; set; }
		string FileName { get; set; }
		T Model { get; }
	}

	public interface IDocument: IDocument<object>
	{
	}
	
#if USING_QUESTPDF
	public interface IPdfDocument : IDocument, QuestPDF.Infrastructure.IDocument
	{
	}
#endif

	public interface IDocumentService : IDocumentService<IDocument>
	{
	}

	public interface IDocumentService<TDocument> where TDocument : IDocument
	{
		T Compose<T>(object model = null) where T : TDocument;

		byte[] Generate<T>(T instance, string format) where T : TDocument;
	
	}
}
