using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using Superscale;
using UnityEngine;
using UnityEngine.TestTools;


public static class TaskWaitUntilCompleted
{
    public static WaitUntil WaitUntilCompleted(this Task task)
    {
        return new WaitUntil(() => task.IsCompleted);
    }
}

public class SuperDuperRemembererTest
{
    private SuperDuperRememberer rememberer;

    [SetUp]
    public void BeforeEach()
    {
        rememberer = new SuperDuperRememberer();
    }

    [Test]
    public void ShouldReturnEmptyArrayOfRememberedItems()
    {
        Assert.AreEqual(new string[] {}, rememberer.Items);
    }

    [UnityTest]
    public IEnumerator ShouldRememberThings()
    {
        var startedState = rememberer.ExecuteCommand(
            new ExecuteStatementCommand(
                "CAN YOU PLEASE REMEMBER THESE ITEMS? 'banana','orange','pomegranate' K THX BYE."
            )
        );
        Assert.AreEqual(startedState.status, CommandStatus.RUNNING);
        yield return new WaitForSeconds(1); // if only there was a better way to do this..
        var doneState = rememberer.ExecuteCommand(new DescribeStatementCommand(startedState.id));
        Assert.AreEqual(doneState.status, CommandStatus.DONE);
        Assert.AreEqual(new string[] {"banana", "orange", "pomegranate"}, rememberer.Items);
    }

    [UnityTest]
    public IEnumerator ShouldThrowExceptionOnInvalidStatement()
    {
        var startedState = rememberer.ExecuteCommand(
            new ExecuteStatementCommand(
                "CAN YOU PLEASE REMEMBER THESE ITEMS 'banana','orange','pomegranate' K THX BYE."
            )
        );
        Assert.AreEqual(startedState.status, CommandStatus.RUNNING);
        yield return new WaitForSeconds(1); // too slow..
        var failedState = rememberer.ExecuteCommand(new DescribeStatementCommand(startedState.id));
        Assert.AreEqual(failedState.status, CommandStatus.FAILED);
        Assert.AreEqual(failedState.error, "STATEMENT IS INVALID");
        Assert.AreEqual(new string[] {}, rememberer.Items);
    }

    [UnityTest]
    public IEnumerator ShouldThrowExceptionOnStatementTooLong()
    {
        var startedState = rememberer.ExecuteCommand(
            new ExecuteStatementCommand(
                "CAN YOU PLEASE REMEMBER THESE ITEMS? 'pneumonoultramicroscopicsilicovolcanoconiosis'," +
                "'pseudopseudohypoparathyroidism','floccinaucinihilipilification','antidisestablishmentarianism'," +
                "'supercalifragilisticexpialidocious','pneumonoultramicroscopicsilicovolcanoconiosis'," +
                "'hippopotomonstrosesquippedaliophobia' K THX BYE."
            )
        );
        Assert.AreEqual(startedState.status, CommandStatus.RUNNING);
        yield return new WaitForSeconds(1); // my god this is slow..
        var failedState = rememberer.ExecuteCommand(new DescribeStatementCommand(startedState.id));
        Assert.AreEqual(failedState.status, CommandStatus.FAILED);
        Assert.AreEqual(failedState.error, "THERE'S NO WAY TO REMEMBER ALL THAT AT ONCE!");
        Assert.AreEqual(new string[] {}, rememberer.Items);
    }
}
