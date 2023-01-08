namespace Infrastructure.Interfaces
{

	public interface IDocument
	{
		string Title { get; set; }
		string FileName { get; set; }
	}

	public interface IDocument<TModel> : IDocument
	{
		TModel Model { get; }
	}


#if USING_QUESTPDF
	public interface IPdfDocument : IDocument, QuestPDF.Infrastructure.IDocument
	{
	}

	public interface IPdfDocument<TModel> : IDocument<TModel>, QuestPDF.Infrastructure.IDocument
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
