using System;
using System.IO;
using System.Threading.Tasks;
using Infrastructure.Interfaces;
#if USING_QUESTPDF
using QuestPDF.Drawing;
using QuestPDF.Fluent;
#endif

namespace Infrastructure.Services
{
    public class DocumentService : IDocumentService
    {
        public T Compose<T>(object model = null) where T : IDocument
        {
            var instance = (T)Activator.CreateInstance(typeof(T), model);
            return instance;
        }

        public byte[] Generate<T>(T instance, string format) where T : IDocument
        {
            if (instance == null)
            {
                return null;
            }

            var data = Array.Empty<byte>();

#if USING_QUESTPDF
            if (format.Equals("pdf", StringComparison.InvariantCultureIgnoreCase))
            {
                if (instance is QuestPDF.Infrastructure.IDocument)
                {
                    data = (instance as QuestPDF.Infrastructure.IDocument).GeneratePdf();
                }
            }
#endif
#if DEBUG
            var tempFile = Path.ChangeExtension(Path.GetTempFileName(), $".{format}");
            File.WriteAllBytes(tempFile, data);
            Console.WriteLine($"[{GetType().Name}]: Temporal document generated in: {tempFile}");
#endif
            return data;
        }

        public Task<T> ComposeAsync<T>(object model = null) where T : IDocument
        {
            var instance = Compose<T>(model);
            return Task.FromResult(instance);
        }

        public Task<byte[]> GenerateAsync<T>(T instance, string format) where T : IDocument
        {
            var data = Generate(instance, format);
            return Task.FromResult(data);
        }

        public static void RegisterFonts(string path, string format = "ttf")
        {
#if USING_QUESTPDF
            if (Directory.Exists(path))
            {
                var fonts = Directory.GetFiles(path, $"*.{format}");
                foreach (var font in fonts)
                {
                    using var fs = File.OpenRead(font);
                    FontManager.RegisterFont(fs);
                }
            }
#endif
        }
    }
}
