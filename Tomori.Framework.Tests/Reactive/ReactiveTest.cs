// This code is part of the Tomori framework project. Licensed under the MIT License.
// See the LICENSE file for full license text.

using NUnit.Framework;
using Tomori.Framework.Reactive;
using Tomori.Framework.Logging;

namespace Tomori.Framework.Tests.Reactive;

[TestFixture]
public class ReactiveTest
{
    [SetUp]
    public void Setup()
    {
        Logger.Initialize();
    }

    [Test]
    public void TestInitialValue()
    {
        var reactive = new Reactive<int>(10);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(reactive.Value, Is.EqualTo(10));
            Assert.That(reactive.Default, Is.EqualTo(10));
        }
    }

    [Test]
    public void TestValueChangedEvent()
    {
        string startValue = "anon";
        var reactive = new Reactive<string>(startValue);
        string eventOldValue = null;
        string eventNewValue = null;
        int firedCount = 0;

        reactive.ValueChanged += e =>
        {
            firedCount++;
            eventOldValue = e.OldValue;
            eventNewValue = e.NewValue;
        };

        reactive.Value = "anon tokyo";
        using (Assert.EnterMultipleScope())
        {
            Assert.That(firedCount, Is.EqualTo(1));
            Assert.That(eventOldValue, Is.EqualTo(startValue));
            Assert.That(eventNewValue, Is.EqualTo("anon tokyo"));
            Assert.That(reactive.Value, Is.EqualTo("anon tokyo"));
        }

        Logger.LogPrint("Current value: " + firedCount);

        // Fire again with the same value
        reactive.Value = "anon tokyo";
        Assert.That(firedCount, Is.EqualTo(1), "ValueChanged should not fire when the value does not change");
    }
}
