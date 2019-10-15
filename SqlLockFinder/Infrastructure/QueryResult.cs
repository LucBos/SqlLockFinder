using System.Collections.Generic;
using System.Linq;

namespace SqlLockFinder.Infrastructure
{

    public class QueryResult
    {
        public QueryResult()
        {
            Warnings = new List<string>();
            Errors = new List<string>();
        }

        public List<string> Warnings { get; }
        public List<string> Errors { get; }
        public bool Faulted => Errors.Any() || Warnings.Any();
    }
    public class QueryResult<T>: QueryResult
    {

        public T Result { get; set; }
        
        public bool HasValue => Result != null;
    }
}
