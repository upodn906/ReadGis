using Microsoft.Extensions.Logging;
using Reader.Abstraction.Layers.Models;
using Reader.Infrastructures.Clients._Common.Base;

namespace Reader.Infrastructures.Clients
{
    public class EdsabClient : BaseGisClient
    {
        public EdsabClient(HttpClient client, GisClientConfiguration configuration, ILogger<BaseGisClient> logger) : base(client, configuration, logger)
        {
        }

        protected override string GenerateGetLayerObjectIdsUrlAsync(GisLayer layer)
        {
            return
                $"{Configuration.Address}/{layer.Id}/query?where=1%3D1&text=&objectIds=&time=&geometry=&geometryType=esriGeometryEnvelope&inSR=&spatialRel=esriSpatialRelIntersects&relationParam=&outFields=&returnGeometry=false&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=&outSR=&returnIdsOnly=true&returnCountOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&gdbVersion=&returnDistinctValues=false&resultOffset=&resultRecordCount=&f=pjson";
        }

        protected override string GenerateGetLayerInformationUrl(GisLayer layer)
        {
           return $"{Configuration.Address}/{layer.Id}?f=pjson";
        }

        protected override string GenerateGetLayersUrl()
        {
            return $"{Configuration.Address}?f=pjson";
        }

        protected override string GenerateGetLayerObjectsUrl(GisLayer layer, int startObjectId, int endObjectId, in string objectIdFieldName)
        {
            return $"{Configuration.Address}/{layer.Id}/query?where=+{objectIdFieldName}>={startObjectId} and {objectIdFieldName}<={endObjectId}&text=&objectIds=&time=&geometry=&geometryType=esriGeometryEnvelope&inSR=&spatialRel=esriSpatialRelIntersects&relationParam=&outFields=*&returnGeometry=true&maxAllowableOffset=&geometryPrecision=&outSR=4326&returnIdsOnly=false&returnCountOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&gdbVersion=&returnDistinctValues=false&f=pjson";
        }


        //protected override string GenerateGetLayerObjectsUrl(int layer, IEnumerable<int>? objectIds = null)
        //{
        //    var objectIdsString = string.Empty;
        //    if (objectIds != null)
        //        objectIdsString = string.Join(",", objectIds);

        //    return
        //        $"{Configuration.Address}/{layer}/query?where=1%3D1&text=&objectIds={objectIdsString}&time=&geometry=&geometryType=esriGeometryEnvelope&inSR=4326&outSR=4326&spatialRel=esriSpatialRelIntersects&relationParam=&outFields=*&returnGeometry=true&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=&outSR=4326&returnIdsOnly=false&returnCountOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&gdbVersion=&returnDistinctValues=false&resultOffset=&resultRecordCount=&f=pjson";
        //}
    }
}
