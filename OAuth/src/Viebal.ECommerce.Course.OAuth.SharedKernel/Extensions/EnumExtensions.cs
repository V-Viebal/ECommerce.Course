
using Microsoft.OpenApi.Extensions;
using System.ComponentModel;

namespace Viebal.ECommerce.Course.OAuth.SharedKernel.Extensions;

static class EnumExtensions
{
    public static string GetDescription(this Enum enumValue)
    {
        var attribute = enumValue.GetAttributeOfType<DescriptionAttribute>();
        return attribute is not null ? attribute.Description : enumValue.ToString();
    }
}
