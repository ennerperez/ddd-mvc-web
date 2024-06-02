using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Models
{
    public class Result
    {
        internal Result(bool succeeded, IEnumerable<string> errors)
        {
            Succeeded = succeeded;
            Errors = errors.ToArray();
        }

        public bool Succeeded { get; set; }

        public string[] Errors { get; set; }

        public static Result Success() => new(true, Array.Empty<string>());

        public static Result Failure(IEnumerable<string> errors) => new(false, errors);
    }
}
