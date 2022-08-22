using System.Collections.Generic;
using System.Linq;
using System;
using RandomHelper = System.Extensions;

namespace Tests.Abstractions.Services
{
    public class LoremIpsumService
    {
        public string Text { get; private set; } = @"lorem ipsum amet, pellentesque mattis accumsan maximus etiam mollis ligula non iaculis ornare mauris efficitur ex eu rhoncus aliquam in hac habitasse platea dictumst maecenas ultrices, purus at venenatis auctor, sem nulla urna, molestie nisi mi a ut euismod nibh id libero lacinia, sit amet lacinia lectus viverra donec scelerisque dictum enim, dignissim dolor cursus morbi rhoncus, elementum magna sed, sed velit consectetur adipiscing elit curabitur nulla, eleifend vel, tempor metus phasellus vel pulvinar, lobortis quis, nullam felis orci congue vitae augue nisi, tincidunt id, posuere fermentum facilisis ultricies mi, nisl fusce neque, vulputate integer tortor tempus praesent proin quis nunc massa congue, quam auctor eros placerat eros, leo nec, sapien egestas duis feugiat, vestibulum porttitor, odio sollicitudin arcu, et aenean sagittis ante urna fringilla, risus et, vivamus semper nibh, eget finibus est laoreet justo commodo sagittis, vitae, nunc, diam ac, tellus posuere, condimentum enim tellus, faucibus suscipit ac nec turpis interdum malesuada fames primis quisque pretium ex, feugiat porttitor massa, vehicula dapibus blandit, hendrerit elit, aliquet nam orci, fringilla blandit ullamcorper mauris, ultrices consequat tempor, convallis gravida sodales volutpat finibus, neque pulvinar varius, porta laoreet, eu, ligula, porta, placerat, lacus pharetra erat bibendum leo, tristique cras rutrum at, dui tortor, in, varius arcu interdum, vestibulum, magna, ante, imperdiet erat, luctus odio, non, dui, volutpat, bibendum, quam, euismod, mattis, class aptent taciti sociosqu ad litora torquent per conubia nostra, inceptos himenaeos suspendisse lorem, a, sem, eleifend, commodo, dolor, cursus, luctus, lectus,";

        internal IEnumerable<string> Rearrange(string words)
        {
            return words.Split(" ").OrderBy(_ => RandomHelper.Random());
        }

        internal IEnumerable<string> WordList(bool includePuncation)
        {
            return includePuncation ? Rearrange(Text) : Rearrange(Text.Replace(",", ""));
        }

        public void Update(string text)
        {
            Text = text;
        }

        public bool Chance(int successes, int attempts)
        {
            var number = Number(1, attempts);

            return number <= successes;
        }

        public T Random<T>(T[] items)
        {
            var index = RandomHelper.Instance.Next(items.Length);

            return items[index];
        }

        public TEnum Enum<TEnum>() where TEnum : struct, IConvertible
        {
            if (typeof(TEnum).IsEnum)
            {
                var v = System.Enum.GetValues(typeof(TEnum));
                return (TEnum)v.GetValue(RandomHelper.Random(v.Length))!;
            }
            else
            {
                throw new ArgumentException("Generic type must be an enum.");
            }
        }

        /* http://stackoverflow.com/a/6651661/234132 */
        public long Number(long min, long max)
        {
            byte[] buf = new byte[8];
            RandomHelper.Instance.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            return (Math.Abs(longRand % ((max + 1) - min)) + min);
        }

        #region DateTime

        public DateTime DateTime(int startYear = 1950, int startMonth = 1, int startDay = 1)
        {
            return DateTime(new DateTime(startYear, startMonth, startDay), System.DateTime.Now);
        }

        public DateTime DateTime(DateTime min)
        {
            return DateTime(min, System.DateTime.Now);
        }

        /* http://stackoverflow.com/a/1483677/234132 */
        public DateTime DateTime(DateTime min, DateTime max)
        {
            TimeSpan timeSpan = max - min;
            TimeSpan newSpan = new TimeSpan(0, RandomHelper.Instance.Next(0, (int)timeSpan.TotalMinutes), 0);

            return min + newSpan;
        }

        #endregion

        #region Text

        public string Email()
        {
            return string.Format("{0}@{1}.com", Words(1, false), Words(1, false));
        }

        public string Words(int wordCount, bool uppercaseFirstLetter = true, bool includePunctuation = false)
        {
            return Words(wordCount, wordCount, uppercaseFirstLetter, includePunctuation);
        }

        public string Words(int wordCountMin, int wordCountMax, bool uppercaseFirstLetter = true, bool includePunctuation = false)
        {
            var source = string.Join(" ", WordList(includePunctuation).Take(RandomHelper.Instance.Next(wordCountMin, wordCountMax)));

            if (uppercaseFirstLetter)
            {
                source = char.ToUpper(source[0]) + source.Substring(1);
            }

            return source;
        }

        public string Sentence(int wordCount)
        {
            return Sentence(wordCount, wordCount);
        }

        public string Sentence(int wordCountMin, int wordCountMax)
        {
            return string.Format("{0}.", Words(wordCountMin, wordCountMax, true, true)).Replace(",.", ".").Replace("..", "");
        }

        public string Paragraph(int wordCount, int sentenceCount)
        {
            return Paragraph(wordCount, wordCount, sentenceCount, sentenceCount);
        }

        public string Paragraph(int wordCountMin, int wordCountMax, int sentenceCount)
        {
            return Paragraph(wordCountMin, wordCountMax, sentenceCount, sentenceCount);
        }

        public string Paragraph(int wordCountMin, int wordCountMax, int sentenceCountMin, int sentenceCountMax)
        {
            var source = string.Join(" ", Enumerable.Range(0, RandomHelper.Instance.Next(sentenceCountMin, sentenceCountMax)).Select(_ => Sentence(wordCountMin, wordCountMax)));

            //remove traililng space
            return source.Remove(source.Length - 1);
        }

        public IEnumerable<string> Paragraphs(int wordCount, int sentenceCount, int paragraphCount)
        {
            return Paragraphs(wordCount, wordCount, sentenceCount, sentenceCount, paragraphCount, paragraphCount);
        }

        public IEnumerable<string> Paragraphs(int wordCountMin, int wordCountMax, int sentenceCount, int paragraphCount)
        {
            return Paragraphs(wordCountMin, wordCountMax, sentenceCount, sentenceCount, paragraphCount, paragraphCount);
        }

        public IEnumerable<string> Paragraphs(int wordCountMin, int wordCountMax, int sentenceCountMin, int sentenceCountMax, int paragraphCount)
        {
            return Paragraphs(wordCountMin, wordCountMax, sentenceCountMin, sentenceCountMax, paragraphCount, paragraphCount);
        }

        public IEnumerable<string> Paragraphs(int wordCountMin, int wordCountMax, int sentenceCountMin, int sentenceCountMax, int paragraphCountMin, int paragraphCountMax)
        {
            return Enumerable.Range(0, RandomHelper.Instance.Next(paragraphCountMin, paragraphCountMax)).Select(_ => Paragraph(wordCountMin, wordCountMax, sentenceCountMin, sentenceCountMax)).ToArray();
        }

        #endregion

        #region Color

        /* http://stackoverflow.com/a/1054087/234132 */
        public string HexNumber(int digits)
        {
            byte[] buffer = new byte[digits / 2];
            RandomHelper.Instance.NextBytes(buffer);
            string result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());

            if (digits % 2 == 0)
            {
                return result;
            }

            return result + RandomHelper.Instance.Next(16).ToString("X");
        }

        #endregion
    }
}
