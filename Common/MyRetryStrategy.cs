using Couchbase.Core.Retry;

namespace CouchbaseWebAPI.Common
{
    public class MyRetryStrategy : IRetryStrategy
    {
        private readonly IBackoffCalculator _backoffCalculator;

        public MyRetryStrategy() :
            this(ExponentialBackoff.Create(10, 1, 500))
        {
        }

        public MyRetryStrategy(IBackoffCalculator calculator)
        {
            _backoffCalculator = calculator;
        }

        public RetryAction RetryAfter(IRequest request, RetryReason reason)
        {
            if (reason == RetryReason.NoRetry)
            {
                return RetryAction.Duration(null);
            }
            else if(reason == RetryReason.QueryPreparedStatementFailure)
            {
                throw new System.Exception(reason.ToString());
            }
            if (request.Idempotent || reason.AllowsNonIdempotentRetries())
            {
                var backoffDuration = _backoffCalculator.CalculateBackoff(request);
                return RetryAction.Duration(backoffDuration);
            }

            return RetryAction.Duration(null);
        }
    }
}