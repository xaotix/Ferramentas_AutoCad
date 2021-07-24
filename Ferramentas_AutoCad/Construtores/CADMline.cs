﻿using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Ferramentas_DLM
{
    public class CADMline
    {
        private MlineStyle _mlstyle { get; set; }
        public MlineStyle GetStyle()
        {
            if(_mlstyle==null)
            {
                _mlstyle = Multiline.GetMlStyle(this.Mline.Style);
            }
            return _mlstyle;
        }
        public List<Point2d> GetInterSeccao(Entity line)
        {
            Point3dCollection pts = new Point3dCollection();
            this.GetPLineDummy().IntersectWith(line, Autodesk.AutoCAD.DatabaseServices.Intersect.ExtendBoth, pts, new IntPtr(), new IntPtr());

            List<Point2d> ptss = new List<Point2d>();

            foreach(Point3d p in pts)
            {
                ptss.Add(new Point2d(p.X,p.Y));
            }
            return ptss;
        }
        public System.Windows.Shapes.Line GetCanvas(System.Windows.Point p0, double escala, double espessura, SolidColorBrush cor)
        {
            var p1 = new System.Windows.Point((this.Inicio.X - p0.X) * escala, (this.Inicio.Y - p0.Y) * escala);
            var p2 = new System.Windows.Point((this.Fim.X - p0.X) * escala, (this.Fim.Y - p0.Y) * escala);
            var l = Conexoes.FuncoesCanvas.Linha(p1, p2, cor, espessura, Conexoes.FuncoesCanvas.TipoLinha.Continua);

            return l;
        }
        public Tipo_Multiline Tipo { get; private set; } = Tipo_Multiline.Desconhecido;
        public double Largura { get; private set; } = 0;
        public bool Mapeado { get; set; } = false;
        public override string ToString()
        {
            return $"{Comprimento}X{Largura}X{Angulo}° St: {this.GetStyle().Name}";
        }
        public Mline Mline { get; private set; }
        public double Comprimento { get; private set; } = 0;
        public Point2d Inicio { get; private set; }
        public Point2d Fim { get; private set; }
        public Point2d Centro { get; private set; }

        public Polyline GetPLineDummy()
        {
            Polyline ps = new Polyline();
            var plane = new Plane(Point3d.Origin, Vector3d.ZAxis);
            for (int i = 0; i < this.Pontos.Count; i++)
            {
           
                ps.AddVertexAt(i, this.Pontos[i].Convert2d(plane), 0.0, 0.0, 0.0);
            }
         


            return ps;
        }

        public double Maxx
        {
            get
            {
                return Inicio.X > Fim.X ? Inicio.X : Fim.X;
            }
        }
        public double Maxy
        {
            get
            {
                return Inicio.Y > Fim.Y ? Inicio.Y : Fim.Y;
            }
        }
        public double Miny
        {
            get
            {
                return Inicio.Y < Fim.Y ? Inicio.Y : Fim.Y;
            }
        }
        public double Minx
        {
            get
            {
                return Inicio.X < Fim.X ? Inicio.X : Fim.X;
            }
        }
        public double Angulo { get; set; } = 0;
        public List<Point3d> Pontos { get; set; } = new List<Point3d>();
        public CADMline()
        {

        }
        public CADMline(Mline l, Tipo_Multiline tipo)
        {
            this.Tipo = tipo;
            Point3d p0, p1, centro;
            double comprimento, angulo,largura;
            Ut.GetCoordenadas(l, out p0, out p1, out angulo, out comprimento, out centro, out largura);
            this.Angulo = angulo;
            this.Centro = new Point2d(centro.X,centro.Y);
            this.Comprimento = Math.Round(comprimento);
            this.Mline = l;
            this.Inicio = new Point2d(p0.X,p0.Y);
            this.Fim = new Point2d(p1.X,p1.Y);
            this.Largura = Math.Round(largura);

            this.Pontos = Ut.GetPontos(this.Mline);
        }
    }
}
