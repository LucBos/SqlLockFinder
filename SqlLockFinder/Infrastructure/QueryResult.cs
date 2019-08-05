using System.Collections.Generic;
using System.Linq;

namespace SqlLockFinder.Infrastructure
{
    public class QueryResult<T>
    {
        public QueryResult()
        {
            Warnings=new List<string>();
            Errors=new List<string>();
        }

        public T Result { get; set; }
        public List<string> Warnings { get; }
        public List<string> Errors { get; }
        public bool HasValue => Result != null;
        public bool Faulted => Errors.Any() || Warnings.Any();
    }
}
