using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEditor.GUI;
using FlaxEngine;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ProceduralGraph.Interface;

/// <summary>
/// GraphComponentEditor class.
/// </summary>
[CustomEditor(typeof(IGraphComponent))]
internal sealed class GraphComponentEditor : GenericEditor
{
    internal static ImmutableDictionary<string, Func<IGraphComponent>> Factories { get; }

    private ComboBox? _comboBox;

    static GraphComponentEditor()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        Factories = assemblies.SelectMany(Types)
                              .Where(IsAssignableToT)
                              .ToImmutableDictionary(NameOf, MakeFactory);
    }

    public override void Initialize(LayoutElementsContainer layout)
    {
        _comboBox = layout.ComboBox("Type").ComboBox;
        _comboBox.Items.AddRange(Factories.Keys.Order());
       
        if (Values[0] is object target)
        {
            Type type = target.GetType();
            _comboBox.SelectedIndex = _comboBox.Items.IndexOf(NameOf(type));
            base.Initialize(layout);
        }
        else
        {
            _comboBox.SelectedIndex = -1;
        }
        
        _comboBox.SelectedIndexChanged += OnSelectionChanged;
    }

    protected override void Deinitialize()
    {
        if (_comboBox is not null)
        {
            _comboBox.SelectedIndexChanged -= OnSelectionChanged;
        }

        base.Deinitialize();
    }

    private void OnSelectionChanged(ComboBox box)
    {
        if (Factories.TryGetValue(box.SelectedItem, out var factory))
        {
            SetValue(factory());
        }
    }

    private static Func<IGraphComponent> MakeFactory(Type type)
    {
        var ctor = type.GetConstructor(Type.EmptyTypes) ?? throw new ArgumentException($"Type '{type.Name}' must have a public parameterless constructor.", nameof(type));

        var newExpression = Expression.New(ctor);
        var castExpression = Expression.Convert(newExpression, typeof(IGraphComponent));
        var lambda = Expression.Lambda<Func<IGraphComponent>>(castExpression);

        return lambda.Compile();
    }

    private static string NameOf(Type type)
    {
        return $"{(type.GetCustomAttribute<DisplayNameAttribute>() is DisplayNameAttribute attribute ? attribute.DisplayName : type.Name)} ({type.Namespace})";
    }

    private static Type[] Types(Assembly assembly)
    {
        return assembly.GetTypes();
    }

    private static bool IsAssignableToT(Type type)
    {
        return !type.IsAbstract && type.IsAssignableTo(typeof(IGraphComponent));
    }
}
