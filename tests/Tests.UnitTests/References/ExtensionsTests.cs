using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using Domain.Interfaces;
using Shouldly;
using Xunit;

namespace Tests.UnitTests.References
{
    public class ExtensionsTests
    {
        #region System

        private enum TestEnum : short
        {
            [Description("None")]
            None = 0,
        }

        [Fact]
        public void Should_Get_Attribute()
        {
            TestEnum.None.GetAttribute<DescriptionAttribute>().ShouldNotBeNull();
        }

        [Fact]
        public void Should_Get_Name()
        {
            TestEnum.None.ToName().ShouldNotBeNull();
        }

        [Fact]
        public void Should_Convert_To_Dictionary()
        {
            EnumExtensions.ToDictionary<TestEnum>().ShouldNotBeNull();
        }

        [Fact]
        public void Should_Not_Convert_To_Dictionary()
        {
            try
            {
                typeof(int).ToDictionary<int>().ShouldBeNull();
            }
            catch (Exception e)
            {
                e.ShouldBeOfType<ArgumentException>();
            }
        }

        [Fact]
        public void Should_Convert_To_Dictionary_With_Values()
        {
            EnumExtensions.ToDictionary<TestEnum, short>().ShouldNotBeNull();
        }

        #endregion

        #region Reflection

        private class SoftDelete : IEntity, ISoftDelete
        {
            public int Id { get; set; }
            public bool IsDeleted { get; set; }
            public DateTime? DeletedAt { get; set; }
        }

        private class NotSoftDelete : IEntity
        {
            public int Id { get; set; }
            public bool IsDeleted { get; set; }
            public DateTime? DeletedAt { get; set; }
        }

        [Fact]
        public void Should_Return_True_When_IsSoftDelete()
        {
            var instance = new SoftDelete();
            instance.IsSoftDelete().ShouldBeTrue();
        }

        [Fact]
        public void Should_Return_False_When_Not_IsSoftDelete()
        {
            var instance = new NotSoftDelete();
            instance.IsSoftDelete().ShouldBeFalse();
        }

        #endregion

        #region Text

        public enum Culture
        {
            None = 0,
            Invariant = 1,
            Current = 2,
            CurrentUi = 3
        }

        [Theory]
        [InlineData(Culture.None)]
        [InlineData(Culture.Invariant)]
        [InlineData(Culture.Current)]
        public void Should_Convert_To_Title_Case(Culture culture)
        {
            string value = culture switch
            {
                Culture.None => Guid.NewGuid().ToString().Replace("-", " ").ToTitleCase(),
                Culture.Invariant => Guid.NewGuid().ToString().Replace("-", " ").ToTitleCaseInvariant(),
                Culture.Current => Guid.NewGuid().ToString().Replace("-", " ").ToTitleCase(CultureInfo.CurrentCulture),
                Culture.CurrentUi => Guid.NewGuid().ToString().Replace("-", " ").ToTitleCase(CultureInfo.CurrentUICulture),
                _ => throw new ArgumentOutOfRangeException(nameof(culture), culture, null)
            };
            value.ShouldNotBeNull();
        }

        [Theory]
        [InlineData(Culture.None)]
        [InlineData(Culture.Invariant)]
        [InlineData(Culture.Current)]
        public void Should_Convert_To_Camel_Case(Culture culture)
        {
            string value = culture switch
            {
                Culture.None => Guid.NewGuid().ToString().Replace("-", " ").ToCamelCase(),
                Culture.Invariant => Guid.NewGuid().ToString().Replace("-", " ").ToCamelCaseInvariant(),
                Culture.Current => Guid.NewGuid().ToString().Replace("-", " ").ToCamelCase(CultureInfo.CurrentCulture),
                Culture.CurrentUi => Guid.NewGuid().ToString().Replace("-", " ").ToCamelCase(CultureInfo.CurrentUICulture),
                _ => throw new ArgumentOutOfRangeException(nameof(culture), culture, null)
            };
            value.ShouldNotBeNull();
        }

        [Theory]
        [InlineData(Culture.None)]
        [InlineData(Culture.Invariant)]
        [InlineData(Culture.Current)]
        public void Should_Convert_To_Sentence(Culture culture)
        {
            string value = culture switch
            {
                Culture.None => Guid.NewGuid().ToString().Replace("-", " ").ToSentence(),
                Culture.Invariant => Guid.NewGuid().ToString().Replace("-", " ").ToSentenceInvariant(),
                Culture.Current => Guid.NewGuid().ToString().Replace("-", " ").ToSentence(CultureInfo.CurrentCulture),
                Culture.CurrentUi => Guid.NewGuid().ToString().Replace("-", " ").ToSentence(CultureInfo.CurrentUICulture),
                _ => throw new ArgumentOutOfRangeException(nameof(culture), culture, null)
            };
            value.ShouldNotBeNull();
        }

        [Theory]
        [InlineData("28CA562C", new[] { "2", "8", "C", "A", "5", "6" }, "1", "11111111")]
        [InlineData("28CA562C", new[] { "C", "A" }, "1", "28115621")]
        public void Should_Replace_Values(string input, string[] find, string replace, string expected)
        {
            input.Replace(find, replace).ShouldBe(expected);
        }

        [Theory]
        [InlineData("#28c@5A2&", false, true, true, "28c5A2")]
        [InlineData("#28c@5A2&", false, false, true, "28c5a2")]
        [InlineData("#28c@5A2&", false, false, false, "28c5A2")]
        [InlineData("#28c@5A2&", true, false, false, "#28c@5A2&")]
        [InlineData("#28c@5A2&", true, true, false, "#28C@5A2&")]
        [InlineData("#28c@5A2&", true, true, true, "#28c@5A2&")]
        public void Should_Normalize(string input, bool specialsChars, bool upperCase, bool lowerCase, string expected)
        {
            input.Normalize(specialsChars, upperCase, lowerCase).ShouldBe(expected);
        }

        #endregion
    }
}
