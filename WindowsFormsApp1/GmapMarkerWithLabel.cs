using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System.Runtime.Serialization.Formatters.Binary;
//using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization;



namespace WindowsFormsApp1
{
    public class GmapMarkerWithLabel : GMarkerGoogle, ISerializable
    {
        private Font font;
        private GMarkerGoogle innerMarker;

        public string Caption;

        public GmapMarkerWithLabel(PointLatLng p, string caption, GMarkerGoogleType type)
            : base(p,type)
        {
            font = new Font("Arial", 14);
            innerMarker = new GMarkerGoogle(p, type);
            Caption = caption;
        }

        public override void OnRender(Graphics g)
        {
            if (innerMarker != null)
            {
                innerMarker.OnRender(g);
            }

            g.DrawString(Caption, font, Brushes.Black, new PointF(0.0f, innerMarker.Size.Height));
        }

        public override void Dispose()
        {
            if (innerMarker != null)
            {
                innerMarker.Dispose();
                innerMarker = null;
            }

            base.Dispose();
        }

        #region ISerializable Members

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        protected GmapMarkerWithLabel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }

    public class GmapMarkerWithLabel2 : GMapMarker, ISerializable
    {
        private Font font;
        private GMarkerGoogle innerMarker;

        public string Caption;

        public GmapMarkerWithLabel2(PointLatLng p, string caption, GMarkerGoogleType type)
            : base(p)
        {
            font = new Font("Arial", 14);
            innerMarker = new GMarkerGoogle(p, type);
            Caption = caption;
        }

        public override void OnRender(Graphics g)
        {
            if (innerMarker != null)
            {
                innerMarker.OnRender(g);
            }

            g.DrawString(Caption, font, Brushes.Black, new PointF((float)innerMarker.Position.Lat,(float)innerMarker.Position.Lng));
        }

        public override void Dispose()
        {
            if (innerMarker != null)
            {
                innerMarker.Dispose();
                innerMarker = null;
            }

            base.Dispose();
        }

        #region ISerializable Members

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        protected GmapMarkerWithLabel2(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }

    public class GMapMarkerRect : GMapMarker, ISerializable
    {
        [NonSerialized]
        public Pen Pen;

        [NonSerialized]
        public GMarkerGoogle InnerMarker;

        public GMapMarkerRect(PointLatLng p)
           : base(p)
        {
            Pen = new Pen(Brushes.Blue, 5);

            // do not forget set Size of the marker
            // if so, you shall have no event on it ;}
            Size = new System.Drawing.Size(111, 111);
            Offset = new System.Drawing.Point(-Size.Width / 2, -Size.Height / 2);
        }

        public override void OnRender(Graphics g)
        {
            g.DrawRectangle(Pen, new System.Drawing.Rectangle(LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height));
        }

        public override void Dispose()
        {
            if (Pen != null)
            {
                Pen.Dispose();
                Pen = null;
            }

            if (InnerMarker != null)
            {
                InnerMarker.Dispose();
                InnerMarker = null;
            }

            base.Dispose();
        }

        #region ISerializable Members

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        protected GMapMarkerRect(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
        }

        #endregion
    }

}
