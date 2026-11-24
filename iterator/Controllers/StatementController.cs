using System.Diagnostics;
using iterator.Bank;
using Microsoft.AspNetCore.Mvc;

namespace iterator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StatementController : ControllerBase
    {

        private readonly FakeBankApi _api;

        public StatementController(FakeBankApi api)
        {
            _api = api;
        }

        /// <summary>
        /// Whole month streamed to the client - ASP.NET Core writes every transaction as soon as the iterator returns it.
        /// </summary>
        [HttpGet]
        public IAsyncEnumerable<Transaction> GetAll(CancellationToken cancellationToken) =>
            TransactionHistory.ReadAll(_api, cancellationToken);

        [HttpGet("summary")]
        public async Task<ActionResult> GetSummary(CancellationToken cancellationToken)
        {
            decimal income = 0, expenses = 0;
            await foreach (var transaction in new TransactionHistory(_api).WithCancellation(cancellationToken))
            {
                if (transaction.Amount > 0) income += transaction.Amount;
                else expenses += transaction.Amount;
            }
            return Ok(new { Income = income, Expenses = expenses, Balance = income + expenses, RequestsToBankSoFar = _api.Requests });
        }

        /// <summary>
        /// Three use cases, each with a fresh bank API, so you can see how many pages were really downloaded.
        /// </summary>
        [HttpGet("demo")]
        public async Task<ActionResult> Demo(CancellationToken cancellationToken)
        {
            var latest5 = await Measure(async history =>
            {
                var result = new List<Transaction>();
                await foreach (var transaction in history.WithCancellation(cancellationToken))
                {
                    result.Add(transaction);
                    if (result.Count == 5) break;   // iterator stops here, next pages are never downloaded
                }
                return result;
            });

            var latestIncome = await Measure(async history =>
            {
                await foreach (var transaction in history.WithCancellation(cancellationToken))
                    if (transaction.Amount > 0) return transaction;
                return null;
            });

            var summary = await Measure(async history =>
            {
                decimal income = 0, expenses = 0;
                var count = 0;
                await foreach (var transaction in history.WithCancellation(cancellationToken))
                {
                    count++;
                    if (transaction.Amount > 0) income += transaction.Amount;
                    else expenses += transaction.Amount;
                }
                return new { Transactions = count, Income = income, Expenses = expenses, Balance = income + expenses };
            });

            return Ok(new
            {
                PageSize = FakeBankApi.PageSize,
                LatestFiveTransactions = latest5,
                LatestIncome = latestIncome,
                MonthlySummary = summary
            });
        }

        private static async Task<object> Measure<T>(Func<TransactionHistory, Task<T>> useCase)
        {
            var api = new FakeBankApi();
            var stopwatch = Stopwatch.StartNew();
            var result = await useCase(new TransactionHistory(api));
            return new { RequestsToBank = api.Requests, ElapsedMs = stopwatch.ElapsedMilliseconds, Result = result };
        }

    }
}
