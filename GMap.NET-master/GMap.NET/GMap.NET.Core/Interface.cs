using GMap.NET.MapProviders;

namespace GMap.NET
{
    public interface Interface
    {
        PointLatLng MapPosition
        {
            get;
            set;
        }

        GPoint MapPositionPixel
        {
            get;
        }

        string MapCacheLocation
        {
            get;
            set;
        }

        bool IsDragging
        {
            get;
        }

        RectLatLng MapViewArea
        {
            get;
        }

        GMapProvider MapProvider
        {
            get;
            set;
        }

        bool MapCanDragMap
        {
            get;
            set;
        }

        RenderMode RenderMode
        {
            get;
        }

        // events
        event PositionChanged OnPositionChanged;
        event TileLoadComplete OnTileLoadComplete;
        event TileLoadStart OnTileLoadStart;
        event MapDrag OnMapDrag;
        event MapZoomChanged OnMapZoomChanged;
        event MapTypeChanged OnMapTypeChanged;

        void MapReload();

        PointLatLng MapFromLocalToLatLng(int x, int y);
        GPoint MapFromLatLngToLocal(PointLatLng point);

#if SQLite
        bool ShowExportDialog();
        bool ShowImportDialog();
#endif
    }
}
