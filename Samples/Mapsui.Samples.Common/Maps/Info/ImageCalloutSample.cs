﻿using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Samples.Common.Maps.Geometries;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets.InfoWidgets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#pragma warning disable IDISP001 // Dispose created

namespace Mapsui.Samples.Common.Maps.Info;

public class ImageCalloutSample : ISample
{
    private static readonly Random _random = new(1);

    public string Name => "Image Callout";
    public string Category => "Info";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreatePointLayer());
        map.Navigator.CenterOnAndZoomTo(map.Layers[1].Extent!.Centroid, map.Navigator.Resolutions[5]);

        map.Widgets.Add(new MapInfoWidget(map));
        map.Info += MapOnInfo;

        return Task.FromResult(map);
    }

    private static void MapOnInfo(object? sender, MapInfoEventArgs e)
    {
        var calloutStyle = e.MapInfo?.Feature?.Styles.OfType<CalloutStyle>().FirstOrDefault();
        if (calloutStyle is not null)
        {
            calloutStyle.Enabled = !calloutStyle.Enabled;
            e.MapInfo?.Layer?.DataHasChanged();
        }
    }

    private static Layer CreatePointLayer()
    {
        return new Layer
        {
            Name = "Point with callout",
            DataSource = new MemoryProvider(GetCitiesFromEmbeddedResource()),
            IsMapInfoLayer = true
        };
    }

    private static IEnumerable<IFeature> GetCitiesFromEmbeddedResource()
    {
        const string path = "Mapsui.Samples.Common.GeoData.Json.congo.json";
        var assembly = typeof(PointsSample).GetTypeInfo().Assembly;
        using var stream = assembly.GetManifestResourceStream(path) ?? throw new NullReferenceException();
        var cities = DeserializeFromStream(stream);

        return cities.Select(c =>
        {
            var feature = new PointFeature(SphericalMercator.FromLonLat(c.Lng, c.Lat).ToMPoint());
            feature["name"] = c.Name;
            feature["country"] = c.Country;

            var calloutStyle = CreateCalloutStyle("embedded://Mapsui.Samples.Common.Images.loc.png");
            feature.Styles.Add(calloutStyle);
            return feature;
        });
    }

    private static CalloutStyle CreateCalloutStyle(string ImageSource)
    {
        var calloutStyle = new CalloutStyle { ImageSource = ImageSource, TailPosition = _random.Next(1, 9) * 0.1f, RotateWithMap = true, Type = CalloutType.Image };
        switch (_random.Next(0, 4))
        {
            case 0:
                calloutStyle.TailAlignment = TailAlignment.Bottom;
                calloutStyle.Offset = new Offset(0, SymbolStyle.DefaultHeight * 0.5f);
                break;
            case 1:
                calloutStyle.TailAlignment = TailAlignment.Left;
                calloutStyle.Offset = new Offset(SymbolStyle.DefaultHeight * 0.5f, 0);
                break;
            case 2:
                calloutStyle.TailAlignment = TailAlignment.Top;
                calloutStyle.Offset = new Offset(0, -SymbolStyle.DefaultHeight * 0.5f);
                break;
            case 3:
                calloutStyle.TailAlignment = TailAlignment.Right;
                calloutStyle.Offset = new Offset(-SymbolStyle.DefaultHeight * 0.5f, 0);
                break;
        }
        calloutStyle.RectRadius = 10;
        calloutStyle.ShadowWidth = 4;
        calloutStyle.StrokeWidth = 0;
        calloutStyle.Enabled = false;
        return calloutStyle;
    }

    internal class City
    {
        public string? Country { get; set; }
        public string? Name { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    private static List<City> DeserializeFromStream(Stream stream)
    {
        return JsonSerializer.Deserialize(stream, ImageCalloutSampleContext.Default.ListCity) ?? [];
    }
}

[JsonSerializable(typeof(List<ImageCalloutSample.City>))]
internal partial class ImageCalloutSampleContext : JsonSerializerContext
{
}
