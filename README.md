# RxProcess library

![NuGet Version](https://img.shields.io/nuget/v/RxProcess)

Provides a way to run child processes/forks as observables reactively emitting stdout/stderr data.

- [RxProcess](#rxprocess)
- [RxFork](#rxfork)

## RxProcess

The sample shows how a new process is started with subscription for standard output/error data.

```cs
using System.Reactive.Linq;
using RxProcessLib;

// Starts a new process with passing an arg.
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
```

The sample produces output like:

```
Ping Out:
Ping Out: Pinging www.google.com [142.251.1.99] with 32 bytes of data:
Ping Out: Reply from 142.251.1.99: bytes=32 time=41ms TTL=106
Ping Out: Reply from 142.251.1.99: bytes=32 time=39ms TTL=106
Ping Out: Reply from 142.251.1.99: bytes=32 time=39ms TTL=106
Ping Out: Reply from 142.251.1.99: bytes=32 time=40ms TTL=106
Ping Out:
Ping Out: Ping statistics for 142.251.1.99:
Ping Out:     Packets: Sent = 4, Received = 4, Lost = 0 (0% loss),
Ping Out: Approximate round trip times in milli-seconds:
Ping Out:     Minimum = 39ms, Maximum = 41ms, Average = 39ms
Ping exit code: 0
```

## RxFork

The sample shows starting 2 forks from the master process.
Each fork receives input data from the standard input then performs a calculation and returns results via the standard output.
The master process is notified about results from subscription for the forks standard output.

```cs
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

// The delegate is run only in forks.
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
    Console.WriteLine($"FORK1 exit code = {process.ExitCode}");
});
fork2.AccessInMaster(process => 
{
    process.WaitForExit();
    Console.WriteLine($"FORK2 exit code = {process.ExitCode}");
});

RxForker.RunInMaster(() => Console.ReadKey(true));
```

The sample produces output like:

```
FORK1: 5 * 6 = 30
FORK1: exited
FORK1 exited with code = 0
FORK2: 7 * 8 = 56
FORK2: exited
FORK2 exited with code = 0
```