using Microsoft.UI.Xaml.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Visual_Block_Studio;

public class NavItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? RegularTemplate { get; set; }
    public DataTemplate? FooterTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        if (item is Block model)
            return FooterTemplate;
        return RegularTemplate;
    }
}