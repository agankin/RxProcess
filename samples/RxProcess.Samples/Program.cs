using System.Reactive.Linq;
using RxProcessLib;

// Starts a new ping process with passing an arg.
using var ping = RxProcess.Create("ping", "www.google.com");

// Subscription for the process standard output/error and completed events.
using var _ = ping.Subscribe(
    onNext: line => Console.WriteLine($"Ping {line.Type}: {line.Value}"),
    onCompleted: () => Console.WriteLine($"Ping exit code: {ping.ExitCode}")
);

// Starts the process.
ping.Start();

// Waits for the process exit.
ping.WaitForExit();

Console.ReadKey(true);