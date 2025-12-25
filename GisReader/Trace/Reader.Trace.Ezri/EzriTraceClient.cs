using Microsoft.Extensions.Configuration;
using Reader.Trace.Ezri.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Reader.Trace.Ezri
{
    public class EzriTraceClient
    {
        private readonly HttpClient _client;
        private readonly string? _token = null;
        private readonly string _baseUrl;
        public EzriTraceClient(IConfiguration configuration)
        {
            _client = new HttpClient(new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
            });
            _client.Timeout = TimeSpan.FromMinutes(5);
            _token = configuration["GisServer:Token"]!;
            //if (_token == null)
            //    throw new InvalidOperationException("Token for trace has been not set.");
            _baseUrl = configuration["GisServer:TraceUrl"]!;
            if (_baseUrl == null)
                throw new InvalidOperationException("TraceUrl for trace has been not set.");
        }
        public async Task<TraceResponse> TraceAsync(NetTopologySuite.Geometries.Geometry geometry)
        {
            var json = JsonSerializer.Serialize(new { geometry.Coordinate.X, geometry.Coordinate.Y });
            //var url = $"https://gisservices.meedc.ir:2005/arcgis/rest/services/gdbNet_Services/MapServer/exts/GeometricNetworkUtility/GeometricNetworks/1/TraceNetwork?traceSolverType=FindAccumulation&flowMethod=esriFMDownstream&flowElements=esriFEJunctionsAndEdges&edgeFlags=&junctionFlags=[{json}]&edgeBarriers=&junctionBarriers=&outFields=globalid&maxTracedFeatures=310000&tolerance=1&traceIndeterminateFlow=&shortestPathObjFn=esriSPObjFnMinMax&disableLayers=&junctionWeight=&fromToEdgeWeight=&toFromEdgeWeight=&junctionFilterWeight=&junctionFilterRanges=&junctionFilterNotOperator=&fromToEdgeFilterWeight=&toFromEdgeFilterWeight=&edgeFilterRanges=&edgeFilterNotOperator=&f=pjson&token={_token}";
            //var url = $"https://gis.yed.co.ir:2943/yazd/rest/services/new_spatial/srv_net/MapServer/exts/GeometricNetworkUtility/GeometricNetworks/1/TraceNetwork/?token={_token}&f=json&traceSolverType=FindAccumulation&flowMethod=esriFMDownstream&flowElements=esriFEJunctionsAndEdges&outFields=objectid,globalid,st_length(shape)&maxTracedFeatures=2000000&tolerance=10&edgeFlags=[{json}]&junctionFlags=[]&edgeBarriers=[]&junctionBarriers=[]&traceIndeterminateFlow=false&shortestPathObjFn=esriSPObjFnMinMax";
            
            var url = string.Format(_baseUrl, json, _token);
            using var response = await _client.GetAsync(url);
            return await response.Content.ReadFromJsonAsync<TraceResponse>() ??
                throw new InvalidOperationException("Fail to create trace response object.");
        }
    }
}
