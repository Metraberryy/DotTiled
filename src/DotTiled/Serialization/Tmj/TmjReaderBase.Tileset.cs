using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace DotTiled.Serialization.Tmj;

public abstract partial class TmjReaderBase
{
  internal Tileset ReadTileset(JsonElement element)
  {
    var backgroundColor = element.GetOptionalPropertyParseable<Color?>("backgroundcolor", s => Color.Parse(s, CultureInfo.InvariantCulture), null);
    var @class = element.GetOptionalProperty<string>("class", "");
    var columns = element.GetOptionalProperty<uint?>("columns", null);
    var fillMode = element.GetOptionalPropertyParseable<FillMode>("fillmode", s => s switch
    {
      "stretch" => FillMode.Stretch,
      "preserve-aspect-fit" => FillMode.PreserveAspectFit,
      _ => throw new JsonException($"Unknown fill mode '{s}'")
    }, FillMode.Stretch);
    var firstGID = element.GetOptionalProperty<uint?>("firstgid", null);
    var grid = element.GetOptionalPropertyCustom<Grid?>("grid", ReadGrid, null);
    var image = element.GetOptionalProperty<string?>("image", null);
    var imageHeight = element.GetOptionalProperty<uint?>("imageheight", null);
    var imageWidth = element.GetOptionalProperty<uint?>("imagewidth", null);
    var margin = element.GetOptionalProperty<uint?>("margin", null);
    var name = element.GetOptionalProperty<string?>("name", null);
    var objectAlignment = element.GetOptionalPropertyParseable<ObjectAlignment>("objectalignment", s => s switch
    {
      "unspecified" => ObjectAlignment.Unspecified,
      "topleft" => ObjectAlignment.TopLeft,
      "top" => ObjectAlignment.Top,
      "topright" => ObjectAlignment.TopRight,
      "left" => ObjectAlignment.Left,
      "center" => ObjectAlignment.Center,
      "right" => ObjectAlignment.Right,
      "bottomleft" => ObjectAlignment.BottomLeft,
      "bottom" => ObjectAlignment.Bottom,
      "bottomright" => ObjectAlignment.BottomRight,
      _ => throw new JsonException($"Unknown object alignment '{s}'")
    }, ObjectAlignment.Unspecified);
    var properties = element.GetOptionalPropertyCustom("properties", ReadProperties, []);
    var source = element.GetOptionalProperty<string?>("source", null);
    var spacing = element.GetOptionalProperty<uint?>("spacing", null);
    var tileCount = element.GetOptionalProperty<uint?>("tilecount", null);
    var tiledVersion = element.GetOptionalProperty<string?>("tiledversion", null);
    var tileHeight = element.GetOptionalProperty<uint?>("tileheight", null);
    var tileOffset = element.GetOptionalPropertyCustom<TileOffset?>("tileoffset", ReadTileOffset, null);
    var tileRenderSize = element.GetOptionalPropertyParseable<TileRenderSize>("tilerendersize", s => s switch
    {
      "tile" => TileRenderSize.Tile,
      "grid" => TileRenderSize.Grid,
      _ => throw new JsonException($"Unknown tile render size '{s}'")
    }, TileRenderSize.Tile);
    var tiles = element.GetOptionalPropertyCustom<List<Tile>>("tiles", ReadTiles, []);
    var tileWidth = element.GetOptionalProperty<uint?>("tilewidth", null);
    var transparentColor = element.GetOptionalPropertyParseable<Color?>("transparentcolor", s => Color.Parse(s, CultureInfo.InvariantCulture), null);
    var type = element.GetOptionalProperty<string?>("type", null);
    var version = element.GetOptionalProperty<string?>("version", null);
    var transformations = element.GetOptionalPropertyCustom<Transformations?>("transformations", ReadTransformations, null);
    var wangsets = element.GetOptionalPropertyCustom<List<Wangset>?>("wangsets", el => el.GetValueAsList<Wangset>(e => ReadWangset(e)), null);

    if (source is not null)
    {
      var resolvedTileset = _externalTilesetResolver(source);
      resolvedTileset.FirstGID = firstGID;
      resolvedTileset.Source = source;
      return resolvedTileset;
    }

    var imageModel = image is not null ? new Image
    {
      Format = Helpers.ParseImageFormatFromSource(image),
      Source = image,
      Height = imageHeight,
      Width = imageWidth,
      TransparentColor = transparentColor
    } : null;

    return null;
    // return new Tileset
    // {
    //   Class = @class,
    //   Columns = columns,
    //   FillMode = fillMode,
    //   FirstGID = firstGID,
    //   Grid = grid,
    //   Image = imageModel,
    //   Margin = margin,
    //   Name = name,
    //   ObjectAlignment = objectAlignment,
    //   Properties = properties,
    //   Source = source,
    //   Spacing = spacing,
    //   TileCount = tileCount,
    //   TiledVersion = tiledVersion,
    //   TileHeight = tileHeight,
    //   TileOffset = tileOffset,
    //   RenderSize = tileRenderSize,
    //   Tiles = tiles,
    //   TileWidth = tileWidth,
    //   Version = version,
    //   Wangsets = wangsets,
    //   Transformations = transformations
    // };
  }

  internal static Transformations ReadTransformations(JsonElement element)
  {
    var hFlip = element.GetOptionalProperty<bool>("hflip", false);
    var vFlip = element.GetOptionalProperty<bool>("vflip", false);
    var rotate = element.GetOptionalProperty<bool>("rotate", false);
    var preferUntransformed = element.GetOptionalProperty<bool>("preferuntransformed", false);

    return new Transformations
    {
      HFlip = hFlip,
      VFlip = vFlip,
      Rotate = rotate,
      PreferUntransformed = preferUntransformed
    };
  }

  internal static Grid ReadGrid(JsonElement element)
  {
    var orientation = element.GetOptionalPropertyParseable<GridOrientation>("orientation", s => s switch
    {
      "orthogonal" => GridOrientation.Orthogonal,
      "isometric" => GridOrientation.Isometric,
      _ => throw new JsonException($"Unknown grid orientation '{s}'")
    }, GridOrientation.Orthogonal);
    var height = element.GetRequiredProperty<uint>("height");
    var width = element.GetRequiredProperty<uint>("width");

    return new Grid
    {
      Orientation = orientation,
      Height = height,
      Width = width
    };
  }

  internal static TileOffset ReadTileOffset(JsonElement element)
  {
    var x = element.GetRequiredProperty<int>("x");
    var y = element.GetRequiredProperty<int>("y");

    return new TileOffset
    {
      X = x,
      Y = y
    };
  }

  internal List<Tile> ReadTiles(JsonElement element) =>
    element.GetValueAsList<Tile>(e =>
    {
      var animation = e.GetOptionalPropertyCustom<List<Frame>?>("animation", e => e.GetValueAsList<Frame>(ReadFrame), null);
      var id = e.GetRequiredProperty<uint>("id");
      var image = e.GetOptionalProperty<string?>("image", null);
      var imageHeight = e.GetOptionalProperty<uint?>("imageheight", null);
      var imageWidth = e.GetOptionalProperty<uint?>("imagewidth", null);
      var x = e.GetOptionalProperty<uint>("x", 0);
      var y = e.GetOptionalProperty<uint>("y", 0);
      var width = e.GetOptionalProperty<uint>("width", imageWidth ?? 0);
      var height = e.GetOptionalProperty<uint>("height", imageHeight ?? 0);
      var objectGroup = e.GetOptionalPropertyCustom<ObjectLayer?>("objectgroup", e => ReadObjectLayer(e), null);
      var probability = e.GetOptionalProperty<float>("probability", 0.0f);
      var properties = e.GetOptionalPropertyCustom("properties", ReadProperties, []);
      // var terrain, replaced by wangsets
      var type = e.GetOptionalProperty<string>("type", "");

      var imageModel = image != null ? new Image
      {
        Format = Helpers.ParseImageFormatFromSource(image),
        Source = image,
        Height = imageHeight ?? 0,
        Width = imageWidth ?? 0
      } : null;

      return new Tile
      {
        Animation = animation,
        ID = id,
        Image = imageModel,
        X = x,
        Y = y,
        Width = width,
        Height = height,
        ObjectLayer = objectGroup,
        Probability = probability,
        Properties = properties,
        Type = type
      };
    });

  internal static Frame ReadFrame(JsonElement element)
  {
    var duration = element.GetRequiredProperty<uint>("duration");
    var tileID = element.GetRequiredProperty<uint>("tileid");

    return new Frame
    {
      Duration = duration,
      TileID = tileID
    };
  }

  internal Wangset ReadWangset(JsonElement element)
  {
    var @clalss = element.GetOptionalProperty<string>("class", "");
    var colors = element.GetOptionalPropertyCustom<List<WangColor>>("colors", e => e.GetValueAsList<WangColor>(el => ReadWangColor(el)), []);
    var name = element.GetRequiredProperty<string>("name");
    var properties = element.GetOptionalPropertyCustom("properties", ReadProperties, []);
    var tile = element.GetOptionalProperty<int>("tile", 0);
    var type = element.GetOptionalProperty<string>("type", "");
    var wangTiles = element.GetOptionalPropertyCustom<List<WangTile>>("wangtiles", e => e.GetValueAsList<WangTile>(ReadWangTile), []);

    return new Wangset
    {
      Class = @clalss,
      WangColors = colors,
      Name = name,
      Properties = properties,
      Tile = tile,
      WangTiles = wangTiles
    };
  }

  internal WangColor ReadWangColor(JsonElement element)
  {
    var @class = element.GetOptionalProperty<string>("class", "");
    var color = element.GetRequiredPropertyParseable<Color>("color", s => Color.Parse(s, CultureInfo.InvariantCulture));
    var name = element.GetRequiredProperty<string>("name");
    var probability = element.GetOptionalProperty<float>("probability", 1.0f);
    var properties = element.GetOptionalPropertyCustom("properties", ReadProperties, []);
    var tile = element.GetOptionalProperty<int>("tile", 0);

    return new WangColor
    {
      Class = @class,
      Color = color,
      Name = name,
      Probability = probability,
      Properties = properties,
      Tile = tile
    };
  }

  internal static WangTile ReadWangTile(JsonElement element)
  {
    var tileID = element.GetRequiredProperty<uint>("tileid");
    var wangID = element.GetOptionalPropertyCustom<List<byte>>("wangid", e => e.GetValueAsList<byte>(el => (byte)el.GetUInt32()), []);

    return new WangTile
    {
      TileID = tileID,
      WangID = [.. wangID]
    };
  }
}
