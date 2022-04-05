using Superscale;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using System;
using System.Text;

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

    [UnityTest]
    public IEnumerator ShouldRememberItems()
    {
        string[] words = new [] { "banana", "orange", "pomegranate" };
        client.RememberData(words);
        client.FlushCache();
        yield return client.WaitForPendingDataCoroutine();
        AssertContentExists(words, rememberer.Items);
    }

    [UnityTest]
    public IEnumerator ShouldThrowExceptionOnWrongCharacters()
    {
        string[] words = new[] { "banana!", "orange", "pomegranate" };
        Exception exceptionThrown = null;
        client.RememberData(words);
        client.FlushCache();
        yield return RunThrowingCoroutine(client.WaitForPendingDataCoroutine(), (Exception e) => { exceptionThrown = e; });
        Assert.IsNotNull(exceptionThrown);
        Assert.AreEqual(Client.E_FAILED_COMMAND, exceptionThrown.Message);
    }

    [Test]
    public void ShouldThrowExceptionOnWordTooLong()
    {
        string[] words = new[] { 
            "orange",
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit.Duis iaculis orci id arcu ultrices, a rhoncus elit mollis.Sed ligula dolor, luctus in elementum a, hendrerit eu lacus.Vestibulum elementum sit amet orci a tempor.Sed in arcu pulvinar libero......".Replace(' ', 'a').Replace(',', 'a').Replace('.', 'a'), 
            "pomegranate" };
        Assert.AreEqual(251, words[1].Length);

        Exception exceptionThrown = null;
        try
        {
            client.RememberData(words);
        }
        catch (Exception e)
        {
            exceptionThrown = e;
        }

        Assert.IsNotNull(exceptionThrown);
        Assert.AreEqual(Client.E_WORD_TOO_LONG, exceptionThrown.Message);
    }


    [UnityTest]
    public IEnumerator ShouldRememberManyLargeItems()
    {
        int wordsCnt = 100;
        int charsCnt = 100;
        string[] words = GenerateWordsArray(wordsCnt, charsCnt, 0);
        client.RememberData(words);
        client.FlushCache();
        yield return client.WaitForPendingDataCoroutine();
        AssertContentExists(words, rememberer.Items);
    }

    [UnityTest]
    public IEnumerator ShouldRememberManyManySmallItems()
    {
        int wordsCnt = 100;
        int charsCnt = 7;
        int repeat = 10;

        string[][] words = new string[repeat][];

        for (int i = 0; i < repeat; i++)
        {
            words[i] = GenerateWordsArray(wordsCnt, charsCnt, i * wordsCnt);
            client.RememberData(words[i]);
        }

        client.FlushCache();

        yield return client.WaitForPendingDataCoroutine();

        for (int i = 0; i < repeat; i++)
        {
            AssertContentExists(words[i], rememberer.Items);
        }
    }

    private IEnumerator RunThrowingCoroutine(IEnumerator enumerator, Action<Exception> done)
    {
        while (true)
        {
            object current;
            try
            {
                if (enumerator.MoveNext() == false) break;
                current = enumerator.Current;
            }
            catch (Exception ex)
            {
                done(ex);
                yield break;
            }
            yield return current;
        }
        done(null);
    }

    void AssertContentExists(string[] a1, string[] a2)
    {
        Assert.IsNotNull(a1);
        Assert.IsNotNull(a2);

        for (int i1 = 0; i1 < a1.Length; i1++)
        {
            bool hasMatch = false;

            for (int i2 = 0; i2 < a2.Length; i2++)
            {
                if (a1[i1] == a2[i2])
                {
                    hasMatch = true;
                    break;
                }
            }

            Assert.IsTrue(hasMatch);
        }
    }

    string[] GenerateWordsArray(int wordsCnt, int charsCnt, int startNum)
    {
        string[] words = new string[wordsCnt];
        for (int i = 0; i < wordsCnt; i++)
        {
            string str0 = (i + startNum).ToString();
            char[] arr = str0.ToCharArray();
            Array.Reverse(arr);
            string str1 = new string(arr);

            StringBuilder builder = new StringBuilder(charsCnt);
            builder.Append(str1);
            for (int ii = 0; ii < builder.Length; ii++)
            {
                builder[ii] = (char)(builder[ii] - '0' + 'a');
            }

            for (int ii = builder.Length; ii < charsCnt; ii++)
            {
                builder.Append('a');
            }

            words[i] = builder.ToString();
        }
        return words;
    }
}