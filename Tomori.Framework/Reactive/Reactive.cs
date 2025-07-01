// This code is part of the Tomori framework project. Licensed under the MIT License.
// See the LICENSE file for full license text.

#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Tomori.Framework.Reactive;

/// <summary>
/// The minimum implementation of a reactive object that can be bound to other reactive objects.
/// When the value of this object changes, it will raise an event.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Reactive<T> : IReactive<T>
{
    /// <summary>
    /// The event that will be raised when the value of the reactive object changes.
    /// </summary>
    public event Action<ValueChangedEvent<T>> ValueChanged = delegate { };

    private T value;
    private readonly List<IReactive<T>> bindings = new List<IReactive<T>>();
    private readonly T defaultValue;

    public T Default => defaultValue;

    public Reactive(T defaultValue)
    {
        this.defaultValue = defaultValue;
        value = defaultValue;
    }

    private bool disabled;

    public bool Disabled
    {
        get => disabled;
        set => disabled = value;
    }

    public virtual T Value
    {
        get => value;
        set
        {
            if (Disabled)
                return;

            if (EqualityComparer<T>.Default.Equals(this.value, value))
                return;

            T oldValue = this.value;
            this.value = value;

            TriggerValueChanged(oldValue, value);
        }
    }

    public bool IsDefault => EqualityComparer<T>.Default.Equals(value, defaultValue);

    public void BindTo(IReactive<T> other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));
        if (other == this)
            throw new InvalidOperationException("Cannot bind the reactive object to itself.");
        if (bindings.Contains(other))
            return; // Already bound to this source.

        bindings.Add(other);
        other.ValueChanged += OnBoundValueChanged;

        setValueFromBinding(other.Value);
    }

    public void UnbindFrom(IReactive<T> other)
    {
        other.ValueChanged -= OnBoundValueChanged;
        bindings.Remove(other);
    }

    public void UnbindAll()
    {
        foreach (var binding in bindings)
        {
            binding.ValueChanged -= OnBoundValueChanged;
        }
        bindings.Clear();
    }

    private void OnBoundValueChanged(ValueChangedEvent<T> e)
    {
        // When any bound source changes, update this reactive object's value.
        // The new value is whatever changed last.
        setValueFromBinding(e.NewValue);
    }

    /// <summary>
    /// Parses the input object into the value of this reactive object.
    /// </summary>
    /// <param name="input">The input object to parse.</param>
    /// <exception cref="InvalidOperationException">Thrown if the reactive object is bound to another source or if parsing fails.</exception>
    public void Parse(object input)
    {
        if (Disabled)
            return;

        if (input == null)
        {
            Value = Default;
            return;
        }

        try
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter.CanConvertFrom(input.GetType()))
            {
                Value = (T)converter.ConvertFrom(input);
            }
            else
            {
                Value = (T)Convert.ChangeType(input, typeof(T), CultureInfo.InvariantCulture);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse input '{input}' into type '{typeof(T)}'.", ex);
        }
    }

    private void setValueFromBinding(T newValue)
    {
        if (Disabled)
            return;

        if (EqualityComparer<T>.Default.Equals(value, newValue))
            return;

        T oldValue = value;
        value = newValue;

        TriggerValueChanged(oldValue, newValue);
    }

    protected virtual void TriggerValueChanged(T oldValue, T newValue)
    {
        ValueChanged.Invoke(new ValueChangedEvent<T>(oldValue, newValue));
    }

    public override string ToString()
    {
        return $"{GetType().Name}(Value: {value}, Default: {defaultValue}, Disabled: {disabled})";
    }

    public static implicit operator T(Reactive<T> reactive) => reactive.Value;
}
