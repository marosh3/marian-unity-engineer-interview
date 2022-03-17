# Super Duper Rememberer client

This project should contain a `Client` class for saving data to Super Duper Rememberer(SDR).

Your task is to go through the SDR documentation below and implement "remember data" method for `Client` class. Treat this as a client for uploading data to SDR and implement it just like you would for production(handle edge cases, **write unit tests**).

The `Client` class should be able to handle large number of input strings, remember items in the same order as they appear in the input and minimize number of calls to the SDR.


> To make the assignment simple, the repo contains implementation of SDR, but please treat is as a client for a remote system, like a database running in the cloud. Don't make any changes to SuperDuperRememberer.

# Super Duper Rememberer documentation
SDR remembers strings of any length consisting of small english letters(a-z).

To remember strings, you use Super Duper Query Language(SDQL).

SDQL only has one statement:
-  `CAN YOU PLEASE REMEMBER THESE ITEMS? [comma-separated single-quoted strings] K THX BYE.`

Example:
```
> CAN YOU PLEASE REMEMBER THESE ITEMS? 'banana','orange','pomegranate' K THX BYE.

// Once the statement is executed, the memory of SDR will contain strings `banana`, `orange`, `pomegranate`.
```




SDR is a very busy system, so if you want to run a statement you have to issue commands that will eventually be processed.

There are 2 commands:
- **ExecuteStatementCommand**: Orders the SDR to run a statement as described above. The result of the command is an object describing the statement execution e.g. `{id: "some-id", state: "RUNNING"}`
    > Statements can be at most 300 characters long
- **DescribeStatementCommand**: Asks SDR about the current state of a statement execution started with `ExecuteStatementCommand`. The result is an object describing the statement.

> Since this documentation sucks, there are equally bad tests you can take a look at in `./Assets/Scripts/Tests/SuperDuperRememberer.spec.cs` or even better, run them with Unity Test Runner.