using NUnit.Framework;
using Superscale;

public class ClientTest
{
    private SuperDuperRememberer rememberer;
    private Client client;

    [SetUp]
    public void BeforeEach()
    {
        rememberer = new SuperDuperRememberer();
        client = new Client(rememberer);
    }

    [Test]
    public void ShouldRememberItems()
    {
        string[] words = new [] { "banana", "orange", "pomegranate" };
        // TODO: somehow remember words with client
        Assert.AreEqual(new string[] {"banana", "orange", "pomegranate"}, rememberer.Items);
    }

    // TODO: a bunch more tests?
}