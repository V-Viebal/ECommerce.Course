using System.ComponentModel;

namespace Viebal.ECommerce.Course.OAuth.SharedKernel.Enums;

[Flags]
public enum ApiCategory
{
    [Description("Internal API")]
    InternalApi = 1,
    [Description("Public API")]
    PublicApi = 2
}
