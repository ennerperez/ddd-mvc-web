#if !USING_SWAGGER
using System;
// ReSharper disable CheckNamespace

namespace Swashbuckle.AspNetCore
{
    namespace Annotations
    {
        [AttributeUsage(AttributeTargets.Method)]
        public class SwaggerOperationAttribute : Attribute
        {
            public string Description { get; }
            public SwaggerOperationAttribute(string description)
            {
                Description = description;
            }
        }
    }
}
#endif
