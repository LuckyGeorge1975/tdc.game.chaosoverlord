using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace ChaosOverlords.App;

/// <summary>
/// Resolves views for view models based on naming convention (FooViewModel → FooView).
/// </summary>
public sealed class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        if (data is null)
        {
            return new TextBlock { Text = "No view available" };
        }

        var name = data.GetType().FullName?.Replace("ViewModel", "View");
        if (name is null)
        {
            return new TextBlock { Text = "View lookup failed" };
        }

        var type = Type.GetType(name);
        if (type is null)
        {
            return new TextBlock { Text = name };
        }

        try
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        catch (Exception ex)
        {
            return new TextBlock { Text = $"Failed to create view: {type.FullName}\n{ex.Message}" };
        }
    }

    public bool Match(object? data)
        => data is not null && data.GetType().Name.EndsWith("ViewModel", StringComparison.Ordinal);
}
