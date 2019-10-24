using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MQTTnet.Internal;

namespace MQTTnet.Tests
{
    [TestClass]
    public class ConcurrentDictionaryExtensions_Tests
    {
        [TestMethod]
        public void TestGetNonBlocking()
        {
            var dictionary = new ConcurrentDictionary<int, int>();
            Assert.AreEqual(dictionary.Count, dictionary.GetNonBlocking(x => x.Count));
            Assert.AreEqual(dictionary.IsEmpty, dictionary.GetNonBlocking(x => x.IsEmpty));
            Assert.IsTrue(dictionary.Keys.SequenceEqual(dictionary.GetNonBlocking(x => x.Keys)));
            Assert.IsTrue(dictionary.Values.SequenceEqual(dictionary.GetNonBlocking(x => x.Values)));

            dictionary[0] = 0; dictionary[1] = 1; dictionary[2] = 2;
            Assert.AreEqual(dictionary.Count, dictionary.GetNonBlocking(x => x.Count));
            Assert.AreEqual(dictionary.IsEmpty, dictionary.GetNonBlocking(x => x.IsEmpty));
            Assert.IsTrue(dictionary.Keys.SequenceEqual(dictionary.GetNonBlocking(x => x.Keys)));
            Assert.IsTrue(dictionary.Values.SequenceEqual(dictionary.GetNonBlocking(x => x.Values)));
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void CallingMethodInPropertySelectorShouldThrowException()
        {
            var dictionary = new ConcurrentDictionary<int, int>();
            dictionary.GetNonBlocking(x => x.Count());
        }

        [TestMethod]
        public async Task GetNonBlockingValuesShouldNotRaiseAnyExceptions()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            var dictionary = new ConcurrentDictionary<int, int>();
            var changeTask = DictionaryChangeTask(cts.Token, dictionary);

            while (cts.Token.IsCancellationRequested == false)
            {
                foreach (var value in dictionary.GetNonBlocking(x => x.Values)) ;
            }
            await changeTask;
        }

        private static Task DictionaryChangeTask(CancellationToken cts, ConcurrentDictionary<int, int> dictionary)
        {
            return Task.Run(() =>
            {
                var random = new Random();
                while (cts.IsCancellationRequested == false)
                {
                    dictionary.TryRemove(dictionary.Select(x => x.Key).FirstOrDefault(), out var _);
                    var value = random.Next(10000);
                    dictionary[value] = value;
                }
            });
        }
    }
}