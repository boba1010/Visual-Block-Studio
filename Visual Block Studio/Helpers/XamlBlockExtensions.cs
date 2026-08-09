using System.Numerics;
using Visual_Block_Studio.Models;

namespace Visual_Block_Studio.Helpers;

public static class XamlBlockExtensions
{
    /// <summary>
    /// Recursively shifts a block and its entire visual child hierarchy 
    /// by a specific dimensional movement vector.
    /// </summary>
    public static void Translate(this XamlBlock block, Vector2 delta)
    {
        block.Position += delta;

        // Loop through children safely using structural recursion
        foreach (var child in block.Children)
        {
            child.Translate(delta);
        }
    }
}