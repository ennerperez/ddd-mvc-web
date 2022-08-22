#if USING_QUESTPDF
using System;
using System.IO;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Infrastructure.Services
{
    public class DocumentService : IDocumentService<IDocument>
    {
        private readonly IConfiguration _configuration;

        public DocumentService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public T ComposePdf<T>(object model = null) where T : IDocument
        {
            var instance = (T)Activator.CreateInstance(typeof(T), _configuration, model);
            return instance;
        }

        public byte[] GeneratePdf<T>(object model = null) where T : IDocument
        {
            var instance = ComposePdf<T>(model);
            if (instance == null) return null;

            var data = instance.GeneratePdf();
#if DEBUG
            var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ".pdf");
            File.WriteAllBytes(tempFile, data);
            Console.WriteLine($"[DocumentService]: Temporal document generated in: {tempFile}");
#endif
            return data;
        }

        public static void RegisterFonts(string path, string format = "ttf")
        {
            if (!Directory.Exists(path))
                return;

            var fonts = Directory.GetFiles(path, $"*.{format}");
            foreach (var font in fonts)
                using (var fs = File.OpenRead(font))
                    QuestPDF.Drawing.FontManager.RegisterFont(fs);
        }
    }
}
#endif
