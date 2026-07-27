using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace GearZone.Api.OData;

public static class ODataEdmModel
{
    public static IEdmModel Build()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<CatalogProductODataDto>("CatalogProducts");
        return builder.GetEdmModel();
    }
}
