using System;
using System.IO;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;

#if USING_QUESTPDF
using QuestPDF.Fluent;
#endif

namespace Infrastructure.Services
{
	public class DocumentService : IDocumentService
	{
		// ReSharper disable once NotAccessedField.Local
		private readonly IConfiguration _configuration;

		public DocumentService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public T Compose<T>(object model = null) where T : IDocument
		{
			var instance = (T)Activator.CreateInstance(typeof(T), model);
			return instance;
		}

		public byte[] Generate<T>(T instance, string format) where T : IDocument
		{
			if (instance == null) return null;

			var data = Array.Empty<byte>();

#if USING_QUESTPDF
			if (format.Equals("pdf", StringComparison.InvariantCultureIgnoreCase))
				if (instance is QuestPDF.Infrastructure.IDocument)
					data = (instance as QuestPDF.Infrastructure.IDocument).GeneratePdf();
#endif
#if DEBUG
			var tempFile = Path.ChangeExtension(Path.GetTempFileName(), $".{format}");
			File.WriteAllBytes(tempFile, data);
			Console.WriteLine($"[{GetType().Name}]: Temporal document generated in: {tempFile}");
#endif
			return data;
		}
		public static void RegisterFonts(string path, string format = "ttf")
		{
#if USING_QUESTPDF
			if (Directory.Exists(path))
			{
				var fonts = Directory.GetFiles(path, $"*.{format}");
				foreach (var font in fonts)
					using (var fs = File.OpenRead(font))
						QuestPDF.Drawing.FontManager.RegisterFont(fs);
			}
#endif
		}
	}
}
