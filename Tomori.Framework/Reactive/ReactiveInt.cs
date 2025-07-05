// This code is part of the Tomori framework project. Licensed under the MIT License.
// See the LICENSE file for full license text.

namespace Tomori.Framework.Reactive;

public class ReactiveInt : ReactiveNumber<int>
{
    public ReactiveInt(int defaultValue = 0) : base(defaultValue)
    {
    }
}
