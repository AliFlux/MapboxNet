using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MapboxNetCore
{
    public interface IMap
    {
        event EventHandler Ready;
        event EventHandler Styled;
        event EventHandler Render;
        event EventHandler CenterChanged;
        event EventHandler ZoomChanged;
        event EventHandler PitchChanged;
        event EventHandler BearingChanged;
        event EventHandler Reloading;

        string AccessToken { get; set; }
        object MapStyle { get; set; }
        bool RemoveAttribution { get; set; }
        MapboxNetCore.GeoLocation Center { get; set; }
        double Zoom { get; set; }
        double Pitch { get; set; }
        double Bearing { get; set; }
        bool IsReady { get; }

        dynamic Invoke { get; }
        dynamic SoftInvoke { get; }
        dynamic LazyInvoke { get; }

        object SoftExecute(string expression);
        object Execute(string expression);
        Task<object> ExecuteAsync(string expression);

        void AddImage(string id, MemoryStream stream);
        void AddImage(string id, string base64);

        Point2D Project(GeoLocation location);
        GeoLocation UnProject(Point2D point);
    }
}
