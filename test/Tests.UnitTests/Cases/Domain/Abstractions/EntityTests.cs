using System;
using Domain.Abstractions;
using Shouldly;
using Xunit;

namespace Tests.UnitTests.Cases.Domain.Abstractions
{
    public class EntityTests
    {
        private class TestEntity : Entity
        {
        }

        private class TestCustomKeyTypeEntity : Entity<Guid>
        {
        }

        [Fact]
        public void CustomEntity_WithCustomKeyType()
        {
            var entity = new TestCustomKeyTypeEntity();
            entity.Id = Guid.NewGuid();

            entity.Id.ShouldNotBe(Guid.Empty);
            entity.Id.ShouldBeOfType<Guid>();
        }

        [Fact]
        public void Entity_Equality_Comparison()
        {
            var entity1 = new TestEntity { Id = 1 };
            var entity2 = new TestEntity { Id = 1 };
            var entity3 = new TestEntity { Id = 2 };

            entity1.Equals(entity2).ShouldBeTrue();
            entity1.Equals(entity3).ShouldBeFalse();
            entity2.Equals(entity3).ShouldBeFalse();
        }


        [Fact]
        public void Entity_GetHashCode_ReturnsSameValueForEqualEntities()
        {
            var entity1 = new TestEntity { Id = 1 };
            var entity2 = new TestEntity { Id = 1 };
            var entity3 = new TestEntity { Id = 2 };

            entity1.GetHashCode().ShouldBe(entity2.GetHashCode());
            entity1.GetHashCode().ShouldNotBe(entity3.GetHashCode());
            entity2.GetHashCode().ShouldNotBe(entity3.GetHashCode());
        }

        [Fact]
        public void Entity_Equality_WithNull()
        {
            var entity = new TestEntity { Id = 1 };
            TestEntity nullEntity = null;

            Assert.NotEqual(entity, nullEntity);
            Assert.NotEqual(nullEntity, entity);
        }
    }
}
