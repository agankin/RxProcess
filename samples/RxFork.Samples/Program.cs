using System.Reactive.Linq;
using RxProcessLib;

// Creating 2 forks of the current master process. The calls have no effect in forks.
using var fork1 = RxForker.Fork();
using var fork2 = RxForker.Fork();

// The delegate is run only in the master process.
RxForker.RunInMaster(() => Console.WriteLine("Starting forks..."));

// These 2 subscriptions receive events only in the master process.
using var fork1Subscription = fork1.Subscribe(
    onNext: line => Console.WriteLine($"FORK1: {line.Value}"),
    onCompleted: () => Console.WriteLine("FORK1: exited")
);
using var fork2Subscription = fork2.Subscribe(
    onNext: line => Console.WriteLine($"FORK2: {line.Value}"),
    onCompleted: () => Console.WriteLine("FORK2: exited")
);

// These 2 calls have effect only in the master process and ignored in forks.
fork1.Start();
fork2.Start();

// Sends data to forks via the standard input. The calls also have no effect in forks.
fork1.SendLine("5");
fork1.SendLine("6");

fork2.SendLine("7");
fork2.SendLine("8");

// This code is run only in forks.
RxForker.RunInFork(() =>
{
    var x = int.Parse(Console.ReadLine()!);
    var y = int.Parse(Console.ReadLine()!);

    Console.WriteLine($"{x} * {y} = {x * y}");
});

// Accessing underlying fork's process in the master.
fork1.AccessInMaster(process => 
{
    process.WaitForExit();
    Console.WriteLine($"FORK1 exited with code = {process.ExitCode}");
});
fork2.AccessInMaster(process => 
{
    process.WaitForExit();
    Console.WriteLine($"FORK2 exited with code = {process.ExitCode}");
});

RxForker.RunInMaster(() => Console.ReadKey(true));