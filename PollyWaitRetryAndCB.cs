using Polly;
using Polly.CircuitBreaker;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PollyPolicy.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var demo = new Demo();
            while (true)
            {
                demo.Run();
                Console.WriteLine("Hit Enter for the next Attempt");
                Console.ReadLine();
            }

        }

    }

    public class Demo
    {
        private int retries;
        

        public void Run()
        {
            retries = 0;
            var retries1 = 0;
            var cancellationToken = new CancellationTokenSource();

            var waitAndRetryPolicy1 = Policy.Handle<Exception>()
                .WaitAndRetryForever(
               sleepDurationProvider: attempt => TimeSpan.FromMinutes(30), //Notes: Wait 30 minutes between each try.
               onRetry: (exception, calculatedWaitDuration) =>
               {
                   retries1++;
                   Console.WriteLine($"Retry1 Count: {retries1}");

               });

            var waitAndRetryPolicy = Policy.Handle<Exception>(e => !(e is BrokenCircuitException))
                .WaitAndRetry(new[]
                                    {
                                        TimeSpan.FromSeconds(5),
                                        TimeSpan.FromMinutes(1),
                                        TimeSpan.FromMinutes(5)
                                    },
               onRetry: (exception, calculatedWaitDuration) =>
               {
                   retries++;
                   Console.WriteLine($"Retry Count: {retries}");
               });


            CircuitBreakerPolicy circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreaker(
                    exceptionsAllowedBeforeBreaking: 2,
                    durationOfBreak: TimeSpan.FromSeconds(1),
                    onBreak: (ex, breakDelay) =>
                    {
                        Console.WriteLine(".Breaker logging: Breaking the circuit for " + breakDelay.TotalMilliseconds + "ms!");
                        Console.WriteLine("..due to: " + ex.Message);
                    },
                    onReset: () => Console.WriteLine(".Breaker logging: Call ok! Closed the circuit again!"),
                    onHalfOpen: () => Console.WriteLine(".Breaker logging: Half-open: Next call is a trial!")
                ); ;


            PolicyWrap policyWrap = Policy.Wrap(waitAndRetryPolicy1, waitAndRetryPolicy, circuitBreakerPolicy);

            var ctx = new Context();
            policyWrap.Execute((ct, token) =>
           {
               Console.WriteLine("Please enter your choice");
               if (!token.IsCancellationRequested)
                   Execute(cancellationToken);

           }, ctx, cancellationToken.Token);


        }

        private void Execute(CancellationTokenSource cancellationTokenSource)
        {
            var input = Console.ReadLine();
            if (string.Equals(input, "fail", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("Call has failed");
            }
            else
            {
                Console.WriteLine("Call was successful");
                cancellationTokenSource.Cancel();
            }



        }

    }
}
