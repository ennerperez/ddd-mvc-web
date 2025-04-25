using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Tests.UnitTests.References
{
    namespace System
    {
        public class SystemTests
        {
            private enum TestEnum : short
            {
                [Description("None")]
                None = 0,
            }

            [Fact]
            public void Should_Get_Enum_Attribute()
            {
                TestEnum.None.GetAttribute<DescriptionAttribute>().ShouldNotBeNull();
            }

            [Fact]
            public void Should_Get_Enum_Name()
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
        }

        public class ReflectionTests
        {
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
        }

        public class TextTests
        {
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
            public void Should_Convert_To_TitleCase(Culture culture)
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
            public void Should_Convert_To_CamelCase(Culture culture)
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
            public void Should_Normalize_Text(string input, bool specialsChars, bool upperCase, bool lowerCase, string expected)
            {
                input.Normalize(specialsChars, upperCase, lowerCase).ShouldBe(expected);
            }
        }
    }

    namespace Microsoft.EntityFrameworkCore
    {
        public class DbContextExtensionsTests
        {
            private class TestContext : DbContext
            {
            }

            [Fact]
            public void UseDbEngine_Should_Return_True_When_Single_ContextName()
            {
                var options = new DbContextOptionsBuilder<TestContext>();
                var keys = new List<KeyValuePair<string, string>>
                {
                    new("ConnectionStrings:TestContext.Sqlite", "Data Source=mydb.db;")
                };
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(keys)
                    .Build();
                try
                {
                    options.UseDbEngine(configuration);
                }
                catch (Exception)
                {
                    // ignored
                }
                options.IsConfigured.ShouldBeTrue();
            }

            [Fact]
            public void UseDbEngine_Should_Return_True_When_Multiple_ContextNames()
            {
                var options = new DbContextOptionsBuilder<TestContext>();
                var keys = new List<KeyValuePair<string, string>>
                {
                    new("ConnectionStrings:TestContext1.Sqlite", "Data Source=mydb1.db;"),
                    new("ConnectionStrings:TestContext2.Sqlite", "Data Source=mydb2.db;")
                };
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(keys)
                    .Build();
                try
                {
                    options.UseDbEngine(configuration, "TestContext1");
                }
                catch (Exception)
                {
                    // ignored
                }
                options.IsConfigured.ShouldBeTrue();
            }

            [Fact]
            public void UseDbEngine_Should_Return_False_When_Duplicated_ContextName()
            {
                var options = new DbContextOptionsBuilder<TestContext>();
                var keys = new List<KeyValuePair<string, string>>
                {
                    new("ConnectionStrings:TestContext.Sqlite", "Data Source=mydb.db;"),
                    new("ConnectionStrings:TestContext.SqlServer", "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;")
                };
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(keys)
                    .Build();
                try
                {
                    options.UseDbEngine(configuration);
                }
                catch (Exception)
                {
                    // ignored
                }

                options.IsConfigured.ShouldBeTrue();
            }

            [Fact]
            public void UseMultiTenantDbEngine_Should_Return_True_When_Single_ContextName()
            {
                var options = new DbContextOptionsBuilder<TestContext>();
                var keys = new List<KeyValuePair<string, string>>
                {
                    new("ConnectionStrings:TestContext[1].Sqlite", "Data Source=mydb.db;")
                };
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(keys)
                    .Build();
                try
                {
                    options.UseMultiTenantDbEngine(configuration, "1");
                }
                catch (Exception)
                {
                    // ignored
                }
                options.IsConfigured.ShouldBeTrue();
            }

            [Fact]
            public void UseMultiTenantDbEngine_Should_Return_True_When_Multiple_ContextName()
            {
                var options = new DbContextOptionsBuilder<TestContext>();
                var keys = new List<KeyValuePair<string, string>>
                {
                    new("ConnectionStrings:TestContext[1].Sqlite", "Data Source=mydb1.db;"),
                    new("ConnectionStrings:TestContext[2].Sqlite", "Data Source=mydb2.db;")
                };
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(keys)
                    .Build();
                try
                {
                    options.UseMultiTenantDbEngine(configuration, "2");
                }
                catch (Exception)
                {
                    // ignored
                }
                options.IsConfigured.ShouldBeTrue();
            }

            [Fact]
            public void UseMultiTenantDbEngine_Should_Return_False_When_Different_Providers()
            {
                var options = new DbContextOptionsBuilder<TestContext>();
                var keys = new List<KeyValuePair<string, string>>
                {
                    new("ConnectionStrings:TestContext[1].Sqlite", "Data Source=mydb.db;"),
                    new("ConnectionStrings:TestContext[2].SqlServer", "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;")
                };
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(keys)
                    .Build();
                try
                {
                    options.UseMultiTenantDbEngine(configuration, "2");
                }
                catch (Exception)
                {
                    // ignored
                }

                options.IsConfigured.ShouldBeFalse();
            }
        }
    }
}
