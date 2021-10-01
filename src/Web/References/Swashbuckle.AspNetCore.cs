#if !USING_SWAGGER
using System;

namespace Swashbuckle.AspNetCore
{
    namespace Annotations
    {
        [AttributeUsage(AttributeTargets.Method)]
        public class SwaggerOperationAttribute : Attribute
        {
            public SwaggerOperationAttribute(string description)
            {
            }
        }
    }
}
#endif
